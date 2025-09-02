using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic nhóm
    /// </summary>
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GroupService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Owner)
                .Include(g => g.Members)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<IEnumerable<GroupViewModel>> GetUserGroupsAsync(string userId)
        {
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsActive)
                .Include(gm => gm.Group)
                .ThenInclude(g => g.Owner)
                .Select(gm => gm.Group)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GroupViewModel>>(groups);
        }

        public async Task<Group> CreateGroupAsync(string ownerId, string name, string? description = null)
        {
            var group = new Group
            {
                Name = name,
                Description = description,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Thêm owner làm thành viên
            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = ownerId,
                Role = GroupRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(ownerMember);
            await _context.SaveChangesAsync();

            return group;
        }

        public async Task<bool> UpdateGroupAsync(int groupId, string userId, string name, string? description = null)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return false;

            // Kiểm tra quyền
            if (!await IsGroupAdminAsync(groupId, userId)) return false;

            group.Name = name;
            group.Description = description;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteGroupAsync(int groupId, string userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return false;

            // Chỉ owner mới có thể xóa nhóm
            if (group.OwnerId != userId) return false;

            group.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddMemberAsync(int groupId, string userId, string newMemberId)
        {
            // Kiểm tra quyền
            if (!await IsGroupAdminAsync(groupId, userId)) return false;

            // Kiểm tra xem đã là thành viên chưa
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == newMemberId);

            if (existingMember != null)
            {
                if (existingMember.IsActive) return false; // Đã là thành viên
                
                // Kích hoạt lại thành viên
                existingMember.IsActive = true;
                existingMember.JoinedAt = DateTime.UtcNow;
                existingMember.LeftAt = null;
            }
            else
            {
                var newMember = new GroupMember
                {
                    GroupId = groupId,
                    UserId = newMemberId,
                    Role = GroupRole.Member,
                    JoinedAt = DateTime.UtcNow
                };

                _context.GroupMembers.Add(newMember);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int groupId, string userId, string memberId)
        {
            // Kiểm tra quyền
            if (!await IsGroupAdminAsync(groupId, userId)) return false;

            // Không thể xóa owner
            var group = await _context.Groups.FindAsync(groupId);
            if (group?.OwnerId == memberId) return false;

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (member == null) return false;

            member.IsActive = false;
            member.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMemberRoleAsync(int groupId, string userId, string memberId, GroupRole role)
        {
            // Chỉ owner mới có thể thay đổi role
            var group = await _context.Groups.FindAsync(groupId);
            if (group?.OwnerId != userId) return false;

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (member == null) return false;

            member.Role = role;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<GroupMemberViewModel>> GetGroupMembersAsync(int groupId)
        {
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.IsActive)
                .Include(gm => gm.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GroupMemberViewModel>>(members);
        }

        public async Task<bool> LeaveGroupAsync(int groupId, string userId)
        {
            // Owner không thể rời nhóm, phải xóa nhóm
            var group = await _context.Groups.FindAsync(groupId);
            if (group?.OwnerId == userId) return false;

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (member == null) return false;

            member.IsActive = false;
            member.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsGroupMemberAsync(int groupId, string userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);
        }

        public async Task<bool> IsGroupAdminAsync(int groupId, string userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && 
                              gm.IsActive && (gm.Role == GroupRole.Admin || gm.Role == GroupRole.Owner));
        }

        public async Task<bool> IsGroupOwnerAsync(int groupId, string userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && 
                              gm.IsActive && gm.Role == GroupRole.Owner);
        }
    }
}
