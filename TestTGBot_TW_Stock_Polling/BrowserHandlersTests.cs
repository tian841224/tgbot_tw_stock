using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using TGBot_TW_Stock_Polling.Interface;
using TGBot_TW_Stock_Polling.Services;

namespace TestTGBot_TW_Stock_Polling
{
    [TestClass]
    internal class BrowserHandlersTests
    {
        private Mock<ILogger<BrowserHandlers>> _loggerMock = null!;
        private IBrowserHandlers _browserHandlers = null!;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<BrowserHandlers>>();
            _browserHandlers = new BrowserHandlers(_loggerMock.Object);
        }

        [TestMethod]
        public async Task GetPageAsync_PageInitialized_ReturnsPage()
        {
            // Arrange
            var page = await _browserHandlers.GetPageAsync();

            // Act & Assert
            Assert.IsNotNull(page);
        }

        //[TestMethod]
        //[ExpectedException(typeof(Exception), "初始化Page錯誤")]
        //public async Task GetPageAsync_PageNotInitialized_ThrowsException()
        //{
        //    // Arrange
        //    await _browserHandlers.ReleaseBrowser();

        //    // Act
        //    await _browserHandlers.GetPageAsync();
        //}

        [TestMethod]
        public async Task LoadUrl_ValidUrl_ReturnsPage()
        {
            // Arrange
            var url = "https://www.cnyes.com/twstock/";

            // Act
            var page = await _browserHandlers.LoadUrl(url);

            // Assert
            Assert.IsNotNull(page);
        }

        [TestMethod]
        public async Task LoadUrl_InvalidUrl_ThrowsException()
        {
            // Arrange
            var invalidUrl = "invalid_url";

            // Act & Assert
            await Assert.ThrowsExceptionAsync<PlaywrightException>(async () => await _browserHandlers.LoadUrl(invalidUrl));
        }

    }
}
