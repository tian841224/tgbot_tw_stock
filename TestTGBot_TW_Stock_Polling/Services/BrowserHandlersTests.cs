using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Moq.Protected;
using TGBot_TW_Stock_Polling.Interface;
using TGBot_TW_Stock_Polling.Services;

namespace TestTGBot_TW_Stock_Polling.Services
{
    [TestClass]
    public class BrowserHandlersTests
    {
        private Mock<ILogger<BrowserHandlers>> _mockLogger;
        private Mock<IPlaywright> _mockPlaywright;
        private Mock<IBrowser> _mockBrowser;
        private Mock<IPage> _mockPage;
        private BrowserHandlers _browserHandlers;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<BrowserHandlers>>();
            _mockPlaywright = new Mock<IPlaywright>();
            _mockBrowser = new Mock<IBrowser>();
            _mockPage = new Mock<IPage>();

            _browserHandlers = new BrowserHandlers(_mockLogger.Object);
        }

        [TestMethod]
        public async Task LoadUrl_ValidUrl_ReturnsPage()
        {
            // Arrange
            var url = "https://www.cnyes.com/twstock/";

            // Act
            var page = await _browserHandlers.LoadUrlAsync(url);

            // Assert
            Assert.IsNotNull(page);
        }

        [TestMethod]
        public async Task LoadUrl_ValidUrl_Cnyes_ReturnsPage()
        {
            // Arrange
            var url = "https://www.cnyes.com/twstock/2330";

            // Act
            var page = await _browserHandlers.LoadUrlAsync(url);

            // Assert
            Assert.IsNotNull(page);
        }

        [TestMethod]
        public async Task LoadUrl_ValidUrl_TradingView_ReturnsPage()
        {
            // Arrange
            var url = "https://tw.tradingview.com/chart/?symbol=TWSE%3A2330";

            // Act
            var page = await _browserHandlers.LoadUrlAsync(url);

            // Assert
            Assert.IsNotNull(page);
        }

        [TestMethod]
        public async Task LoadUrl_InvalidUrl_ThrowsException()
        {
            // Arrange
            var invalidUrl = "invalid_url";

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<Exception>(() => _browserHandlers.LoadUrlAsync(invalidUrl));
        }
    }
}
