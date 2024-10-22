using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot_TW_Stock_Polling.Interface;

namespace TGBot_TW_Stock_Polling.Services
{
    /// <summary>
    /// TradingView
    /// </summary>
    public class TradingView
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TradingView> _logger;
        private readonly IBrowserHandlers _browserHandlers;
        private readonly IBotService _botService;
        private string stockUrl = "https://tw.tradingview.com/chart/?symbol=TWSE%3A";

        public TradingView(ITelegramBotClient botClient, ILogger<TradingView> logger, IBrowserHandlers browserHandlers, IBotService botService)
        {
            _botClient = botClient;
            _logger = logger;
            _browserHandlers = browserHandlers;
            _botService = botService;
        }

        /// <summary>
        /// 查詢走勢(日K)
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetChartAsync(string stockNumber, Message message, CancellationToken cancellationToken)
        {
            const int maxRetries = 3; // 最大重試次數
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromSeconds(3); // 每次重試的延遲時間
            while (true)
            {
                try
                {
                    //載入網頁
                    var page = await _browserHandlers.LoadUrlAsync(stockUrl + stockNumber);

                    //等待元素載入
                    await page.WaitForSelectorAsync("//div[@class= 'chart-markup-table']");

                    _logger.LogInformation("擷取網站中...");

                    Stream stream = new MemoryStream(await page.Locator("//div[@class= 'chart-markup-table']").ScreenshotAsync());

                    await _botClient.SendPhotoAsync(
                       chatId: message.Chat.Id,
                       photo: InputFile.FromStream(stream),
                       parseMode: ParseMode.Html,
                       cancellationToken: cancellationToken);
                    _logger.LogInformation("已傳送資訊");

                    // 成功後跳出迴圈
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogInformation($"GetChartAsync 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetChartAsync 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetChartAsync 失敗：{ex.Message}");
                    }

                    _logger.LogInformation($"等待 {delay.TotalSeconds} 秒後重試...");
                    await Task.Delay(delay, cancellationToken);
                }
                finally
                {
                    await _browserHandlers.ReleaseBrowser();
                }
            }
        }

        /// <summary>
        /// ️指定圖表顯示時間範圍
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="chatID">使用者ID</param>
        /// <param name="input">使用者輸入參數</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetRangeAsync(string stockNumber, Message message, string? input, CancellationToken cancellationToken)
        {
            const int maxRetries = 3; // 最大重試次數
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromSeconds(3); // 每次重試的延遲時間
            while (true)
            {
                try
                {
                    //載入網頁
                    var page = await _browserHandlers.LoadUrlAsync(stockUrl + stockNumber);

                    string range;

                    #region
                    switch (input)
                    {
                        case "1d":
                            range = "1D";
                            break;
                        case "5d":
                            range = "5D";
                            break;
                        case "1m":
                            range = "1M";
                            break;
                        case "3m":
                            range = "3M";
                            break;
                        case "6m":
                            range = "6M";
                            break;
                        case "ytd":
                            range = "YTD";
                            break;
                        case "1y":
                            range = "12M";
                            break;
                        case "5y":
                            range = "60M";
                            break;
                        case "all":
                            range = "ALL";
                            break;
                        default:
                            range = "YTD";
                            break;
                    }
                    await page.Locator($"//button[@value = '{range}']").ClickAsync().WaitAsync(new TimeSpan(0, 1, 0));

                    _logger.LogInformation("等待元素載入...");
                    //等待元素載入
                    await page.WaitForSelectorAsync("//div[@class= 'chart-markup-table']");

                    _logger.LogInformation("擷取網站中...");
                    Stream stream = new MemoryStream(await page.Locator("//div[@class= 'chart-markup-table']").ScreenshotAsync());
                    await _botClient.SendPhotoAsync(
                       chatId: message.Chat.Id,
                       photo: InputFile.FromStream(stream),
                       parseMode: ParseMode.Html,
                       cancellationToken: cancellationToken);
                    _logger.LogInformation("已傳送資訊");
                    #endregion

                    // 成功後跳出迴圈
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogInformation($"GetRangeAsync 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetRangeAsync 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetRangeAsync 失敗：{ex.Message}");
                    }

                    _logger.LogInformation($"等待 {delay.TotalSeconds} 秒後重試...");
                    await Task.Delay(delay, cancellationToken);
                }
                finally
                {
                    await _browserHandlers.ReleaseBrowser();
                }
            }
        }
    }
}
