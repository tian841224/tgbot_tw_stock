using Microsoft.Playwright;
using TGBot_TW_Stock_Polling.Interface;

namespace Telegram.Bot.Examples.WebHook.Services
{
    public class BrowserHandlers : IBrowserHandlers
    {
        private readonly ILogger<BrowserHandlers> _logger;
        private IPlaywright? _playwright = null;
        private IBrowser? _browser = null;
        private IPage? _page = null;
        //private DateTime _lastAccessTime;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);


        public BrowserHandlers(ILogger<BrowserHandlers> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 取得頁面
        /// </summary>
        /// <returns></returns>
        public async Task<IPage> GetPageAsync()
        {

            if (_playwright == null || _browser == null || _page == null) await CreateBrowser();

            if (_page == null) throw new Exception("初始化Page錯誤");

            return _page;
        }

        /// <summary>
        /// 釋放瀏覽器流程
        /// </summary>
        /// <returns></returns>
        public async Task ReleaseBrowser()
        {
            await ClosePage();
            await CloseBrowser();
            ClosePlaywright();
        }

        /// <summary>
        /// 載入網頁
        /// </summary>
        /// <param name="stockNumber"></param>
        /// <returns></returns>
        public async Task<IPage> LoadUrl(string url)
        {
            try
            {
                var page = await GetPageAsync();
                await page.GotoAsync($"{url}",
                            new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60000 });

                _logger.LogInformation("等待元素載入...");
                return page;
            }
            catch (Exception ex)
            {
                _logger.LogError($"載入網頁時發生錯誤: {ex.Message}");
                throw new Exception($"LoadUrl : {ex.Message}");
            }
        }

        /// <summary>
        /// 啟動瀏覽器流程
        /// </summary>
        /// <returns></returns>
        private async Task CreateBrowser()
        {
            try
            {
                await LunchesPlaywright();
                await SettingBrowser();
                await SettingPage();
            }
            catch(Exception ex)
            {
                throw new Exception($"CreateBrowser : {ex.Message}");
            }
        }

        /// <summary>
        /// 啟動套件
        /// </summary>
        /// <returns></returns>
        public async Task LunchesPlaywright()
        {
            if (_playwright == null)
            {
                _playwright = await Playwright.CreateAsync();
            }
        }

        /// <summary>
        /// 設定瀏覽器
        /// </summary>
        /// <returns></returns>
        private async Task SettingBrowser()
        {
            try
            {
                _logger.LogInformation($"設定瀏覽器");

                if (_playwright == null) await LunchesPlaywright();

                if (_playwright != null && _browser == null)
                {
                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        //路徑會依瀏覽器版本不同有差異，若有錯時請修正路徑
                        //使用docker執行時須使用下面參數，本機直接執行則不用
                        //ExecutablePath = "/root/.cache/ms-playwright/chromium-1055/chrome-linux/chrome",
                        Args = new[] {
                            "--disable-dev-shm-usage",
                            "--disable-setuid-sandbox",
                            "--no-sandbox",
                            "--disable-gpu"
                            },
                        Headless = true,
                        Timeout = 0,
                    });
                }
                _logger.LogInformation($"瀏覽器設定完成");
            }
            catch (Exception ex)
            {
                _logger.LogError("SettingBrowser：" + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 設定頁面
        /// </summary>
        /// <returns></returns>
        private async Task SettingPage()
        {
            try
            {
                _logger.LogInformation($"設定頁面中");

                if (_browser == null)
                {
                    await SettingBrowser();
                    if (_browser == null)
                    {
                        throw new Exception("初始化Browser錯誤");
                    }
                }

                //新增頁面
                if (_page == null)
                {
                    _page = await _browser.NewPageAsync();
                    if (_page == null)
                    {
                        throw new Exception("初始化Page錯誤");
                    }
                }
                    

                //設定頁面大小
                await _page.SetViewportSizeAsync(1920, 1080);

                _logger.LogInformation($"設定頁面完成");

            }
            catch (Exception ex)
            {
                _logger.LogError("SettingPage：" + ex.Message);
            }
        }

        /// <summary>
        /// 關閉Playwright
        /// </summary>
        /// <returns></returns>
        private void ClosePlaywright()
        {
            try
            {
                if (_playwright != null)
                    _playwright.Dispose();
                _playwright = null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"關閉頁面時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 關閉瀏覽器
        /// </summary>
        /// <returns></returns>
        private async Task CloseBrowser()
        {
            try
            {
                if (_browser != null)
                    await _browser.DisposeAsync();

                _browser = null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"關閉頁面時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 關閉頁面
        /// </summary>
        /// <returns></returns>
        private async Task ClosePage()
        {
            try
            {
                if (_page != null)
                    await _page.CloseAsync();
                _page = null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"關閉頁面時發生錯誤: {ex.Message}");
            }
        }
    }
}
