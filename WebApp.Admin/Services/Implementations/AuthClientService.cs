using WebApp.Admin.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Admin.Services.Implementations
{
    public class AuthClientService : IAuthClientService
    {
        private readonly HttpClient _httpClient;
        public AuthClientService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        // <summary>
        // Xử lý đăng nhập, đăng xuất cho management và lấy thông tin người dùng hiện tại
        // </summary>
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(AdminLoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/admin-login", loginDto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                return result ?? ApiResponse<AuthResponseDto>.FailureResult("Lỗi khi đọc phản hồi từ Server.");

            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auth/admin-me");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                return result ?? ApiResponse<AuthResponseDto>.FailureResult("Lỗi khi đọc phản hồi từ Server.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/auth/admin-logout", null);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? ApiResponse<bool>.FailureResult("Lỗi khi đọc phản hồi từ Server.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }


        // <summary>
        // Xử lý Forgot Password, Verify OTP và Reset Password cho management
        // </summary>
        public async Task<ApiResponse<bool>> ForgotPasswordAsync(AdminForgotPasswordDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/admin-forgot-password", dto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? ApiResponse<bool>.FailureResult("Lỗi khi đọc phản hồi từ Server.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> VerifyOtpAsync(AdminVerifyOtpDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/admin-verify-otp", dto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? ApiResponse<bool>.FailureResult("Lỗi khi đọc phản hồi từ Server.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(AdminResetPasswordDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/admin-reset-password", dto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? ApiResponse<bool>.FailureResult("Lỗi khi đọc phản hồi từ Server.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Lỗi kết nối máy chủ: {ex.Message}");
            }
        }
    }
}
