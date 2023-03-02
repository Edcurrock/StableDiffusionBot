using Telegram.Bot;
using Telegram.Bot.Types;
using StableDiffusionBot.Collections;
using StableDiffusionBot.Models;
using StableDiffusionBot.Utility;
using StableDiffusion.Services.Services;
using StableDiffusionBot.Clients;

namespace StableDiffusionBot.Processors
{
    public interface IMessageProcessor
    {
        public Task<ProcessResponse> Process(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default);
    }

    public class MessageProcessor
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IAIImagesService _service;
        public MessageProcessor(ILogger<MessageProcessor> logger, IAIImagesService service, ApiClient apiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<ChatStatus?> Begin(long chatId, ITelegramBotClient botClient, object _, CancellationToken cancellationToken = default)
        {
            var chat = await GetChatInfo(chatId);

            if (chat is null)
            {
                var isCreated = await TryCreateNewChatInfo(chatId);

                if (!isCreated)
                {
                    await botClient.SendTextMessageAsync(chatId,
                        "Произошла ошибка. Пожалуйста, повторите или попробуйте через некоторое время");
                    return null;
                }
            }

            await botClient.SendTextMessageAsync(chatId,
                "Для того, чтобы начать генерацию изображения, введите набор ключевых слов на английском");

            return ChatStatus.ADD_PROMPT;
        }

        public async Task<ChatStatus?> AddPrompt(long chatId, ITelegramBotClient botClient, object prompt, CancellationToken cancellationToken = default)
        {
            var promptString = TypeHelper.GetTypedValue<string>(prompt);
            var settingsModel = new UpdateUserSettingsModel
            {
                Prompt = promptString
            };

            var isSettingsUpdated = await TryUpdateUserSettings(chatId, settingsModel);

            if (!isSettingsUpdated)
            {
                await botClient.SendTextMessageAsync(chatId,
                        "Произошла ошибка. Пожалуйста, повторите или попробуйте через некоторое время");
                return null;
            }

            await botClient.SendTextMessageAsync(chatId,
                  "Вы также можете прислать изображение, на основе которого будет выполнена генерация");
            await botClient.SendTextMessageAsync(chatId,
                  "Пришлите фото (не в виде документа) либо пропустите этот пункт");

            await ChatKeyboardsCollection.SendImageWayKeyboard(botClient, chatId);

            return ChatStatus.REQUEST;
        }

        public async Task<ChatStatus?> DoRequest(long chatId, ITelegramBotClient botClient, object img, CancellationToken cancellationToken = default)
        {
            await botClient.SendTextMessageAsync(chatId,
                   "Запрос обрабатывается...", replyMarkup: new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove());

            var chat = await GetChatInfo(chatId);

            if (chat is null)
            {
                await botClient.SendTextMessageAsync(chatId,
                      "Произошла ошибка. Пожалуйста, повторите действия");
                await ChatKeyboardsCollection.SendMenuKeyboard(botClient, chatId);
                return ChatStatus.BEGIN;
            }

            var sourceImg = TypeHelper.GetTypedValue<string>(img);

            var result = await _service.GenerateImageAsync(new GenerateImageParams
            {
                Prompt = chat.Prompt,
                SourceImg = sourceImg == ChatCommandsCollection.GET_IMAGE_WITHOUT_SOURCE_IMG ? 
                                                                                    null : sourceImg,
            });

            if (string.IsNullOrWhiteSpace(result.Id))
            {
                await botClient.SendTextMessageAsync(chatId,
                   "Попробуйте заново. Что-то пошло не так :(");
                await ChatKeyboardsCollection.SendMenuKeyboard(botClient, chatId);
                return ChatStatus.BEGIN;
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId,
                   "Запрос выполнен! Скоро мы пришлем Ваше изображение :)");
            }

            //_logger.LogInformation($"Id of created image: ${result.Id}");

            var processResult = await ProcessImage(chatId, botClient, result.Id);

            return processResult;
        }

