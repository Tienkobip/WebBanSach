using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId);
        Task<ApiResponse<bool>> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
