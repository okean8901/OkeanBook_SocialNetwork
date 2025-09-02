using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic thông báo
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .Include(n => n.RelatedUser)
                .Include(n => n.RelatedPost)
                .Include(n => n.RelatedMessage)
                .Include(n => n.RelatedGroup)
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.RelatedUser)
                .Include(n => n.RelatedPost)
                .Include(n => n.RelatedMessage)
                .Include(n => n.RelatedGroup)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(string userId, string message, NotificationType type, string? relatedUserId = null, int? relatedPostId = null, int? relatedMessageId = null, int? relatedGroupId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                RelatedUserId = relatedUserId,
                RelatedPostId = relatedPostId,
                RelatedMessageId = relatedMessageId,
                RelatedGroupId = relatedGroupId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null || notification.UserId != userId || notification.IsRead) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null || notification.UserId != userId) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Include(n => n.RelatedUser)
                .Include(n => n.RelatedPost)
                .Include(n => n.RelatedMessage)
                .Include(n => n.RelatedGroup)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
