using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Data;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;

namespace OkeanBook.Services
{
    /// <summary>
    /// Service xử lý logic bài viết
    /// </summary>
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PostService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
        }

        public async Task<IEnumerable<PostViewModel>> GetNewsFeedAsync(string userId, int page = 1, int pageSize = 10)
        {
            // Lấy danh sách bạn bè
            var friendIds = await _context.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Accepted)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .ToListAsync();

            // Thêm chính user vào danh sách
            friendIds.Add(userId);

            var posts = await _context.Posts
                .Where(p => !p.IsDeleted && p.IsPublic && friendIds.Contains(p.UserId))
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postViewModels = new List<PostViewModel>();
            foreach (var post in posts)
            {
                var postViewModel = _mapper.Map<PostViewModel>(post);
                postViewModel.IsLiked = await IsPostLikedAsync(post.Id, userId);
                postViewModels.Add(postViewModel);
            }

            return postViewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetUserPostsAsync(string userId, int page = 1, int pageSize = 10)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postViewModels = new List<PostViewModel>();
            foreach (var post in posts)
            {
                var postViewModel = _mapper.Map<PostViewModel>(post);
                postViewModel.IsLiked = await IsPostLikedAsync(post.Id, userId);
                postViewModels.Add(postViewModel);
            }

            return postViewModels;
        }

        public async Task<Post> CreatePostAsync(string userId, CreatePostViewModel model)
        {
            var post = new Post
            {
                UserId = userId,
                Content = model.Content,
                Type = model.Type,
                IsPublic = model.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            // Xử lý upload media nếu có
            if (model.MediaFile != null)
            {
                var fileName = $"{userId}_{DateTime.UtcNow.Ticks}_{model.MediaFile.FileName}";
                var filePath = Path.Combine("wwwroot", "uploads", "posts", fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.MediaFile.CopyToAsync(stream);
                }
                
                post.MediaUrl = $"/uploads/posts/{fileName}";
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<bool> UpdatePostAsync(int postId, string userId, CreatePostViewModel model)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId || post.IsDeleted) return false;

            post.Content = model.Content;
            post.Type = model.Type;
            post.IsPublic = model.IsPublic;
            post.UpdatedAt = DateTime.UtcNow;

            // Xử lý upload media mới nếu có
            if (model.MediaFile != null)
            {
                var fileName = $"{userId}_{DateTime.UtcNow.Ticks}_{model.MediaFile.FileName}";
                var filePath = Path.Combine("wwwroot", "uploads", "posts", fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.MediaFile.CopyToAsync(stream);
                }
                
                post.MediaUrl = $"/uploads/posts/{fileName}";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int postId, string userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId) return false;

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LikePostAsync(int postId, string userId)
        {
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

            if (existingLike != null) return false; // Đã like rồi

            var like = new PostLike
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);

            // Cập nhật số lượng like
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.LikeCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(int postId, string userId)
        {
            var like = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

            if (like == null) return false;

            _context.PostLikes.Remove(like);

            // Cập nhật số lượng like
            var post = await _context.Posts.FindAsync(postId);
            if (post != null && post.LikeCount > 0)
            {
                post.LikeCount--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsPostLikedAsync(int postId, string userId)
        {
            return await _context.PostLikes
                .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
        }

        public async Task<Comment> CreateCommentAsync(string userId, CreateCommentViewModel model)
        {
            var comment = new Comment
            {
                PostId = model.PostId,
                UserId = userId,
                Content = model.Content,
                ParentCommentId = model.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            // Cập nhật số lượng comment
            var post = await _context.Posts.FindAsync(model.PostId);
            if (post != null)
            {
                post.CommentCount++;
            }

            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> UpdateCommentAsync(int commentId, string userId, string content)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId || comment.IsDeleted) return false;

            comment.Content = content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId) return false;

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;

            // Cập nhật số lượng comment
            var post = await _context.Posts.FindAsync(comment.PostId);
            if (post != null && post.CommentCount > 0)
            {
                post.CommentCount--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LikeCommentAsync(int commentId, string userId)
        {
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (existingLike != null) return false;

            var like = new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentLikes.Add(like);

            // Cập nhật số lượng like
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.LikeCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeCommentAsync(int commentId, string userId)
        {
            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

            if (like == null) return false;

            _context.CommentLikes.Remove(like);

            // Cập nhật số lượng like
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null && comment.LikeCount > 0)
            {
                comment.LikeCount--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCommentLikedAsync(int commentId, string userId)
        {
            return await _context.CommentLikes
                .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
        }

        public async Task<IEnumerable<CommentViewModel>> GetPostCommentsAsync(int postId, int page = 1, int pageSize = 20)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .Include(c => c.Likes)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var commentViewModels = new List<CommentViewModel>();
            foreach (var comment in comments)
            {
                var commentViewModel = _mapper.Map<CommentViewModel>(comment);
                commentViewModels.Add(commentViewModel);
            }

            return commentViewModels;
        }

        public async Task<IEnumerable<PostViewModel>> SearchPostsAsync(string query, int page = 1, int pageSize = 10)
        {
            var posts = await _context.Posts
                .Where(p => !p.IsDeleted && p.IsPublic && p.Content.Contains(query))
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PostViewModel>>(posts);
        }
    }
}
