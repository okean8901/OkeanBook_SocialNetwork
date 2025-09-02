using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OkeanBook.Models
{
    /// <summary>
    /// Model thông báo
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Info;

        [StringLength(450)]
        public string? RelatedUserId { get; set; }

        public int? RelatedPostId { get; set; }

        public int? RelatedMessageId { get; set; }

        public int? RelatedGroupId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("RelatedUserId")]
        public virtual ApplicationUser? RelatedUser { get; set; }

        [ForeignKey("RelatedPostId")]
        public virtual Post? RelatedPost { get; set; }

        [ForeignKey("RelatedMessageId")]
        public virtual Message? RelatedMessage { get; set; }

        [ForeignKey("RelatedGroupId")]
        public virtual Group? RelatedGroup { get; set; }
    }

    /// <summary>
    /// Loại thông báo
    /// </summary>
    public enum NotificationType
    {
        Info = 0,
        FriendRequest = 1,
        Message = 2,
        PostLike = 3,
        PostComment = 4,
        GroupInvite = 5,
        GroupMessage = 6,
        System = 7
    }
}
