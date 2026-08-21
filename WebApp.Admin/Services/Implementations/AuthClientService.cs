using WebApp.Admin.Services.Base;
using WebApp.Admin.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Auth;
using WebApp.Shared.Dtos.User;

namespace WebApp.Admin.Services.Implementations
{
    public class AuthClientService : BaseApiClient, IAuthClientService
    {
        private readonly HttpClient _httpClient;
        public AuthClientService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        // <summary>
        // Xử lý đăng nhập, đăng xuất cho management và lấy thông tin người dùng hiện tại
        // </summary>
        public Task<ApiResponse<AuthResponseDto>> LoginAsync(AdminLoginDto loginDto)
        { 
            return ExecuteApiAsync<AuthResponseDto>(() => _httpClient.PostAsJsonAsync("api/auth/admin-login", loginDto)); 
        }

        public Task<ApiResponse<AuthResponseDto>> GetCurrentUserAsync()
        { 
            return ExecuteApiAsync<AuthResponseDto>(() => _httpClient.GetAsync("api/auth/admin-me"));
        }

        public Task<ApiResponse<bool>> LogoutAsync() 
        { 
            return ExecuteApiAsync<bool>(() => _httpClient.PostAsync("api/auth/admin-logout", null));
        }


        // <summary>
        // Xử lý Forgot Password, Verify OTP và Reset Password cho management
        // </summary>
        public Task<ApiResponse<bool>> ForgotPasswordAsync(AdminForgotPasswordDto dto) 
        { 
            return ExecuteApiAsync<bool>(() => _httpClient.PostAsJsonAsync("api/auth/admin-forgot-password", dto));
        }

        public Task<ApiResponse<bool>> VerifyOtpAsync(AdminVerifyOtpDto dto) 
        { 
            return ExecuteApiAsync<bool>(() => _httpClient.PostAsJsonAsync("api/auth/admin-verify-otp", dto));
        }

        public Task<ApiResponse<bool>> ResetPasswordAsync(AdminResetPasswordDto dto) 
        { 
            return ExecuteApiAsync<bool>(() => _httpClient.PostAsJsonAsync("api/auth/admin-reset-password", dto));
        }

        // <summary>
        // Xử lý lấy thông tin hồ sơ người dùng, cập nhật hồ sơ và đổi mật khẩu cho management
        // </summary>

        public Task<ApiResponse<UserProfileDto>> GetUserProfileAsync()
        {
            return ExecuteApiAsync<UserProfileDto>(() => _httpClient.GetAsync("api/user/profile"));
        }

        public Task<ApiResponse<bool>> UpdateUserProfileAsync(UpdateProfileDto dto)
        {
            return ExecuteApiAsync<bool>(() => _httpClient.PutAsJsonAsync("api/user/profile", dto));
        }

        public Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            return ExecuteApiAsync<bool>(() => _httpClient.PostAsJsonAsync("api/user/change-password", dto));
        }
    }
}
