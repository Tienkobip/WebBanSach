namespace WebApp.Shared.Dtos.User
{
    // Component dùng: ChangePassword.razor (Đổi mật khẩu tài khoản cá nhân)
    public record ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;

        public ChangePasswordDto() { }

        public ChangePasswordDto(string currentPassword, string newPassword, string confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
        
}
