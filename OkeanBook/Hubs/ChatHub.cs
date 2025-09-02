using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using System.Security.Claims;

namespace OkeanBook.Hubs
{
    /// <summary>
    /// SignalR Hub cho chat realtime
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kết nối người dùng
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _userConnections[userId] = Context.ConnectionId;

                // Cập nhật trạng thái online
                await UpdateUserStatus(userId, UserStatus.Online);

                // Thông báo cho bạn bè về trạng thái online
                await NotifyFriendsStatusChange(userId, UserStatus.Online);

                // Tham gia các nhóm chat
                await JoinUserGroups(userId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Ngắt kết nối người dùng
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                _userConnections.Remove(userId);

                // Cập nhật trạng thái offline
                await UpdateUserStatus(userId, UserStatus.Offline);

                // Thông báo cho bạn bè về trạng thái offline
                await NotifyFriendsStatusChange(userId, UserStatus.Offline);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Gửi tin nhắn riêng tư
        /// </summary>
        public async Task SendPrivateMessage(string receiverId, string content, string messageType = "Text")
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            // Kiểm tra mối quan hệ bạn bè
            var areFriends = await AreFriends(senderId, receiverId);
            if (!areFriends)
            {
                await Clients.Caller.SendAsync("Error", "Bạn không thể gửi tin nhắn cho người này");
                return;
            }

            // Lưu tin nhắn vào database
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Type = Enum.Parse<MessageType>(messageType),
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Lấy thông tin người gửi
            var sender = await _context.Users.FindAsync(senderId);

            // Gửi tin nhắn cho người nhận
            if (_userConnections.ContainsKey(receiverId))
            {
                await Clients.Client(_userConnections[receiverId]).SendAsync("ReceiveMessage", new
                {
                    Id = message.Id,
                    SenderId = senderId,
                    SenderName = sender?.UserName,
                    SenderAvatar = sender?.Avatar,
                    Content = content,
                    Type = messageType,
                    SentAt = message.SentAt,
                    IsOwn = false
                });
            }

            // Gửi tin nhắn cho người gửi (để hiển thị trong UI)
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Id = message.Id,
                SenderId = senderId,
                SenderName = sender?.UserName,
                SenderAvatar = sender?.Avatar,
                Content = content,
                Type = messageType,
                SentAt = message.SentAt,
                IsOwn = true
            });

            // Tạo thông báo
            await CreateNotification(receiverId, $"{sender?.UserName} đã gửi tin nhắn cho bạn", NotificationType.Message, senderId, message.Id);
        }

        /// <summary>
        /// Gửi tin nhắn nhóm
        /// </summary>
        public async Task SendGroupMessage(int groupId, string content, string messageType = "Text")
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            // Kiểm tra quyền thành viên nhóm
            var isMember = await IsGroupMember(senderId, groupId);
            if (!isMember)
            {
                await Clients.Caller.SendAsync("Error", "Bạn không phải thành viên của nhóm này");
                return;
            }

            // Lưu tin nhắn vào database
            var message = new Message
            {
                SenderId = senderId,
                GroupId = groupId,
                Content = content,
                Type = Enum.Parse<MessageType>(messageType),
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Lấy thông tin người gửi
            var sender = await _context.Users.FindAsync(senderId);

            // Gửi tin nhắn cho tất cả thành viên nhóm
            await Clients.Group($"Group_{groupId}").SendAsync("ReceiveGroupMessage", new
            {
                Id = message.Id,
                GroupId = groupId,
                SenderId = senderId,
                SenderName = sender?.UserName,
                SenderAvatar = sender?.Avatar,
                Content = content,
                Type = messageType,
                SentAt = message.SentAt,
                IsOwn = senderId == Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            });

            // Tạo thông báo cho các thành viên khác
            var groupMembers = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.UserId != senderId && gm.IsActive)
                .Select(gm => gm.UserId)
                .ToListAsync();

            foreach (var memberId in groupMembers)
            {
                await CreateNotification(memberId, $"{sender?.UserName} đã gửi tin nhắn trong nhóm", NotificationType.GroupMessage, senderId, message.Id, groupId);
            }
        }

        /// <summary>
        /// Tham gia nhóm chat
        /// </summary>
        public async Task JoinGroup(int groupId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var isMember = await IsGroupMember(userId, groupId);
            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
                await Clients.Group($"Group_{groupId}").SendAsync("UserJoined", userId);
            }
        }

        /// <summary>
        /// Rời khỏi nhóm chat
        /// </summary>
        public async Task LeaveGroup(int groupId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
            await Clients.Group($"Group_{groupId}").SendAsync("UserLeft", userId);
        }

        /// <summary>
        /// Thông báo đang nhập
        /// </summary>
        public async Task StartTyping(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            if (_userConnections.ContainsKey(receiverId))
            {
                await Clients.Client(_userConnections[receiverId]).SendAsync("UserTyping", senderId);
            }
        }

        /// <summary>
        /// Dừng thông báo đang nhập
        /// </summary>
        public async Task StopTyping(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            if (_userConnections.ContainsKey(receiverId))
            {
                await Clients.Client(_userConnections[receiverId]).SendAsync("UserStoppedTyping", senderId);
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        public async Task MarkMessageAsRead(int messageId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var message = await _context.Messages.FindAsync(messageId);
            if (message != null && message.ReceiverId == userId && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Thông báo cho người gửi
                if (_userConnections.ContainsKey(message.SenderId))
                {
                    await Clients.Client(_userConnections[message.SenderId]).SendAsync("MessageRead", messageId);
                }
            }
        }

        #region Private Methods

        private async Task UpdateUserStatus(string userId, UserStatus status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = status;
                if (status == UserStatus.Offline)
                {
                    user.LastSeen = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task NotifyFriendsStatusChange(string userId, UserStatus status)
        {
            var friends = await _context.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Accepted)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .ToListAsync();

            foreach (var friendId in friends)
            {
                if (_userConnections.ContainsKey(friendId))
                {
                    await Clients.Client(_userConnections[friendId]).SendAsync("FriendStatusChanged", userId, status.ToString());
                }
            }
        }

        private async Task JoinUserGroups(string userId)
        {
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsActive)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            foreach (var groupId in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
            }
        }

        private async Task<bool> AreFriends(string userId1, string userId2)
        {
            return await _context.Friends
                .AnyAsync(f => (f.UserId == userId1 && f.FriendId == userId2 || f.UserId == userId2 && f.FriendId == userId1) 
                              && f.Status == FriendStatus.Accepted);
        }

        private async Task<bool> IsGroupMember(string userId, int groupId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId && gm.IsActive);
        }

        private async Task CreateNotification(string userId, string message, NotificationType type, string? relatedUserId = null, int? relatedMessageId = null, int? relatedGroupId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                RelatedUserId = relatedUserId,
                RelatedMessageId = relatedMessageId,
                RelatedGroupId = relatedGroupId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi thông báo realtime
            if (_userConnections.ContainsKey(userId))
            {
                await Clients.Client(_userConnections[userId]).SendAsync("ReceiveNotification", new
                {
                    Id = notification.Id,
                    Message = message,
                    Type = type.ToString(),
                    CreatedAt = notification.CreatedAt
                });
            }
        }

        #endregion
    }
}
