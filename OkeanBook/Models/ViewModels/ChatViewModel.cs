namespace OkeanBook.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho chat
    /// </summary>
    public class ChatViewModel
    {
        public string CurrentUserId { get; set; } = string.Empty;
        public List<ChatUserViewModel> Friends { get; set; } = new List<ChatUserViewModel>();
        public List<GroupViewModel> Groups { get; set; } = new List<GroupViewModel>();
        public string? SelectedChatId { get; set; }
        public bool IsGroupChat { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
    }

    public class ChatUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsTyping { get; set; }
    }

    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public List<GroupMemberViewModel> Members { get; set; } = new List<GroupMemberViewModel>();
    }

    public class GroupMemberViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public GroupRole Role { get; set; }
        public UserStatus Status { get; set; }
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public string? MediaUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public bool IsRead { get; set; }
        public bool IsRecalled { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsOwn { get; set; }
    }
}
