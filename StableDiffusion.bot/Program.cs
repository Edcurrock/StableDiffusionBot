using Telegram.Bot;
using Telegram.Bot.Polling;
using StableDiffusionBot.Processors;
using StableDiffusionBot;
using StableDiffusion.Services.Models;
using StableDiffusion.Services.Services;
using StableDiffusion.Services.Clients;
using StableDiffusionBot.Clients;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;

services.AddSingleton(new StableDiffusionSettings
{
    ApiKey = config.GetValue<string>("StableHorde:ApiKey"),
    BaseUrl = config.GetValue<string>("StableHorde:BaseUrl")
});

services.AddHttpClient<StableDiffusionClient>(client =>
{
    client.BaseAddress = new Uri(config.GetValue<string>("StableHorde:BaseUrl") ?? "");
    client.DefaultRequestHeaders.Add("apikey", config.GetValue<string>("StableHorde:ApiKey") ?? "");
});

services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(config.GetValue<string>("ApiUrl") ?? "");
});

services.AddTransient<IAIImagesService, StableDiffusionService>();
services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config.GetValue<string>("BotKey") ?? ""));
services.AddSingleton<IUpdateHandler, TelegramUpdateHandler>();
services.AddSingleton<MessageProcessor>();
services.AddSingleton<MainProcessor>();
services.AddSingleton<TelegramBotStateMachine>();
services.AddHostedService<TelegramBotHost>();

var app = builder.Build();

app.Run();
