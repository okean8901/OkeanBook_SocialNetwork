using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Services.Interfaces
{
    /// <summary>
    /// Interface cho Post Service
    /// </summary>
    public interface IPostService
    {
        Task<Post?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostViewModel>> GetNewsFeedAsync(string userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<PostViewModel>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 10);
        Task<Post> CreatePostAsync(string userId, CreatePostViewModel model);
        Task<bool> UpdatePostAsync(int postId, string userId, CreatePostViewModel model);
        Task<bool> DeletePostAsync(int postId, string userId);
        Task<bool> LikePostAsync(int postId, string userId);
        Task<bool> UnlikePostAsync(int postId, string userId);
        Task<bool> IsPostLikedAsync(int postId, string userId);
        Task<Comment> CreateCommentAsync(string userId, CreateCommentViewModel model);
        Task<bool> UpdateCommentAsync(int commentId, string userId, string content);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<bool> LikeCommentAsync(int commentId, string userId);
        Task<bool> UnlikeCommentAsync(int commentId, string userId);
        Task<bool> IsCommentLikedAsync(int commentId, string userId);
        Task<IEnumerable<CommentViewModel>> GetPostCommentsAsync(int postId, int page = 1, int pageSize = 20);
        Task<IEnumerable<PostViewModel>> SearchPostsAsync(string query, int page = 1, int pageSize = 10);
    }
}
