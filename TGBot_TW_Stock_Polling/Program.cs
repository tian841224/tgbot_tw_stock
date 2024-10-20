using NLog;
using NLog.Extensions.Logging;
using Telegram.Bot;
using TGBot_TW_Stock_Polling.Interface;
using TGBot_TW_Stock_Polling.Services;


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
var logger = LogManager.GetCurrentClassLogger();

logger.Info("啟動程式");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 設定NLog
    builder.Services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        logging.AddNLog();
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // 設定TelegramBotClient
    var apikey = builder.Configuration["BotConfiguration:BotToken"];
    builder.Services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                TelegramBotClientOptions options = new(apikey);
                return new TelegramBotClient(options, httpClient);
            });
    //builder.Services.AddHostedService<InitService>();
    builder.Services.AddHostedService<PollingService>();
    builder.Services.AddSingleton<UpdateHandler>();
    builder.Services.AddSingleton<ReceiverService>();
    builder.Services.AddTransient<IBrowserHandlers, BrowserHandlers>();
    builder.Services.AddTransient<IBotService, BotService>();
    builder.Services.AddTransient<Lazy<TradingView>>();
    builder.Services.AddTransient<Lazy<Cnyes>>();

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