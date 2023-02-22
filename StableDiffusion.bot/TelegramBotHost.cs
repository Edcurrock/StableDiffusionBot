using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace StableDiffusionBot
{
    public class TelegramBotHost : BackgroundService
    {
        private readonly IUpdateHandler _updateHandler;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger _logger;

        public TelegramBotHost(IUpdateHandler updateHandler, ITelegramBotClient bot, ILogger<TelegramBotHost> logger)
        {
            _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram Bot host has started. Preparing to receive messages");

            try
            {
                await _bot.ReceiveAsync(_updateHandler, null, stoppingToken);
            }
            catch (Exception exception)
            {
                //await _updateHandler.HandleErrorAsync(_bot, exception, stoppingToken);
            }
        }
    }

}
