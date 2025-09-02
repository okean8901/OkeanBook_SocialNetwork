using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho Group Service
    /// </summary>
    public interface IGroupService
    {
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<IEnumerable<GroupViewModel>> GetUserGroupsAsync(string userId);
        Task<Group> CreateGroupAsync(string ownerId, string name, string? description = null);
        Task<bool> UpdateGroupAsync(int groupId, string userId, string name, string? description = null);
        Task<bool> DeleteGroupAsync(int groupId, string userId);
        Task<bool> AddMemberAsync(int groupId, string userId, string newMemberId);
        Task<bool> RemoveMemberAsync(int groupId, string userId, string memberId);
        Task<bool> UpdateMemberRoleAsync(int groupId, string userId, string memberId, GroupRole role);
        Task<IEnumerable<GroupMemberViewModel>> GetGroupMembersAsync(int groupId);
        Task<bool> LeaveGroupAsync(int groupId, string userId);
        Task<bool> IsGroupMemberAsync(int groupId, string userId);
        Task<bool> IsGroupAdminAsync(int groupId, string userId);
        Task<bool> IsGroupOwnerAsync(int groupId, string userId);
    }
}
