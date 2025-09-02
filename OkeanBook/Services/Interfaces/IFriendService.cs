using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho Friend Service
    /// </summary>
    public interface IFriendService
    {
        Task<bool> SendFriendRequestAsync(string senderId, string receiverId);
        Task<bool> AcceptFriendRequestAsync(string userId, string friendId);
        Task<bool> DeclineFriendRequestAsync(string userId, string friendId);
        Task<bool> BlockUserAsync(string userId, string blockedUserId);
        Task<bool> UnblockUserAsync(string userId, string unblockedUserId);
        Task<bool> RemoveFriendAsync(string userId, string friendId);
        Task<FriendStatus?> GetFriendStatusAsync(string userId, string friendId);
        Task<IEnumerable<UserViewModel>> GetFriendsAsync(string userId);
        Task<IEnumerable<UserViewModel>> GetFriendRequestsAsync(string userId);
        Task<IEnumerable<UserViewModel>> GetBlockedUsersAsync(string userId);
        Task<bool> AreFriendsAsync(string userId1, string userId2);
    }
}
