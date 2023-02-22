using Microsoft.EntityFrameworkCore;
using StableDiffusion.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Repositories
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserSettings> Settings { get; set; }
    }
}
