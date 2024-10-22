using Microsoft.Playwright;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TGBot_TW_Stock_Polling.Interface;

namespace TGBot_TW_Stock_Polling.Services
{
    /// <summary>
    /// 鉅亨網
    /// </summary>
    public class Cnyes
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<Cnyes> _logger;
        private readonly IBrowserHandlers _browserHandlers;
        private readonly IBotService _botService;
        private string stockUrl = "https://www.cnyes.com/twstock/";


        public Cnyes(ITelegramBotClient botClient, ILogger<Cnyes> logger, IBrowserHandlers browserHandlers, IBotService botService)
        {
            _botClient = botClient;
            _logger = logger;
            _browserHandlers = browserHandlers;
            _botService = botService;
        }

        /// <summary>
        /// 取得K線
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="input">使用者輸入參數</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetKlineAsync(string stockNumber, Message message, string? input, CancellationToken cancellationToken)
        {
            const int maxRetries = 3; // 最大重試次數
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromSeconds(3); // 每次重試的延遲時間

            while (true)
            {
                try
                {
                    // 載入網頁
                    var page = await _browserHandlers.LoadUrlAsync(stockUrl + stockNumber);

                    // 等待圖表載入
                    await page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[2]//div//div[2]//div[1]//div//div//div//div[2]//table")
                        .WaitAsync(new TimeSpan(0, 1, 0));

                    // 拆解元素
                    var element = await page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
                    if (element == null) throw new Exception("找不到指定元素");
                    var textContent = await element.EvaluateAsync<string>("node => node.innerText");

                    // 股票名稱
                    var stockName = textContent.Split("\n").FirstOrDefault() ?? "未知股票";

                    // 點擊按鈕
                    await page.GetByRole(AriaRole.Button, new() { Name = input, Exact = true, }).ClickAsync();
                    await page.WaitForTimeoutAsync(1500);

                    // 圖表
                    _logger.LogInformation("擷取網站中...");
                    using Stream stream = new MemoryStream(await page.Locator("//div[@class= 'jsx-3777377768 tradingview-chart']").ScreenshotAsync());
                    await _botClient.SendPhotoAsync(
                        caption: $"{stockName}：{input}線圖　💹",
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
                    _logger.LogInformation($"GetKlineAsync 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetKlineAsync 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                            text: "因機器人部屬於國外，有時會無法讀取網頁，請將程式部屬至本機執行。",
                            chatId: message.Chat.Id,
                            parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetKlineAsync 失敗：{ex.Message}");
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
        /// 取得詳細報價
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetDetialPriceAsync(string stockNumber, Message message, CancellationToken cancellationToken)
        {
            const int maxRetries = 3; // 最大重試次數
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromSeconds(3); // 每次重試的延遲時間

            while (true)
            {
                try
                {
                    // 載入網頁
                    var page = await _browserHandlers.LoadUrlAsync(stockUrl + stockNumber);

                    // 股價資訊字典
                    var InfoDic = new Dictionary<int, string>()
                    {
                        { 1, "開盤價"},{ 2, "最高價"},{ 3, "成交量"},
                        { 4, "昨日收盤價"},{ 5, "最低價"},{ 6, "成交額"},
                        { 7, "均價"},{ 8, "本益比"},{ 9, "市值"},
                        { 10, "振幅"},{ 11, "迴轉率"},{ 12, "發行股"},
                        { 13, "漲停"},{ 14, "52W高"},{ 15, "內盤量"},
                        { 16, "跌停"},{ 17, "52W低"},{ 18, "外盤量"},
                        { 19, "近四季EPS"},{ 20, "當季EPS"},{ 21, "毛利率"},
                        { 22, "每股淨值"},{ 23, "本淨比"},{ 24, "營利率"},
                        { 25, "年股利"},{ 26, "殖利率"},{ 27, "淨利率"},
                    };

                    // 等待圖表載入
                    await page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[2]//div//div[2]//div[1]//div//div//div//div[2]//table")
                              .WaitAsync(new TimeSpan(0, 1, 0));
                    await page.WaitForTimeoutAsync(1500);

                    _logger.LogInformation("處理相關資料...");
                    // 拆解元素
                    var element = await page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
                    if (element == null) throw new Exception("找不到指定元素");
                    var textContent = await element.EvaluateAsync<string>("node => node.innerText");

                    // 股票名稱
                    var stockName = textContent.Split("\n").ToList()[0];

                    // 詳細報價
                    var temp_returnStockUD = await page.QuerySelectorAllAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[4]//div[2]");
                    if (temp_returnStockUD == null || temp_returnStockUD.Count == 0)
                        throw new Exception("找不到詳細報價的元素");

                    var returnStockUD = await temp_returnStockUD[0].InnerTextAsync();
                    var StockUD_List = returnStockUD.Split("\n");

                    // 股價相關信息
                    var stock_price = await page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//h3");
                    var stock_change_price = await page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//div//div[1]//span[1]");
                    var stock_amplitude = await page.TextContentAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//div//div//div[1]//span[2]");

                    // 選擇輸出欄位
                    var output = new int[] { 1, 2, 5 };

                    StringBuilder chart = new StringBuilder();
                    chart.AppendLine(@$"<b>-{stockName}-📝</b>");
                    chart.AppendLine(@$"<code>收盤價：{stock_price}</code>");
                    chart.AppendLine(@$"<code>漲跌幅：{stock_change_price}</code>");
                    chart.AppendLine(@$"<code>漲跌%：{stock_amplitude}</code>");

                    foreach (var i in output)
                    {
                        if (i * 2 - 1 < StockUD_List.Length)
                        {
                            chart.AppendLine(@$"<code>{InfoDic[i]}：{StockUD_List[i * 2 - 1]}</code>");
                        }
                        else
                        {
                            _logger.LogWarning($"索引 {i * 2 - 1} 超出 StockUD_List 範圍。");
                        }
                    }

                    // 圖表
                    _logger.LogInformation("擷取網站中...");
                    using Stream stream = new MemoryStream(await page.Locator("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]").First.ScreenshotAsync());
                    await _botClient.SendPhotoAsync(
                        caption: chart.ToString(),
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
                    _logger.LogInformation($"GetDetialPriceAsync 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetDetialPriceAsync 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetDetialPriceAsync 失敗：{ex.Message}");
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
        /// 取得績效
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetPerformanceAsync(string stockNumber, Message message, CancellationToken cancellationToken)
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

                    //點選cookie提示按鈕
                    var cookiebutton = await page.QuerySelectorAsync("#__next > div._1GCLL > div > button._122qv");
                    if (cookiebutton != null)
                        await cookiebutton.ClickAsync();

                    //滾動網頁至最下方，觸發js
                    await page.EvaluateAsync(@"() => {
                        window.scrollTo({
                            top: document.body.scrollHeight,
                            behavior: 'smooth'
                        });
                        }");

                    await page.WaitForTimeoutAsync(3000);

                    //等待圖表載入
                    await page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[3]//section//div[2]//section//div[2]//div[1]//div//div[2]//div").WaitAsync(new TimeSpan(0, 1, 0));
                    //等待數據載入
                    await page.WaitForSelectorAsync("//html//body//div[1]//div[1]//div[4]//div[3]//section//div[2]//section//div[2]//div[2]//div//div//table").WaitAsync(new TimeSpan(0, 1, 0));


                    //拆解元素
                    var element = await page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
                    if (element == null) throw new Exception("找不到指定元素");
                    var textContent = await element.EvaluateAsync<string>("node => node.innerText");

                    //股票名稱
                    var stockName = textContent.Split("\n").ToList()[0];

                    //股價
                    var price = await page.Locator("//*[@id=\"tw-stock-tabs\"]//div[2]//section").First.ScreenshotAsync();

                    _logger.LogInformation("擷取網站中...");
                    var stream = new MemoryStream(price);
                    await _botClient.SendPhotoAsync(
                        caption: $"{stockName} 績效表現　✨",
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
                    _logger.LogInformation($"GetPerformanceAsync： 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetPerformanceAsync： 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetPerformanceAsync： 失敗：{ex.Message}");
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
        /// 取得新聞
        /// </summary>
        /// <param name="stockNumber">股票代號</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task GetNewsAsync(string stockNumber, Message message, CancellationToken cancellationToken)
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

                    //拆解元素
                    var element = await page.QuerySelectorAsync("//html//body//div[1]//div[1]//div[4]//div[2]//div[1]//div[1]//div[1]//div//div[2]//div[2]//h2");
                    if (element == null) throw new Exception("找不到指定元素");
                    var textContent = await element.EvaluateAsync<string>("node => node.innerText");

                    //股票名稱
                    var stockName = textContent.Split("\n").ToList()[0];
                    //定位新聞版塊
                    var newsList = await page.QuerySelectorAsync("//div[contains(@class, 'news-notice-container-summary')]");
                    if (newsList == null) throw new Exception("找不到指定元素");
                    var newsContent = await newsList.QuerySelectorAllAsync("//a[contains(@class, 'container shadow')]");
                    if (newsContent == null) throw new Exception("找不到指定元素");

                    var InlineList = new List<IEnumerable<InlineKeyboardButton>>();
                    for (int i = 0; i < 5; i++)
                    {
                        if (newsContent[i] == null) continue;
                        var text = await newsContent[i].TextContentAsync() ?? string.Empty;
                        var url = await newsContent[i].GetAttributeAsync("href") ?? string.Empty;
                        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(text)) continue;
                        InlineList.Add(new[] { InlineKeyboardButton.WithUrl(text, url) });
                    }

                    InlineKeyboardMarkup inlineKeyboard = new(InlineList);
                    var s = inlineKeyboard.InlineKeyboard;
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: @$"⚡️{stockName}-即時新聞",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                    _logger.LogInformation("已傳送資訊");

                    // 成功後跳出迴圈
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogInformation($"GetNewsAsync 嘗試 {retryCount} 次失敗：{ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"GetNewsAsync 已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");
                        throw new Exception($"GetNewsAsync 失敗：{ex.Message}");
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
