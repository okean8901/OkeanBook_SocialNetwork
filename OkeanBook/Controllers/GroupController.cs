using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;
using System.Security.Claims;

namespace OkeanBook.Controllers
{
    /// <summary>
    /// Controller xử lý nhóm
    /// </summary>
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;

        public GroupController(IGroupService groupService, IUserService userService)
        {
            _groupService = groupService;
            _userService = userService;
        }

        /// <summary>
        /// Danh sách nhóm của người dùng
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var groups = await _groupService.GetUserGroupsAsync(userId);
            return View(groups);
        }

        /// <summary>
        /// Chi tiết nhóm
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            var isMember = await _groupService.IsGroupMemberAsync(id, userId);
            if (!isMember)
            {
                return Forbid();
            }

            var members = await _groupService.GetGroupMembersAsync(id);
            var isAdmin = await _groupService.IsGroupAdminAsync(id, userId);
            var isOwner = await _groupService.IsGroupOwnerAsync(id, userId);

            ViewBag.Members = members;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsOwner = isOwner;

            return View(group);
        }

        /// <summary>
        /// Tạo nhóm mới
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Xử lý tạo nhóm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("Name", "Tên nhóm là bắt buộc");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var group = await _groupService.CreateGroupAsync(userId, name, description);
                TempData["Success"] = "Đã tạo nhóm thành công";
                return RedirectToAction("Details", new { id = group.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
                return View();
            }
        }

        /// <summary>
        /// Chỉnh sửa nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            // Chỉ admin mới có thể chỉnh sửa
            var isAdmin = await _groupService.IsGroupAdminAsync(id, userId);
            if (!isAdmin)
            {
                return Forbid();
            }

            return View(group);
        }

        /// <summary>
        /// Cập nhật nhóm
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string? description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("Name", "Tên nhóm là bắt buộc");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _groupService.UpdateGroupAsync(id, userId, name, description);
                if (result)
                {
                    TempData["Success"] = "Đã cập nhật nhóm thành công";
                    return RedirectToAction("Details", new { id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không có quyền chỉnh sửa nhóm này");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
            }

            return View();
        }

        /// <summary>
        /// Xóa nhóm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _groupService.DeleteGroupAsync(id, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã xóa nhóm thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không có quyền xóa nhóm này" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Thêm thành viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddMember(int groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _groupService.AddMemberAsync(groupId, currentUserId, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã thêm thành viên thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không có quyền thêm thành viên" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa thành viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveMember(int groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _groupService.RemoveMemberAsync(groupId, currentUserId, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã xóa thành viên thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không có quyền xóa thành viên này" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Rời khỏi nhóm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _groupService.LeaveGroupAsync(groupId, userId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã rời khỏi nhóm" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể rời khỏi nhóm này" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật vai trò thành viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateMemberRole(int groupId, string userId, GroupRole role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _groupService.UpdateMemberRoleAsync(groupId, currentUserId, userId, role);
                if (result)
                {
                    return Json(new { success = true, message = "Đã cập nhật vai trò thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không có quyền cập nhật vai trò" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
