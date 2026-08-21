using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Admin.Services.Interfaces
{
    public interface IUserClientService
    {
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync();
        Task<ApiResponse<bool>> UpdateUserProfileAsync(UpdateProfileDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto);
    }
}
