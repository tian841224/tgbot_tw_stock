using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Examples.WebHook.Services;
using TGBot_TW_Stock_Polling.Interface;

namespace TestTGBot_TW_Stock_Polling
{
    [TestClass]
    public class CnyesTests
    {
        private Mock<IBrowserHandlers> _mockBrowserHandlers = null!;
        private Mock<ILogger<Cnyes>> _mockLogger = null!;
        private Mock<ITelegramBotClient> _mockbotClient = null!;
        private Cnyes _cnyes = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockBrowserHandlers = new Mock<IBrowserHandlers>();
            _mockLogger = new Mock<ILogger<Cnyes>>();
            _mockbotClient = new Mock<ITelegramBotClient>();
            _cnyes = new Cnyes(_mockbotClient.Object, _mockLogger.Object, _mockBrowserHandlers.Object);
        }

        [TestMethod]
        public async Task LoadUrl_SuccessfulLoad_ReturnsPage()
        {
            // Arrange
            string stockNumber = "2330";
            var expectedUrl = $"https://www.cnyes.com/twstock/{stockNumber}";
            var mockPage = new Mock<IPage>();
            _mockBrowserHandlers.Setup(b => b.LoadUrl(expectedUrl)).ReturnsAsync(mockPage.Object);

            // Act
            var result = await _cnyes.LoadUrl(stockNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockPage.Object, result);
            _mockBrowserHandlers.Verify(b => b.LoadUrl(expectedUrl), Times.Once);
        }

        [TestMethod]
        public async Task LoadUrl_NullPage_ThrowsException()
        {
            // Arrange
            string stockNumber = "2330";
            var expectedUrl = $"https://www.cnyes.com/twstock/{stockNumber}";
            _mockBrowserHandlers.Setup(b => b.LoadUrl(expectedUrl)).ReturnsAsync((IPage)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _cnyes.LoadUrl(stockNumber));
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("初始化瀏覽器錯誤")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task LoadUrl_ExceptionThrown_LogsAndRethrows()
        {
            // Arrange
            string stockNumber = "2330";
            var expectedUrl = $"https://www.cnyes.com/twstock/{stockNumber}";
            var expectedException = new Exception("測試異常");
            _mockBrowserHandlers.Setup(b => b.LoadUrl(expectedUrl)).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(async () => await _cnyes.LoadUrl(stockNumber));
            Assert.AreEqual(expectedException, exception);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("載入網頁時發生錯誤: 測試異常")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
