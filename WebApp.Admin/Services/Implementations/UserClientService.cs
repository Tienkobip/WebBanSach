using WebApp.Admin.Services.Base;
using WebApp.Admin.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Admin.Services.Implementations
{
    public class UserClientService : BaseApiClient, IUserClientService
    {
        private readonly HttpClient _httpClient;
        public UserClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
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
