using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OkeanBook.Models
{
    /// <summary>
    /// Model người dùng mở rộng từ IdentityUser
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [StringLength(200)]
        public string? Avatar { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Offline;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeen { get; set; }

        // Navigation properties
        public virtual ICollection<Friend> SentFriendRequests { get; set; } = new List<Friend>();
        public virtual ICollection<Friend> ReceivedFriendRequests { get; set; } = new List<Friend>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
        public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    /// <summary>
    /// Trạng thái người dùng
    /// </summary>
    public enum UserStatus
    {
        Offline = 0,
        Online = 1,
        Away = 2,
        Busy = 3
    }
}
