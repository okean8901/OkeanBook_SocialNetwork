using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;
using System.Security.Claims;

namespace OkeanBook.Controllers
{
    /// <summary>
    /// Controller xử lý bạn bè
    /// </summary>
    [Authorize]
    public class FriendController : Controller
    {
        private readonly IFriendService _friendService;
        private readonly IUserService _userService;

        public FriendController(IFriendService friendService, IUserService userService)
        {
            _friendService = friendService;
            _userService = userService;
        }

        /// <summary>
        /// Trang quản lý bạn bè
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var friends = await _friendService.GetFriendsAsync(userId);
            var friendRequests = await _friendService.GetFriendRequestsAsync(userId);

            ViewBag.Friends = friends;
            ViewBag.FriendRequests = friendRequests;

            return View();
        }

        /// <summary>
        /// Tìm kiếm người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var users = await _userService.SearchUsersAsync(query, userId);
                return Json(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gửi lời mời kết bạn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (userId == friendId)
            {
                return Json(new { success = false, message = "Không thể gửi lời mời cho chính mình" });
            }

            try
            {
                var result = await _friendService.SendFriendRequestAsync(userId, friendId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã gửi lời mời kết bạn" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể gửi lời mời kết bạn" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Chấp nhận lời mời kết bạn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _friendService.AcceptFriendRequestAsync(userId, friendId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã chấp nhận lời mời kết bạn" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể chấp nhận lời mời" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Từ chối lời mời kết bạn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeclineFriendRequest(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _friendService.DeclineFriendRequestAsync(userId, friendId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã từ chối lời mời kết bạn" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể từ chối lời mời" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa bạn bè
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _friendService.RemoveFriendAsync(userId, friendId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã xóa bạn bè" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa bạn bè" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Chặn người dùng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (currentUserId == userId)
            {
                return Json(new { success = false, message = "Không thể chặn chính mình" });
            }

            try
            {
                var result = await _friendService.BlockUserAsync(currentUserId, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã chặn người dùng" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể chặn người dùng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bỏ chặn người dùng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _friendService.UnblockUserAsync(currentUserId, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã bỏ chặn người dùng" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể bỏ chặn người dùng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy trạng thái bạn bè
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFriendStatus(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            try
            {
                var status = await _friendService.GetFriendStatusAsync(userId, friendId);
                return Json(new { success = true, status = status?.ToString() });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
