using StableDiffusion.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Services.Services
{
    public interface IDataService
    {
        Task CreateChatAsync(CreateChatCommand command);
        Task EditChatAsync(EditChatCommand command);
        Task<ChatInfo?> GetChatInfoAsync(long chatId);
        Task EditChatSettingsAsync(EditChatSettingsCommand command);
    }

    public class CreateChatCommand : Command
    {
        public long ChatId { get; set; }
    }

    public class EditChatCommand : Command
    {
        public long ChatId { get; set; }
        public string? Status { get; set; }
        public string? GeneratedImageId { get; set; }
    }

    public class EditChatSettingsCommand : Command
    {
        public long ChatId { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public string? Prompt { get; set; }
    }

    public class ChatInfo
    { 
        public string Status { get; set; }
        public string? Prompt { get; set; }
        public string? LastGeneratedId { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
