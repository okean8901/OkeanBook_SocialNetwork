using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OkeanBook.Models
{
    /// <summary>
    /// Model quản lý bạn bè
    /// </summary>
    public class Friend
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string FriendId { get; set; } = string.Empty;

        public FriendStatus Status { get; set; } = FriendStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("FriendId")]
        public virtual ApplicationUser FriendUser { get; set; } = null!;
    }

    /// <summary>
    /// Trạng thái bạn bè
    /// </summary>
    public enum FriendStatus
    {
        Pending = 0,    // Đang chờ chấp nhận
        Accepted = 1,   // Đã chấp nhận
        Blocked = 2,    // Đã chặn
        Declined = 3    // Đã từ chối
    }
}
