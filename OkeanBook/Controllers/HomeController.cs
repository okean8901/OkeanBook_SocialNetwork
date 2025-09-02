using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;
using System.Security.Claims;

namespace OkeanBook.Controllers
{
    /// <summary>
    /// Controller trang chủ và dashboard
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public HomeController(
            IPostService postService,
            IUserService userService,
            INotificationService notificationService)
        {
            _postService = postService;
            _userService = userService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Trang chủ - News Feed
        /// </summary>
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var posts = await _postService.GetNewsFeedAsync(userId, page, 10);
            var unreadNotificationCount = await _notificationService.GetUnreadNotificationCountAsync(userId);

            ViewBag.UnreadNotificationCount = unreadNotificationCount;
            ViewBag.CurrentPage = page;

            return View(posts);
        }

        /// <summary>
        /// Tạo bài viết mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _postService.CreatePostAsync(userId, model);
                TempData["Success"] = "Đã tạo bài viết thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo bài viết: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Like/Unlike bài viết
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var isLiked = await _postService.IsPostLikedAsync(postId, userId);
                bool result;

                if (isLiked)
                {
                    result = await _postService.UnlikePostAsync(postId, userId);
                }
                else
                {
                    result = await _postService.LikePostAsync(postId, userId);
                }

                if (result)
                {
                    var post = await _postService.GetPostByIdAsync(postId);
                    return Json(new { 
                        success = true, 
                        isLiked = !isLiked,
                        likeCount = post?.LikeCount ?? 0 
                    });
                }

                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo bình luận
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Nội dung bình luận không hợp lệ";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _postService.CreateCommentAsync(userId, model);
                TempData["Success"] = "Đã thêm bình luận";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Tìm kiếm bài viết
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            var posts = await _postService.SearchPostsAsync(query, page, 10);
            ViewBag.Query = query;
            ViewBag.CurrentPage = page;

            return View("Index", posts);
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    avatar = user.Avatar,
                    status = user.Status.ToString()
                }
            });
        }
    }
}
