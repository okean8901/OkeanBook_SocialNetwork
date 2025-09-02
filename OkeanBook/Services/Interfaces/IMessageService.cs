using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho Message Service
    /// </summary>
    public interface IMessageService
    {
        Task<Message?> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<MessageViewModel>> GetMessagesAsync(string userId, string friendId, int page = 1, int pageSize = 50);
        Task<IEnumerable<MessageViewModel>> GetGroupMessagesAsync(int groupId, int page = 1, int pageSize = 50);
        Task<Message> SendMessageAsync(string senderId, string receiverId, string content, MessageType type = MessageType.Text, string? mediaUrl = null);
        Task<Message> SendGroupMessageAsync(string senderId, int groupId, string content, MessageType type = MessageType.Text, string? mediaUrl = null);
        Task<bool> MarkMessageAsReadAsync(int messageId, string userId);
        Task<bool> MarkMessagesAsReadAsync(string userId, string friendId);
        Task<bool> MarkGroupMessagesAsReadAsync(string userId, int groupId);
        Task<bool> DeleteMessageAsync(int messageId, string userId);
        Task<bool> RecallMessageAsync(int messageId, string userId);
        Task<IEnumerable<ChatUserViewModel>> GetChatListAsync(string userId);
        Task<IEnumerable<GroupViewModel>> GetGroupChatListAsync(string userId);
        Task<int> GetUnreadMessageCountAsync(string userId);
        Task<int> GetUnreadMessageCountWithUserAsync(string userId, string friendId);
        Task<int> GetUnreadGroupMessageCountAsync(string userId, int groupId);
    }
}
