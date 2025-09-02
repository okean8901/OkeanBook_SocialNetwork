using System.ComponentModel.DataAnnotations;

namespace OkeanBook.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho bài viết
    /// </summary>
    public class PostViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public PostType Type { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public bool IsLiked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }

    public class CreatePostViewModel
    {
        [Required]
        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        public string Content { get; set; } = string.Empty;

        public IFormFile? MediaFile { get; set; }

        public PostType Type { get; set; } = PostType.Text;

        public bool IsPublic { get; set; } = true;
    }

    public class CreateCommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}
