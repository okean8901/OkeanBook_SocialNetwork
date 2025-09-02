using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OkeanBook.Models
{
    /// <summary>
    /// Model tin nhắn
    /// </summary>
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ReceiverId { get; set; }

        public int? GroupId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        [StringLength(500)]
        public string? MediaUrl { get; set; }

        [StringLength(200)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public bool IsRecalled { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser? Receiver { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
    }

    /// <summary>
    /// Loại tin nhắn
    /// </summary>
    public enum MessageType
    {
        Text = 0,
        Image = 1,
        File = 2,
        Video = 3,
        Audio = 4,
        Emoji = 5
    }
}
