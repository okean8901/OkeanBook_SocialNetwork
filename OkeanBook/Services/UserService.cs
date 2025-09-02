using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic người dùng
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetUserByUserNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<IEnumerable<UserViewModel>> SearchUsersAsync(string query, string currentUserId)
        {
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && 
                           (u.UserName.Contains(query) || u.Email.Contains(query)))
                .Take(20)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var friendStatus = await GetFriendStatusAsync(currentUserId, user.Id);
                var userViewModel = _mapper.Map<UserViewModel>(user);
                userViewModel.FriendStatus = friendStatus;
                userViewModels.Add(userViewModel);
            }

            return userViewModels;
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

        public async Task<bool> UpdateProfileAsync(string userId, EditProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.UserName = model.UserName;
            user.Bio = model.Bio;
            user.Status = model.Status;

            // Xử lý upload avatar nếu có
            if (model.AvatarFile != null)
            {
                var fileName = $"{userId}_{DateTime.UtcNow.Ticks}.jpg";
                var filePath = Path.Combine("wwwroot", "uploads", "avatars", fileName);
                
                // Tạo thư mục nếu chưa tồn tại
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }
                
                user.Avatar = $"/uploads/avatars/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, UserStatus status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = status;
            if (status == UserStatus.Offline)
            {
                user.LastSeen = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserViewModel>> GetOnlineFriendsAsync(string userId)
        {
            var friends = await GetFriendsAsync(userId);
            return friends.Where(f => f.Status == UserStatus.Online);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        private async Task<FriendStatus?> GetFriendStatusAsync(string userId1, string userId2)
        {
            var friend = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId1 && f.FriendId == userId2) || 
                                         (f.UserId == userId2 && f.FriendId == userId1));
            
            return friend?.Status;
        }
    }
}
