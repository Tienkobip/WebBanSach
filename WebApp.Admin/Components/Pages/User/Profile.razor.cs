using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel;
using WebApp.Admin.Auth;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Admin.Components.Pages.User
{
    public partial class Profile
    {
        // DTO chứa toàn bộ thông tin Read-only từ DB để hiển thị UI
        private UserProfileDto? userProfileModel;
        // Model cho tab Thông tin cá nhân
        private UpdateProfileDto updateProfileModel = new();

        // Model cho tab Đổi mật khẩu
        private ChangePasswordDto passwordModel = new();

        // Trạng thái UI
        private bool isLoading = true;
        private bool isSubmittingProfile = false;
        private bool isSubmittingPassword = false;
        private string? errorMessage;
        private string? successMessage;
        private string ActiveTab { get; set; } = "info";
        private bool ShowCurrentPwd { get; set; } = false;
        private bool ShowNewPwd { get; set; } = false;
        private bool ShowConfirmPwd { get; set; } = false;
        private IBrowserFile? selectedAvatarFile;
        private string? avatarBase64Data; // Lưu tạm chuỗi Base64
        private string? avatarPreviewUrl;

        private void ChangeTab(string tabName) => ActiveTab = tabName;
        private void ToggleCurrentPwd() => ShowCurrentPwd = !ShowCurrentPwd;
        private void ToggleNewPwd() => ShowNewPwd = !ShowNewPwd;
        private void ToggleConfirmPwd() => ShowConfirmPwd = !ShowConfirmPwd;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserProfileAsync();
        }

        private async Task LoadUserProfileAsync()
        {
            isLoading = true;
            errorMessage = null;

            try
            {
                // Gọi Service lấy thông tin User từ DB qua API Backend
                var response = await UserClientService.GetUserProfileAsync();

                if (response.Success && response.Data != null)
                {
                    var userProfile = response.Data;

                    // Map dữ liệu từ API vào Model hiển thị trên form
                    updateProfileModel = new UpdateProfileDto
                    {
                        FullName = userProfile.FullName,
                        DateOfBirth = userProfile.DateOfBirth,
                        Address = userProfile.Address
                    };

                    userProfileModel = userProfile;
                }
                else
                {
                    errorMessage = response?.Message ?? "Không thể tải thông tin hồ sơ.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        // 1. Hàm chạy khi người dùng chọn file ảnh từ máy tính
        private async Task HandleAvatarSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file != null)
            {
                // Kiểm tra kích thước (ví dụ tối đa 5MB)
                if (file.Size > 5 * 1024 * 1024)
                {
                    errorMessage = "Kích thước ảnh không được vượt quá 5MB!";
                    return;
                }

                selectedAvatarFile = file;

                // Đọc file thành base64 để hiển thị Preview ngay lên giao diện
                using var ms = new MemoryStream();
                using (var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024))
                {
                    await stream.CopyToAsync(ms);
                }

                var buffer = ms.ToArray();
                avatarBase64Data = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
                avatarPreviewUrl = avatarBase64Data;
                errorMessage = string.Empty;
            }
        }

        // Xử lý Cập nhật Hồ sơ Cá nhân
        private async Task HandleUpdateProfileAsync()
        {
            isSubmittingProfile = true;
            errorMessage = null;
            successMessage = null;

            // 🔥 ĐÓNG GÓI CHUỖI BASE64 VÀO MODEL ĐỂ GỬI SANG API
            if (!string.IsNullOrEmpty(avatarPreviewUrl) && selectedAvatarFile != null)
            {
                updateProfileModel.AvatarBase64 = avatarPreviewUrl;
                updateProfileModel.AvatarFileName = selectedAvatarFile.Name;
            }

            var result = await UserClientService.UpdateUserProfileAsync(updateProfileModel);

            if (result.Success)
            {
                successMessage = "Cập nhật thông tin hồ sơ thành công!";
                avatarBase64Data = null;
                await LoadUserProfileAsync();
                avatarPreviewUrl = null;
                selectedAvatarFile = null;

                // 🔥 THÔNG BÁO CHO TOPHEADER CẬP NHẬT LẠI AVATAR MỚI TỨC THÌ
                if (userProfileModel != null && AuthStateProvider is CustomAuthStateProvider customAuth)
                {
                    customAuth.NotifyUserAuthentication(new AuthResponseDto(
                        UserId: userProfileModel.UserId,
                        FullName: userProfileModel.FullName,
                        Email: userProfileModel.Email,
                        AvatarUrl: userProfileModel.AvatarUrl,
                        Roles: new List<string>()
                    ));
                }
            }
            else
            {
                errorMessage = result?.Message ?? "Cập nhật hồ sơ thất bại.";
            }

            isSubmittingProfile = false;
        }

        // 3. Hàm tính toán đường dẫn hiển thị ảnh Avatar chuẩn xác
        private string GetAvatarDisplayUrl()
        {
            if (!string.IsNullOrEmpty(avatarPreviewUrl))
            {
                return avatarPreviewUrl;
            }
            if (!string.IsNullOrEmpty(userProfileModel?.AvatarUrl))
            {
                return userProfileModel.AvatarUrl.StartsWith("http")
                    ? userProfileModel.AvatarUrl
                    : $"https://localhost:7188{userProfileModel.AvatarUrl}";
            }
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(userProfileModel?.FullName ?? "Admin")}&background=E31837&color=fff&size=128";
        }

        // Xử lý Đổi Mật Khẩu
        private async Task HandleChangePasswordAsync()
        {
            isSubmittingPassword = true;
            errorMessage = null;
            successMessage = null;

            var result = await UserClientService.ChangePasswordAsync(passwordModel);

            if (result.Success)
            {
                successMessage = "Đổi mật khẩu thành công!";
                passwordModel = new ChangePasswordDto(); // Reset form mật khẩu
            }
            else
            {
                errorMessage = result?.Message ?? "Đổi mật khẩu thất bại.";
            }

            isSubmittingPassword = false;
        }

        private void OpenChangeEmailModal()
        {

        }

        private void OpenChangePhoneModal()
        {

        }
    }
}
