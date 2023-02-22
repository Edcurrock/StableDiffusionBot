using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Repositories.Models
{
    public class Chat : Entity
    {
        public long ChatId { get; set; }
        public ChatStatus Status { get; set; }
        public string? LastGeneratedId { get; set; } // TODO убрать
    }

    public enum ChatStatus //TODO сделать через settings настройку prompt и тд
    {
        BEGIN = 0,
        ADD_PROMPT = 1,
        //ADD_IMAGE = 2, 
        REQUEST = 2,
        PROCESSING = 3,
        SETTINGS = 4
    }
}
