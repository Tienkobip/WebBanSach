using Microsoft.AspNetCore.Identity;
using WebApp.Api.Entities;
using WebApp.Api.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<UserProfileDto>.FailureResult("Người dùng không tồn tại hoặc đã bị khóa.");
            }

            // Map thông tin Entity sang UserProfileDto
            var profileDto = new UserProfileDto(
                UserId: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                PhoneNumber: user.PhoneNumber,
                DateOfBirth: user.DateOfBirth,
                Address: user.Address,
                AvatarUrl: user.AvatarUrl,
                TotalOrders: 0,        // Phía Admin tạm để 0
                TotalWishlistItems: 0  // Phía Admin tạm để 0
            );

            return ApiResponse<UserProfileDto>.SuccessResult(profileDto);
        }

        public async Task<ApiResponse<bool>> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            try
            {
                // 1. Cập nhật các thông tin cơ bản
                user.UpdateProfile(dto.FullName, dto.Address, dto.DateOfBirth);
                // 2. Xử lý lưu File Ảnh Avatar An Toàn (Nếu có gửi dữ liệu Base64)
                if (!string.IsNullOrEmpty(dto.AvatarBase64))
                {
                    var base64Data = dto.AvatarBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    var bytes = Convert.FromBase64String(base64Data);
                    // Kiểm tra dung lượng tối đa 5MB
                    if (bytes.Length > 5 * 1024 * 1024)
                    {
                        return ApiResponse<bool>.FailureResult("Kích thước file ảnh không được vượt quá 5MB.");
                    }
                    // Kiểm tra Magic Bytes nhị phân (Chống upload file độc giả dạng ảnh)
                    if (!IsValidImageMagicBytes(bytes))
                    {
                        return ApiResponse<bool>.FailureResult("Định dạng file không hợp lệ hoặc bị nghi ngờ chứa mã độc.");
                    }

                    // Đổi tên file thành GUID ngẫu nhiên (Chống Path Traversal)
                    /*
                        Ý nghĩa: Tạo ra một tên file hoàn toàn ngẫu nhiên và duy nhất 
                            bằng mã GUID.

                        ⚠️ Nếu thiếu: Nếu dùng tên file do người dùng gửi lên, 
                            hacker có thể đặt tên file là ../../appsettings.json 
                            hoặc ../../Program.cs. Khi Server lưu file, nó sẽ 
                            ghi đè và phá hủy mã nguồn hoặc làm lộ mật khẩu Database của bạn!
                     */
                    var safeFileName = $"avatar_{user.Id}_{Guid.NewGuid():N}.jpg";

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // 🔥 BƯỚC DỌN RÁC TỰ ĐỘNG: Xóa ảnh cũ của User này khỏi đĩa cứng (nếu đã từng có ảnh trước đó)
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath); // Xóa sạch file ảnh cũ, giải phóng dung lượng đĩa cứng!
                        }
                    }

                    var filePath = Path.Combine(uploadsFolder, safeFileName);
                    await File.WriteAllBytesAsync(filePath, bytes);
                    // Gán AvatarUrl tương đối để lưu xuống SQL Server Database
                    // Call Domain Method vừa tạo
                    user.UpdateAvatar($"/images/avatars/{safeFileName}");
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.FailureResult("Không thể cập nhật hồ sơ.", errors);
                }

                return ApiResponse<bool>.SuccessResult(true, "Cập nhật hồ sơ cá nhân thành công.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Đã xảy ra lỗi khi cập nhật hồ sơ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<bool>.FailureResult("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            // Sử dụng tính năng ChangePasswordAsync tích hợp sẵn của Identity
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.FailureResult("Đổi mật khẩu thất bại.", errors);
            }

            return ApiResponse<bool>.SuccessResult(true, "Đổi mật khẩu thành công.");
        }

        // Hàm kiểm tra Chữ ký Nhị phân (Magic Bytes) của ảnh thật
        /*
            Ý nghĩa: Soi 2 đến 4 byte đầu tiên của tệp nhị phân. 
                Bất kỳ file ảnh nào trên thế giới cũng bắt đầu bằng chữ ký số nhị phân:
                .)File JPG bắt buộc phải bắt đầu bằng: FF D8 FF
                .)File PNG bắt buộc phải bắt đầu bằng: 89 50 4E 47
                .)File WEBP bắt buộc phải bắt đầu bằng: RIFF...WEBP

            ⚠️ Nếu thiếu (LỖ HỔNG BẢO MẬT CỰC NGUY HIỂM): Hacker viết một con 
                virus mã độc (ví dụ file trojan.exe hoặc script shell.php/asp), 
                sau đó đổi tên đuôi file thành avatar.jpg để lừa bộ lọc. Nếu không soi 
                Magic Bytes, Server sẽ lưu file virus này vào wwwroot. Sau đó 
                hacker chỉ cần kích hoạt file đó là chiếm toàn quyền kiểm soát máy chủ của bạn!
         */
        private bool IsValidImageMagicBytes(byte[] bytes)
        {
            if (bytes.Length < 4) return false;
            // JPEG/JPG: FF D8 FF
            var isJpeg = bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
            // PNG: 89 50 4E 47
            var isPng = bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
            // WEBP: RIFF...WEBP
            var isWebp = bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46;
            return isJpeg || isPng || isWebp;
        }
    }
}
