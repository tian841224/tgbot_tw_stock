using Telegram.Bot.Types;

namespace TGBot_TW_Stock_Polling.Dto
{
    public class DeleteDto
    {
        public Message Message { get; set; }

        public Message Reply { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}
