using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;
using System.Security.Claims;

namespace OkeanBook.Controllers
{
    /// <summary>
    /// Controller xử lý hồ sơ người dùng
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IFriendService _friendService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            IUserService userService,
            IPostService postService,
            IFriendService friendService,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _postService = postService;
            _friendService = friendService;
            _userManager = userManager;
        }

        /// <summary>
        /// Xem hồ sơ người dùng
        /// </summary>
        public async Task<IActionResult> Index(string? userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Nếu không có userId, hiển thị hồ sơ của chính mình
            var targetUserId = userId ?? currentUserId;
            var isOwnProfile = targetUserId == currentUserId;

            var user = await _userService.GetUserByIdAsync(targetUserId);
            if (user == null)
            {
                return NotFound();
            }

            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Avatar = user.Avatar,
                Bio = user.Bio,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                LastSeen = user.LastSeen
            };

            // Lấy bài viết của người dùng
            var posts = await _postService.GetUserPostsAsync(targetUserId, 1, 10);

            // Lấy thông tin bạn bè nếu không phải hồ sơ của chính mình
            FriendStatus? friendStatus = null;
            if (!isOwnProfile)
            {
                friendStatus = await _friendService.GetFriendStatusAsync(currentUserId, targetUserId);
            }

            ViewBag.Posts = posts;
            ViewBag.IsOwnProfile = isOwnProfile;
            ViewBag.FriendStatus = friendStatus;

            return View(profileViewModel);
        }

        /// <summary>
        /// Chỉnh sửa hồ sơ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var editViewModel = new EditProfileViewModel
            {
                UserName = user.UserName,
                Bio = user.Bio,
                Status = user.Status
            };

            return View(editViewModel);
        }

        /// <summary>
        /// Cập nhật hồ sơ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _userService.UpdateProfileAsync(userId, model);
                if (result)
                {
                    TempData["Success"] = "Đã cập nhật hồ sơ thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật hồ sơ");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Cập nhật mật khẩu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _userService.ChangePasswordAsync(userId, model);
                if (result)
                {
                    TempData["Success"] = "Đã đổi mật khẩu thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }

        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(UserStatus status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _userService.UpdateUserStatusAsync(userId, status);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                return Json(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        username = user.UserName,
                        avatar = user.Avatar,
                        bio = user.Bio,
                        status = user.Status.ToString(),
                        createdAt = user.CreatedAt,
                        lastSeen = user.LastSeen
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
