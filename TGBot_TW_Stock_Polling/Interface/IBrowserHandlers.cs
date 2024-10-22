using Microsoft.Playwright;

namespace TGBot_TW_Stock_Polling.Interface
{
    public interface IBrowserHandlers
    {
        /// <summary>釋放瀏覽器</summary>
        Task ReleaseBrowser();

        /// <summary>載入網頁</summary>
        Task<IPage> LoadUrlAsync(string url);
    }
}
