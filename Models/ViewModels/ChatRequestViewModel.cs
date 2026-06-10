using System.Collections.Generic;

namespace SentinelAI.Models.ViewModels
{
    public class ChatRequestViewModel
    {
        public List<ChatMessageViewModel> History { get; set; } = new List<ChatMessageViewModel>();
        public string Message { get; set; } = string.Empty;
    }
}
