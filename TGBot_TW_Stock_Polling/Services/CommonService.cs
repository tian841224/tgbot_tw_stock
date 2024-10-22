using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using TGBot_TW_Stock_Polling.Interface;
using Telegram.Bot;

namespace TGBot_TW_Stock_Polling.Services
{
    public class CommonService : ICommonService
    {
        private readonly ILogger<CommonService> _logger;
        private readonly IBrowserHandlers _browserHandlers;
        private readonly ITelegramBotClient _botClient;

        public CommonService(ILogger<CommonService> logger, IBrowserHandlers browserHandlers, ITelegramBotClient botClient)
        {
            _logger = logger;
            _browserHandlers = browserHandlers;
            _botClient = botClient;
        }

        public async Task RetryAsync(Func<Task> action, int maxRetries, TimeSpan delay, Message message, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    await action();
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning($"嘗試 {retryCount} 次失敗：{ex.Message}");
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogInformation($"已達最大重試次數 ({maxRetries})，拋出例外。");
                        await _botClient.SendTextMessageAsync(
                           text: "因機器人部屬於雲端，有時會無法讀取網頁，請將程式部屬至本機執行。",
                           chatId: message.Chat.Id,
                           parseMode: ParseMode.Html,
                           cancellationToken: cancellationToken);
                        _logger.LogInformation("已傳送資訊");

                        // 格式化方法名稱
                        string methodName = action.Method.Name;
                        if (methodName.Contains('<') && methodName.Contains('>'))
                        {
                            // 提取 "<" 和 ">" 之間的部分
                            int startIndex = methodName.IndexOf('<') + 1;
                            int endIndex = methodName.IndexOf('>');
                            methodName = methodName.Substring(startIndex, endIndex - startIndex);
                        }

                        throw new Exception($"{methodName}：{ex.Message}");
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
