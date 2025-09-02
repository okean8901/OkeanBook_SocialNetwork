using OkeanBook.Models;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho Notification Service
    /// </summary>
    public interface INotificationService
    {
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<Notification> CreateNotificationAsync(string userId, string message, NotificationType type, string? relatedUserId = null, int? relatedPostId = null, int? relatedMessageId = null, int? relatedGroupId = null);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    }
}
