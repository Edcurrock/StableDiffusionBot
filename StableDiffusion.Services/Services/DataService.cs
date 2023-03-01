using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StableDiffusion.Repositories;
using StableDiffusion.Repositories.Models;

namespace StableDiffusion.Services.Services
{
    public class DataService : IDataService
    {
        private readonly DataContext _context;
        private readonly ILogger<DataService> _logger;

        public DataService(DataContext context, ILogger<DataService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CreateChatAsync(CreateChatCommand command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var chat = new Chat
            {
                ChatId = command.ChatId
            };

            var settings = new UserSettings
            {
                Chat = chat
            };

            try
            {
                await _context.Chats.AddAsync(chat);
                await _context.Settings.AddAsync(settings);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Attempt to add chat {command.ChatId} has failed. Internal error");
                command.AddError(DataServiceError.Internal);
            }
        }

        public async Task EditChatSettingsAsync(EditChatSettingsCommand command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.ChatId == command.ChatId);

            if (chat is null) //TODO сделать лучше
            {
                _logger.LogError($"Attempt to edit settings for chat {command.ChatId} has failed. Chat not found");
                command.AddError(DataServiceError.ChatNotFound);
                return;
            }

            var setting = await _context.Settings.FirstOrDefaultAsync(x => x.ChatId == chat.Id);

            if (setting is null)
            {
                _logger.LogWarning($"Attempt to edit chat settings {command.ChatId} has failed. Settings not found");
                command.AddError(DataServiceError.SettingsNotFound);
                return;
            }

            setting.Prompt = command.Prompt?.Trim();

            if (command.Width.HasValue)
            {
                setting.Width = command.Width.Value;
            }

            if (command.Height.HasValue)
            {
                setting.Height = command.Height.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Attempt to add chat {command.ChatId} has failed. Internal error");
                command.AddError(DataServiceError.Internal);
            }
        }

        public async Task EditChatAsync(EditChatCommand command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.ChatId == command.ChatId);

            if (chat is null)
            {
                _logger.LogError($"Attempt to edit chat status {command.ChatId} has failed. Chat not found");
                command.AddError(DataServiceError.ChatNotFound);
                return;
            }

            if (!string.IsNullOrWhiteSpace(command.Status))
            {
                chat.Status = command.Status;
            }

            if (!string.IsNullOrWhiteSpace(command.GeneratedImageId))
            {
                chat.LastGeneratedId = command.GeneratedImageId.Trim();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Attempt to add chat {command.ChatId} has failed. Internal error");
                command.AddError(DataServiceError.Internal);
            }
        }

        public async Task<ChatInfo?> GetChatInfoAsync(long chatId)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.ChatId == chatId);

            if (chat is null)
            {
                _logger.LogError($"Attempt to get chat info {chatId} has failed. Chat not found");
                return null;
            }

            var settings = await _context.Settings.FirstOrDefaultAsync(x => x.ChatId == chat.Id);

            if (settings is null)
            {
                _logger.LogError($"Attempt to get chat info {chatId} has failed. User settings not found");
                return null;
            }

            return new ChatInfo
            {
                Status = chat.Status,
                Prompt = settings.Prompt,
                Height = settings.Height,
                Width = settings.Width,
                LastGeneratedId = chat.LastGeneratedId
            };
        }


        public enum DataServiceError
        {
            Undefined = 0,
            Internal = 1,
            ChatNotFound = 2,
            SettingsNotFound = 3,
        }
    }
}
