using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
//using StableDiffusionBot.Clients;
using StableDiffusionBot.Processors;
using Telegram.Bot.Polling;

namespace StableDiffusionBot
{
    public class TelegramUpdateHandler : IUpdateHandler
    {
        private readonly MainProcessor _botProcessor;
        private readonly ILogger<TelegramUpdateHandler> _logger;

        public TelegramUpdateHandler(ILogger<TelegramUpdateHandler> logger, MainProcessor botProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _botProcessor = botProcessor ?? throw new ArgumentNullException(nameof(botProcessor));
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(exception, errorMessage);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (!ContainsChatId(update))
            {
                return;
            }

            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message, cancellationToken),
                //UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery, cancellationToken),
                _ => throw new NotImplementedException(),
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private bool ContainsChatId(Update update)
        {
            long chatId = -1;

            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
            }

            if (chatId == -1)
            {
                return false;
            }

            return true;
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Receive message type: {message.Type}");

            try
            {
                var msgText = string.Empty;

                if (message.Type == MessageType.Text)
                {
                    msgText = message.Text.Trim();
                } 
                else if (message.Type == MessageType.Photo)
                {
                    var image = message.Photo;
                    msgText = await GetImageAsBase64Async(botClient, image);
                }

                if (string.IsNullOrWhiteSpace(msgText))
                {
                    return;
                }

                var chatId = message.Chat.Id;
                await _botProcessor.ProcessMessage(botClient, chatId, msgText, cancellationToken);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in <ProcessMessage>");
            }
      
        }

        //private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery? query, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation($"Recieve Inline message!");

        //    try
        //    {
        //        var chatId = query.From.Id;
        //        await _botProcessor.ProcessMessage(botClient, chatId, query.Data, cancellationToken);
        //    }
        //    catch (ApiException e)
        //    {
        //        _logger.LogError(e, "Error in <ProcessMessage>");
        //    }
        //}

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<string?> GetImageAsBase64Async(ITelegramBotClient botClient, PhotoSize[]? image)
        {
            var maximumImage = image!.FirstOrDefault(file => file.FileId
                                        .Equals(image!.MaxBy(data => data.FileSize)!.FileId));
            if (maximumImage != null)
            {
                using var memoryStream = new MemoryStream((int)maximumImage.FileSize.GetValueOrDefault());
                var file = await botClient.GetInfoAndDownloadFileAsync(maximumImage.FileId, memoryStream);
                // reset position
                memoryStream.Position = 0;
                var dataBytes = memoryStream.ToArray();
                return Convert.ToBase64String(dataBytes);
            }

            return null;
        }
    }
}
