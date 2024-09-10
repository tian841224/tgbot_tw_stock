using NLog;
using NLog.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Examples.WebHook.Services;
using Telegram.Bot.Services;
using TGBot_TW_Stock_Polling.Interface;


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
var logger = LogManager.GetCurrentClassLogger();

logger.Info("�Ұʵ{��");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var apikey = builder.Configuration["BotConfiguration:BotToken"];
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                TelegramBotClientOptions options = new(apikey);
                return new TelegramBotClient(options, httpClient);
            });
    //builder.Services.AddHostedService<InitService>();
    builder.Services.AddScoped<IBrowserHandlers, BrowserHandlers>();
    builder.Services.AddScoped<TradingView>();
    builder.Services.AddScoped<Cnyes>();
    builder.Services.AddScoped<UpdateHandler>();
    builder.Services.AddScoped<ReceiverService>();
    builder.Services.AddHostedService<PollingService>();

    var app = builder.Build();

    app.MapGet("/", () => "Hello World!");

    app.Run();
}


catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}