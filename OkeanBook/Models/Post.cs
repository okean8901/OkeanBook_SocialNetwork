using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OkeanBook.Models
{
    /// <summary>
    /// Model bài viết
    /// </summary>
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MediaUrl { get; set; }

        public PostType Type { get; set; } = PostType.Text;

        public int LikeCount { get; set; } = 0;

        public int CommentCount { get; set; } = 0;

        public int ShareCount { get; set; } = 0;

        public bool IsPublic { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }

    /// <summary>
    /// Loại bài viết
    /// </summary>
    public enum PostType
    {
        Text = 0,
        Image = 1,
        Video = 2,
        Link = 3
    }
}
