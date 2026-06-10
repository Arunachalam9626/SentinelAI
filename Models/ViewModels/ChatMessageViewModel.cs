namespace SentinelAI.Models.ViewModels
{
    public class ChatMessageViewModel
    {
        public string Role { get; set; } = string.Empty;     // "user" or "model"
        public string Content { get; set; } = string.Empty;
    }
}
