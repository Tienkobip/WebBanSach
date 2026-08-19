namespace WebApp.Shared.Dtos.Management.Auth
{
    // Component dùng: ResetPassword.razor (Đặt lại mật khẩu mới bằng mã OTP)
    public record AdminResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;

        public AdminResetPasswordDto() { }
        public AdminResetPasswordDto(string email, string otpCode, string newPassword, string confirmNewPassword)
        {
            Email = email;
            OtpCode = otpCode;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}
