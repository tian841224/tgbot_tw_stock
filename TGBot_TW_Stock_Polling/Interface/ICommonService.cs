using Telegram.Bot.Types;

namespace TGBot_TW_Stock_Polling.Interface
{
    public interface ICommonService
    {
        /// <summary> 方法重試 </summary>
        Task RetryAsync(Func<Task> action, int maxRetries, TimeSpan delay, Message message, CancellationToken cancellationToken);
    }
}
