using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Api.Services.Interfaces
{
    public interface IAdminAuthService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAdminAsync(AdminLoginDto dto);
        Task<ApiResponse<AuthResponseDto>> GetCurrentUserAsync(string userId);
        Task<ApiResponse<bool>> ForgotPasswordAsync(AdminForgotPasswordDto dto);
        Task<ApiResponse<bool>> ResetPasswordAsync(AdminResetPasswordDto dto);
        Task<ApiResponse<bool>> VerifyOtpAsync(AdminVerifyOtpDto dto);
    }
}
