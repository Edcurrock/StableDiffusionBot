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
        public string Status { get; set; } = "BEGIN";
        public string? LastGeneratedId { get; set; } // TODO убрать
    }
}
