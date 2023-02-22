using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Repositories.Models
{
    public class UserSettings : Entity
    {
        public long ChatId { get; set; }
        [ForeignKey(nameof(ChatId))]
        public virtual Chat? Chat { get; set; }

        public string? Prompt { get; set; }
        public int Height { get; set; } = 512;
        public int Width { get; set; } = 512;

        //TODO добавить остальные параметры
    }
}
