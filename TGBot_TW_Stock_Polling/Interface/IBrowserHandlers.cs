using Microsoft.Playwright;

namespace TGBot_TW_Stock_Polling.Interface
{
    public interface IBrowserHandlers
    {
        /// <summary>取得Page</summary>
        Task<IPage> GetPageAsync();

        /// <summary>釋放瀏覽器</summary>
        Task ReleaseBrowser();

        /// <summary>載入網頁</summary>
        Task<IPage> LoadUrl(string url);

        /// <summary>啟動套件</summary>
        Task LunchesPlaywright();

    }
}
