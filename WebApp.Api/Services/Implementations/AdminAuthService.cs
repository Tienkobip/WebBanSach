using Microsoft.AspNetCore.Identity;
using WebApp.Api.Services.Interfaces;
using WebApp.Api.Entities;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Auth;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using WebApp.Api.Utilities;

namespace WebApp.Api.Services.Implementations
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;

        public AdminAuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAdminAsync(AdminLoginDto dto)
        {
            // Kiểm tra tồn tại người dùng theo email hoặc tên đăng nhập
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.UsernameOrEmail);
            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.FailureResult("Tài khoản hoặc mật khẩu không chính xác.");
            }

            try
            {
                user.EnsureCanLogin();
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(ex.Message);
            }

            // Kiểm tra mật khẩu và kiểm tra xem tài khoản người dùng có bị khóa hay không
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (signInResult.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingMinutes = lockoutEnd.HasValue
                    ? Math.Max(1, (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes))
                    : 15;
                return ApiResponse<AuthResponseDto>.FailureResult($"Tài khoản đã bị tạm khóa do nhập sai mật khẩu 5 lần liên tiếp. Vui lòng thử lại sau {remainingMinutes} phút.");
            }

            if (!signInResult.Succeeded)
            {
                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                var remainingAttempts = 5 - failedAttempts;
                string warningMsg = remainingAttempts > 0
                    ? $"Bạn còn {remainingAttempts} lần thử trước khi tài khoản bị khóa."
                    : "Tài khoản của bạn đã bị khóa do nhập sai mật khẩu quá nhiều lần.";
                return ApiResponse<AuthResponseDto>.FailureResult(warningMsg);
            }

            // Đăng nhập thành công
            try
            {
                user.RecordLoginSuccess();
                await _userManager.UpdateAsync(user);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(ex.Message);
            }

            // Lấy role của user hiện tại và lưu vào cookie
            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            // Đọc các claims và thêm cookie được mã hóa vào Header Set-Cookie
            await WriteAuthCookieAsync(user, roles);

            var authResponse = new AuthResponseDto(
                UserId: user.Id,
                FullName: user.UserName,
                Email: user.Email,
                AvatarUrl: user.AvatarUrl,
                Roles: roles
            );
            return ApiResponse<AuthResponseDto>.SuccessResult(authResponse);
        }

        public async Task<ApiResponse<AuthResponseDto>> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailureResult("Người dùng không tồn tại hoặc tài khoản đã bị vô hiệu hóa.");
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var authResponse = new AuthResponseDto(
                UserId: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                AvatarUrl: user.AvatarUrl,
                Roles: roles
            );
            return ApiResponse<AuthResponseDto>.SuccessResult(authResponse);
        }

        private async Task WriteAuthCookieAsync(User user, List<string> roles)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await httpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(AdminForgotPasswordDto dto)
        {
            // Kiểm tra tồn tại người dùng theo email hoặc tên đăng nhập
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Email không tồn tại hoặc tài khoản đã bị vô hiệu hóa.");
            }

            // Sinh mã OTP 6 số sử dụng Email Token Provider mặc định của Identity
            var otpCode = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);


            var htmlMessage = EmailTemplates.GetOtpTemplate(otpCode);

            // Gửi email chứa mã OTP
            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu", htmlMessage);
            }
            catch (Exception)
            {
                return ApiResponse<bool>.FailureResult("Không thể gửi email xác thực. Vui lòng kiểm tra lại hệ thống SMTP hoặc thử lại sau.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Mã OTP đã được gửi đến email của bạn.");
        }

        public async Task<ApiResponse<bool>> VerifyOtpAsync(AdminVerifyOtpDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !user.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Thông tin yêu cầu không hợp lệ.");
            }

            // Xác thực mã OTP 6 số
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, dto.OtpCode);
            if (!isValid)
            {
                return ApiResponse<bool>.FailureResult("Mã OTP không chính xác hoặc đã hết hạn.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Xác thực mã OTP thành công.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(AdminResetPasswordDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !user.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Thông tin yêu cầu không hợp lệ.");
            }

            // Kiểm tra lại OTP ở bước cuối cùng để ngăn chặn việc Bypass API
            var isValidOtp = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, dto.OtpCode);
            if (!isValidOtp)
            {
                return ApiResponse<bool>.FailureResult("Mã OTP không chính xác hoặc đã hết hạn.");
            }

            // Tạo Identity PasswordResetToken ngầm bên trong để thực hiện Reset Password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.FailureResult("Đặt lại mật khẩu thất bại.", errors);
            }

            // Đặt lại số lần đăng nhập sai và mở khóa tài khoản (nếu đang bị Lockout)
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            return ApiResponse<bool>.SuccessResult(true, "Đặt lại mật khẩu thành công.");
        }
    }
}