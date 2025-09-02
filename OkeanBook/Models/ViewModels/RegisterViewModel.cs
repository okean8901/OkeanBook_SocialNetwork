using System.ComponentModel.DataAnnotations;

namespace OkeanBook.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên người dùng không được vượt quá 50 ký tự")]
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Giới thiệu không được vượt quá 500 ký tự")]
        [Display(Name = "Giới thiệu bản thân")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        [Display(Name = "Đồng ý với điều khoản")]
        public bool AgreeToTerms { get; set; }
    }
}
