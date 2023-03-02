﻿using Telegram.Bot;
using StableDiffusionBot.Collections;
using StableDiffusion.Services.Services;
using StableDiffusionBot.Models;
using StableDiffusionBot.Clients;
using StableDiffusionBot.Extensions;

namespace StableDiffusionBot.Processors
{
    public class MainProcessor
    {
        private readonly TelegramBotStateMachine _botStateMachine;
        private readonly ApiClient _apiClient;
        private readonly IAIImagesService _service;
        private readonly ILogger<MainProcessor> _logger;
        public MainProcessor(TelegramBotStateMachine botStateMachine, ILogger<MainProcessor> logger, IAIImagesService service, ApiClient apiClient)
        {
            _botStateMachine = botStateMachine ?? throw new ArgumentNullException(nameof(botStateMachine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task ProcessMessage(ITelegramBotClient botClient, long chatId, string msgText, CancellationToken cancellationToken = default)
        {
            var chat = await GetChatInfo(chatId);

            ChatStatus? nextStatus = null;

            if (IsBeginState(chat, msgText))
            {
                nextStatus = await _botStateMachine.Run(ChatStatus.BEGIN.ToString(), 
                                        chatId, botClient, "");
            }
            else if (chat.Status.ToChatStatus() == ChatStatus.ADD_PROMPT)
            {
                nextStatus = await _botStateMachine.Run(ChatStatus.ADD_PROMPT.ToString(), 
                                        chatId, botClient, msgText);
            }
            else if (chat.Status.ToChatStatus() == ChatStatus.REQUEST)
            {
                nextStatus = await _botStateMachine.Run(ChatStatus.REQUEST.ToString(), 
                                        chatId, botClient, msgText);
            }
            else if (chat.Status.ToChatStatus() == ChatStatus.PROCESSING)
            {
                nextStatus = await _botStateMachine.Run(ChatStatus.PROCESSING.ToString(), 
                                        chatId, botClient, msgText);
            }
            else if (chat.Status.ToChatStatus() == ChatStatus.CHOOSE_LAST_WAY)
            {
                nextStatus = await _botStateMachine.Run(ChatStatus.CHOOSE_LAST_WAY.ToString(),
                                        chatId, botClient, msgText);
            }
            else
            {
                nextStatus = await _botStateMachine.Run(msgText, chatId, botClient, "");
            }

            if (nextStatus.HasValue)
            {
                await UpdateChat(chatId, nextStatus.Value);
            }
        }

        private async Task UpdateChat(long chatId, ChatStatus? status)
        {
            var statusName = status.HasValue? status.ToString() : string.Empty;

            await _apiClient.ChatsPUTAsync(chatId, new UpdateChatStatusModel 
            {
                Status = statusName,
            });
        }

        private bool IsBeginState(ChatInfoResponse? chat, string msgText) // TODO проверка на начальное состояние
        {
            if (chat is null)
            {
                return true;
            }

            var isBeginStatus = chat.Status.ToChatStatus() == ChatStatus.BEGIN ||
                                    msgText.ToLowerInvariant() == ChatCommandsCollection.BEGIN;

            var isCancelStatus = msgText.ToLowerInvariant() == ChatCommandsCollection.CANCEL;

            var isRetry = msgText == ChatCommandsCollection.RETRY;

            return isBeginStatus || isCancelStatus || isRetry;
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
    }
}
