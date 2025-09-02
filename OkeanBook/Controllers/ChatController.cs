using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkeanBook.Models.ViewModels;
using OkeanBook.Services.Interfaces;
using System.Security.Claims;

namespace OkeanBook.Controllers
{
    /// <summary>
    /// Controller xử lý chat
    /// </summary>
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IFriendService _friendService;

        public ChatController(
            IMessageService messageService,
            IUserService userService,
            IGroupService groupService,
            IFriendService friendService)
        {
            _messageService = messageService;
            _userService = userService;
            _groupService = groupService;
            _friendService = friendService;
        }

        /// <summary>
        /// Trang chat chính
        /// </summary>
        public async Task<IActionResult> Index(string? chatId = null, bool isGroup = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var chatViewModel = new ChatViewModel
            {
                CurrentUserId = userId,
                SelectedChatId = chatId,
                IsGroupChat = isGroup
            };

            // Lấy danh sách bạn bè
            var friends = await _userService.GetFriendsAsync(userId);
            chatViewModel.Friends = friends.Select(f => new ChatUserViewModel
            {
                Id = f.Id,
                UserName = f.UserName,
                Avatar = f.Avatar,
                Status = f.Status,
                LastSeen = f.LastSeen
            }).ToList();

            // Lấy danh sách nhóm
            chatViewModel.Groups = (await _groupService.GetUserGroupsAsync(userId)).ToList();

            // Nếu có chat được chọn, lấy tin nhắn
            if (!string.IsNullOrEmpty(chatId))
            {
                if (isGroup && int.TryParse(chatId, out int groupId))
                {
                    chatViewModel.Messages = (await _messageService.GetGroupMessagesAsync(groupId)).ToList();
                }
                else if (!isGroup)
                {
                    chatViewModel.Messages = (await _messageService.GetMessagesAsync(userId, chatId)).ToList();
                }
            }

            return View(chatViewModel);
        }

        /// <summary>
        /// Lấy tin nhắn với một người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMessages(string friendId, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var messages = await _messageService.GetMessagesAsync(userId, friendId, page, 50);
                return Json(new { success = true, messages = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tin nhắn nhóm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGroupMessages(int groupId, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var messages = await _messageService.GetGroupMessagesAsync(groupId, page, 50);
                return Json(new { success = true, messages = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string friendId, bool isGroup = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            try
            {
                bool result;
                if (isGroup && int.TryParse(friendId, out int groupId))
                {
                    result = await _messageService.MarkGroupMessagesAsReadAsync(userId, groupId);
                }
                else
                {
                    result = await _messageService.MarkMessagesAsReadAsync(userId, friendId);
                }

                return Json(new { success = result });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Thu hồi tin nhắn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecallMessage(int messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _messageService.RecallMessageAsync(messageId, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa tin nhắn
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var result = await _messageService.DeleteMessageAsync(messageId, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách chat
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChatList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            try
            {
                var friends = await _messageService.GetChatListAsync(userId);
                var groups = await _messageService.GetGroupChatListAsync(userId);

                return Json(new { 
                    success = true, 
                    friends = friends,
                    groups = groups
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy số tin nhắn chưa đọc
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            try
            {
                var unreadCount = await _messageService.GetUnreadMessageCountAsync(userId);
                return Json(new { success = true, count = unreadCount });
            }
            catch
            {
                return Json(new { success = false, count = 0 });
            }
        }
    }
}
