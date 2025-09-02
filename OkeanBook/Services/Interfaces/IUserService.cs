using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho User Service
    /// </summary>
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByUserNameAsync(string username);
        Task<IEnumerable<UserViewModel>> SearchUsersAsync(string query, string currentUserId);
        Task<IEnumerable<UserViewModel>> GetFriendsAsync(string userId);
        Task<IEnumerable<UserViewModel>> GetFriendRequestsAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, EditProfileViewModel model);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
        Task<bool> UpdateUserStatusAsync(string userId, UserStatus status);
        Task<IEnumerable<UserViewModel>> GetOnlineFriendsAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
    }
}
