namespace WebApp.Shared.Dtos.Management.Auth
{
    // Component dùng: AdminLogin.razor (Giao diện đăng nhập dành riêng cho Admin / Nhân viên nội bộ)
    public record AdminLoginDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public AdminLoginDto() { }

        public AdminLoginDto(string usernameOrEmail, string password)
        {
            UsernameOrEmail = usernameOrEmail;
            Password = password;
        }
    }
}
