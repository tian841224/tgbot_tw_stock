using Telegram.Bot.Types;
using TGBot_TW_Stock_Polling.Dto;

namespace TGBot_TW_Stock_Polling.Interface
{
    public interface IBotService
    {
        /// <summary>傳送訊息</summary>
        Task<Message> SendTextMessageAsync(MessageDto dto);

        /// <summary>傳送Hello訊息</summary>
        Task<Message> SendHelloMessageAsync(Message message, CancellationToken cancellationToken);

        /// <summary>傳送等待訊息</summary>
        Task<Message> SendWaitMessageAsync(Message message, CancellationToken cancellationToken);

        /// <summary>傳送指令錯誤訊息</summary>
        Task SendErrorMessageAsync(Message message, CancellationToken cancellationToken);

        /// <summary>刪除訊息</summary>
        Task DeleteMessageAsync(DeleteDto dto);

        /// <summary>錯誤通知</summary>
        Task ErrorNotify(Message message, string errorMessage, CancellationToken cancellationToken);
    }
}
