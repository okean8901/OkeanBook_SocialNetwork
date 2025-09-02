using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic tin nhắn
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MessageService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<IEnumerable<MessageViewModel>> GetMessagesAsync(string userId, string friendId, int page = 1, int pageSize = 50)
        {
            var messages = await _context.Messages
                .Where(m => !m.IsDeleted && 
                           ((m.SenderId == userId && m.ReceiverId == friendId) ||
                            (m.SenderId == friendId && m.ReceiverId == userId)))
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageViewModels = new List<MessageViewModel>();
            foreach (var message in messages)
            {
                var messageViewModel = _mapper.Map<MessageViewModel>(message);
                messageViewModel.IsOwn = message.SenderId == userId;
                messageViewModels.Add(messageViewModel);
            }

            return messageViewModels.OrderBy(m => m.SentAt);
        }

        public async Task<IEnumerable<MessageViewModel>> GetGroupMessagesAsync(int groupId, int page = 1, int pageSize = 50)
        {
            var messages = await _context.Messages
                .Where(m => !m.IsDeleted && m.GroupId == groupId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageViewModels = new List<MessageViewModel>();
            foreach (var message in messages)
            {
                var messageViewModel = _mapper.Map<MessageViewModel>(message);
                messageViewModels.Add(messageViewModel);
            }

            return messageViewModels.OrderBy(m => m.SentAt);
        }

        public async Task<Message> SendMessageAsync(string senderId, string receiverId, string content, MessageType type = MessageType.Text, string? mediaUrl = null)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Type = type,
                MediaUrl = mediaUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<Message> SendGroupMessageAsync(string senderId, int groupId, string content, MessageType type = MessageType.Text, string? mediaUrl = null)
        {
            var message = new Message
            {
                SenderId = senderId,
                GroupId = groupId,
                Content = content,
                Type = type,
                MediaUrl = mediaUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId, string userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.ReceiverId != userId || message.IsRead) return false;

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkMessagesAsReadAsync(string userId, string friendId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId == friendId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkGroupMessagesAsReadAsync(string userId, int groupId)
        {
            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || (message.SenderId != userId && message.ReceiverId != userId)) return false;

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RecallMessageAsync(int messageId, string userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.SenderId != userId) return false;

            message.IsRecalled = true;
            message.Content = "Tin nhắn đã được thu hồi";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ChatUserViewModel>> GetChatListAsync(string userId)
        {
            var friends = await _context.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Accepted)
                .Include(f => f.User)
                .Include(f => f.FriendUser)
                .ToListAsync();

            var chatList = new List<ChatUserViewModel>();

            foreach (var friend in friends)
            {
                var friendUser = friend.UserId == userId ? friend.FriendUser : friend.User;
                
                // Lấy tin nhắn cuối cùng
                var lastMessage = await _context.Messages
                    .Where(m => !m.IsDeleted && 
                               ((m.SenderId == userId && m.ReceiverId == friendUser.Id) ||
                                (m.SenderId == friendUser.Id && m.ReceiverId == userId)))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                // Đếm tin nhắn chưa đọc
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderId == friendUser.Id && m.ReceiverId == userId && !m.IsRead);

                var chatUser = new ChatUserViewModel
                {
                    Id = friendUser.Id,
                    UserName = friendUser.UserName,
                    Avatar = friendUser.Avatar,
                    Status = friendUser.Status,
                    LastSeen = friendUser.LastSeen,
                    LastMessage = lastMessage?.Content,
                    LastMessageTime = lastMessage?.SentAt,
                    UnreadCount = unreadCount
                };

                chatList.Add(chatUser);
            }

            return chatList.OrderByDescending(c => c.LastMessageTime);
        }

        public async Task<IEnumerable<GroupViewModel>> GetGroupChatListAsync(string userId)
        {
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsActive)
                .Include(gm => gm.Group)
                .Select(gm => gm.Group)
                .ToListAsync();

            var groupChatList = new List<GroupViewModel>();

            foreach (var group in groups)
            {
                // Lấy tin nhắn cuối cùng
                var lastMessage = await _context.Messages
                    .Where(m => !m.IsDeleted && m.GroupId == group.Id)
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                // Đếm tin nhắn chưa đọc
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.GroupId == group.Id && m.SenderId != userId && !m.IsRead);

                // Lấy danh sách thành viên
                var members = await _context.GroupMembers
                    .Where(gm => gm.GroupId == group.Id && gm.IsActive)
                    .Include(gm => gm.User)
                    .ToListAsync();

                var groupViewModel = new GroupViewModel
                {
                    Id = group.Id,
                    Name = group.Name,
                    Avatar = group.Avatar,
                    LastMessage = lastMessage?.Content,
                    LastMessageTime = lastMessage?.SentAt,
                    UnreadCount = unreadCount,
                    Members = _mapper.Map<List<GroupMemberViewModel>>(members.Select(m => m.User))
                };

                groupChatList.Add(groupViewModel);
            }

            return groupChatList.OrderByDescending(g => g.LastMessageTime);
        }

        public async Task<int> GetUnreadMessageCountAsync(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<int> GetUnreadMessageCountWithUserAsync(string userId, string friendId)
        {
            return await _context.Messages
                .CountAsync(m => m.SenderId == friendId && m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<int> GetUnreadGroupMessageCountAsync(string userId, int groupId)
        {
            return await _context.Messages
                .CountAsync(m => m.GroupId == groupId && m.SenderId != userId && !m.IsRead);
        }
    }
}
