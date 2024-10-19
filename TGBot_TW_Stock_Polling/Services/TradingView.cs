using Microsoft.Playwright;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGBot_TW_Stock_Polling.Interface;

namespace Telegram.Bot.Examples.WebHook.Services
{
    /// <summary>
    /// TradingView
    /// </summary>
    public class TradingView
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TradingView> _logger;
        private readonly IBrowserHandlers _browserHandlers;

        public TradingView(ITelegramBotClient botClient, ILogger<TradingView> logger, IBrowserHandlers browserHandlers)
        {
            _botClient = botClient;
            _logger = logger;
            _browserHandlers = browserHandlers;
        }

        /// <summary>
        /// 載入網頁
        /// </summary>
        /// <param name="stockNumber"></param>
        /// <returns></returns>
        public async Task<IPage> LoadUrl(string stockNumber)
        {
            try
            {
                var url = $"https://www.cnyes.com/twstock/{stockNumber}";
                var page = await _browserHandlers.LoadUrl(url);

                return page;
            }
            catch (Exception ex)
            {
                _logger.LogError($"載入網頁時發生錯誤: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 錯誤通知
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ErrorNotify(Message message, string errorMessage, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(
                text: $"UserId：{message.Chat.Id}\n" +
                $"Username：{message.Chat.Username}\n" +
                $"錯誤：\n {errorMessage}",
                chatId: 806077724,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
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
            try
            {
                //載入網頁
                var page = await LoadUrl(stockNumber);

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
            }
            catch (Exception ex)
            {
                _logger.LogError("GetChartAsync：" + ex.Message);
                await ErrorNotify(message,"GetChartAsync：" + ex.Message, cancellationToken);
            }
            finally
            {
                await _browserHandlers.ReleaseBrowser();
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
            try
            {
                //載入網頁
                var page = await LoadUrl(stockNumber);

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
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRangeAsync：" + ex.Message);
                await ErrorNotify(message, "GetRangeAsync：" + ex.Message, cancellationToken);
            }
            finally
            {
                await _browserHandlers.ReleaseBrowser();
            }
        }
    }
}
