using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic bạn bè
    /// </summary>
    public class FriendService : IFriendService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FriendService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
        {
            // Kiểm tra xem đã có mối quan hệ nào chưa
            var existingFriend = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == senderId && f.FriendId == receiverId) ||
                                         (f.UserId == receiverId && f.FriendId == senderId));

            if (existingFriend != null)
            {
                return false; // Đã có mối quan hệ
            }

            var friendRequest = new Friend
            {
                UserId = senderId,
                FriendId = receiverId,
                Status = FriendStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friendRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AcceptFriendRequestAsync(string userId, string friendId)
        {
            var friendRequest = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == FriendStatus.Pending);

            if (friendRequest == null) return false;

            friendRequest.Status = FriendStatus.Accepted;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineFriendRequestAsync(string userId, string friendId)
        {
            var friendRequest = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == FriendStatus.Pending);

            if (friendRequest == null) return false;

            friendRequest.Status = FriendStatus.Declined;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BlockUserAsync(string userId, string blockedUserId)
        {
            var existingFriend = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == blockedUserId) ||
                                         (f.UserId == blockedUserId && f.FriendId == userId));

            if (existingFriend != null)
            {
                existingFriend.Status = FriendStatus.Blocked;
                existingFriend.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var block = new Friend
                {
                    UserId = userId,
                    FriendId = blockedUserId,
                    Status = FriendStatus.Blocked,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Friends.Add(block);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnblockUserAsync(string userId, string unblockedUserId)
        {
            var blockedFriend = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == unblockedUserId && f.Status == FriendStatus.Blocked);

            if (blockedFriend == null) return false;

            _context.Friends.Remove(blockedFriend);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
                                         (f.UserId == friendId && f.FriendId == userId));

            if (friend == null) return false;

            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FriendStatus?> GetFriendStatusAsync(string userId, string friendId)
        {
            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
                                         (f.UserId == friendId && f.FriendId == userId));

            return friend?.Status;
        }

        public async Task<IEnumerable<UserViewModel>> GetFriendsAsync(string userId)
        {
            var friends = await _context.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Accepted)
                .Include(f => f.User)
                .Include(f => f.FriendUser)
                .ToListAsync();

            var friendViewModels = new List<UserViewModel>();
            foreach (var friend in friends)
            {
                var friendUser = friend.UserId == userId ? friend.FriendUser : friend.User;
                var friendViewModel = _mapper.Map<UserViewModel>(friendUser);
                friendViewModels.Add(friendViewModel);
            }

            return friendViewModels;
        }

        public async Task<IEnumerable<UserViewModel>> GetFriendRequestsAsync(string userId)
        {
            var requests = await _context.Friends
                .Where(f => f.FriendId == userId && f.Status == FriendStatus.Pending)
                .Include(f => f.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserViewModel>>(requests.Select(r => r.User));
        }

        public async Task<IEnumerable<UserViewModel>> GetBlockedUsersAsync(string userId)
        {
            var blockedUsers = await _context.Friends
                .Where(f => f.UserId == userId && f.Status == FriendStatus.Blocked)
                .Include(f => f.FriendUser)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserViewModel>>(blockedUsers.Select(b => b.FriendUser));
        }

        public async Task<bool> AreFriendsAsync(string userId1, string userId2)
        {
            return await _context.Friends
                .AnyAsync(f => (f.UserId == userId1 && f.FriendId == userId2) ||
                              (f.UserId == userId2 && f.FriendId == userId1) &&
                              f.Status == FriendStatus.Accepted);
        }
    }
}