        //public async Task<ChatStatus?> Process(long chatId, ITelegramBotClient botClient, object request, CancellationToken cancellationToken = default)
        //{
        //    var chat = await GetChatInfo(chatId);

        //    if (chat is null)
        //    {
        //        await botClient.SendTextMessageAsync(chatId,
        //              "Произошла ошибка. Пожалуйста, повторите действия");
        //        await botClient.SendTextMessageAsync(chatId,
        //            $"Введите {ChatCommandsCollection.BEGIN}, чтобы начать");
        //        return ChatStatus.BEGIN;
        //    }

        //    var result = await _service.GetImageAsync(chat.LastGeneratedId);

        //    if (result.Status == GetImageStatus.Processing)
        //    {
        //        await botClient.SendTextMessageAsync(chatId,
        //                "Идет генерация, пожалуйста подождите");
        //        return null; // TODO изменить логику 
        //    }

        //    if (result.Status == GetImageStatus.Failed)
        //    {
        //        await botClient.SendTextMessageAsync(chatId,
        //            "Ошибка! Попробуйте заново!");
        //        return ChatStatus.BEGIN; // TODO изменить логику 
        //    }

        //    if (result.Status != GetImageStatus.Ready)
        //    {
        //        return null;
        //    }

        //    using (var ms = new MemoryStream(result.Img))
        //    {
        //        var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(ms, "generated.png");
        //        await botClient.SendPhotoAsync(chatId, iof);
        //    }

        //    await ChatKeyboardsCollection.SendMenuKeyboard(botClient, chatId, true);

        //    return ChatStatus.CHOOSE_LAST_WAY; // TODO изменить логику 
        //}

        public async Task<ChatStatus?> ChooseLastWay(long chatId, ITelegramBotClient botClient, object way, CancellationToken cancellationToken = default)
        {
            var wayString = TypeHelper.GetTypedValue<string>(way);

            if (wayString == ChatCommandsCollection.SAME_PROMPT)
            {
                await ChatKeyboardsCollection.SendImageWayKeyboard(botClient, chatId);
                return ChatStatus.REQUEST;
            }

            return ChatStatus.BEGIN;
        }

        private async Task<ChatStatus?> ProcessImage(long chatId, ITelegramBotClient botClient, string lastGeneratedId)
        {
            var result = await _service.GetImageAsync(lastGeneratedId);

            if (result.Status == GetImageStatus.Failed)
            {
                await botClient.SendTextMessageAsync(chatId,
                    "Ошибка! Попробуйте заново!");
                await ChatKeyboardsCollection.SendMenuKeyboard(botClient, chatId);
                return ChatStatus.BEGIN; // TODO изменить логику 
            }

            if (result.Status != GetImageStatus.Ready)
            {
                return null;
            }

            using (var ms = new MemoryStream(result.Img))
            {
                var iof = new Telegram.Bot.Types.InputFiles.InputOnlineFile(ms, "generated.png");
                await botClient.SendPhotoAsync(chatId, iof);
            }

            await ChatKeyboardsCollection.SendMenuKeyboard(botClient, chatId, samePrompt: true);

            return ChatStatus.CHOOSE_LAST_WAY; // TODO изменить логику 
        }



        private async Task<ChatInfoResponse?> GetChatInfo(long chatId)
        {
            try
            {
                var chat = await _apiClient.ChatsGETAsync(chatId);
                return chat;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> TryCreateNewChatInfo(long chatId)
        {
            try
            {
                await _apiClient.ChatsPOSTAsync(chatId); // TODO попроавить по клиенту
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"attempt to create chat info with chatId {chatId} has failed");
                return false;
            }
        }

        //TODO try update user settings
        private async Task<bool> TryUpdateUserSettings(long chatId, UpdateUserSettingsModel model)
        {
            try
            {
                await _apiClient.SettingsAsync(chatId, model);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"attempt to create chat info with chatId {chatId} has failed");
                return false;
            }
        }
    }

    public enum ProcessStatus
    {
        Ok = 0,
        Error = 1
    }

    public class ProcessResponse
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
    }
}
