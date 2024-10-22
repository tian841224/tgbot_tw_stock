using Microsoft.Playwright;
using TGBot_TW_Stock_Polling.Interface;

namespace TGBot_TW_Stock_Polling.Services
{
    public class BrowserHandlers : IBrowserHandlers
    {
        private readonly ILogger<BrowserHandlers> _logger;
        private IPlaywright? _playwright = null;
        private IBrowser? _browser = null;
        private IPage? _page = null;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);

        public BrowserHandlers(ILogger<BrowserHandlers> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 載入網頁
        /// </summary>
        /// <param name="stockNumber"></param>
        /// <returns></returns>
        public async Task<IPage> LoadUrlAsync(string url)
        {
            try
            {
                var page = await GetPageAsync();
                await page.GotoAsync($"{url}",
                            new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = _timeout.Milliseconds });

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
        /// 釋放瀏覽器流程
        /// </summary>
        /// <returns></returns>
        public async Task ReleaseBrowser()
        {
            try
            {
                await ClosePage();
                await CloseBrowser();
                ClosePlaywright();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 取得頁面
        /// </summary>
        /// <returns></returns>
        private async Task<IPage> GetPageAsync()
        {
            try
            {
                if (_page == null)
                {
                    await CreateBrowserAsync();
                }

                if (_page == null)
                {
                    throw new InvalidOperationException("未能初始化 Page。");
                }

                return _page;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 啟動套件
        /// </summary>
        /// <returns></returns>
        private async Task LaunchPlaywrightAsync()
        {
            _logger.LogInformation("初始化Playwright");

            try
            {
                if (_playwright == null)
                    _playwright = await Playwright.CreateAsync();

                if (_playwright == null)
                    throw new Exception("初始化Playwright錯誤");
            }
            catch (Exception ex)
            {
                _logger.LogError("LaunchPlaywright：" + ex.Message);
                throw new Exception("LaunchPlaywright：" + ex.Message);
            }

        }

        /// <summary>
        /// 設定瀏覽器
        /// </summary>
        /// <returns></returns>
        private async Task SetupBrowserAsync()
        {
            _logger.LogInformation("設定瀏覽器");

            try
            {
                if (_playwright == null)
                    throw new Exception("Playwright not initialized");

                if (_browser == null)
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
            }
            catch (Exception ex)
            {
                _logger.LogError("SetupBrowser：" + ex.Message);
                throw new Exception("SetupBrowser：" + ex.Message);
            }
        }

        /// <summary>
        /// 設定頁面
        /// </summary>
        /// <returns></returns>
        private async Task SetupPageAsync()
        {
            _logger.LogInformation("設定頁面中");

            try
            {
                if (_browser == null)
                    throw new Exception("Browser not initialized");

                //新增頁面
                if (_page == null)
                {
                    _page = await _browser.NewPageAsync();

                    if (_page == null)
                        throw new Exception("初始化Page錯誤");
                }

                //設定頁面大小
                await _page.SetViewportSizeAsync(1920, 1080);
            }
            catch (Exception ex)
            {
                _logger.LogError("SetupPage：" + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 啟動瀏覽器流程
        /// </summary>
        /// <returns></returns>
        private async Task CreateBrowserAsync()
        {
            try
            {
                await LaunchPlaywrightAsync();
                await SetupBrowserAsync();
                await SetupPageAsync();
            }
            catch
            {
                throw;
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
                throw new Exception($"ClosePlaywright : {ex.Message}");
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
                throw new Exception($"CloseBrowser : {ex.Message}");
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
                throw new Exception($"ClosePage : {ex.Message}");
            }
        }
    }
}
