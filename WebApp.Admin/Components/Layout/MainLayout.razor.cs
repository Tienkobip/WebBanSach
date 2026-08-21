using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WebApp.Admin.Auth;
using WebApp.Admin.Services.Implementations;

namespace WebApp.Admin.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        private bool isProfileDropdownOpen = false;
        private bool isNotiOpen = false;
        private bool isMobileMenuOpen = false;
        private string userName = "Quản Trị Viên";
        private string userRole = "N/A";
        private string avatarUrl = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            // Lắng nghe sự kiện khi thông tin User thay đổi (ví dụ: vừa lưu Avatar mới)
            AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
            await LoadUserInfoFromClaimsAsync();
        }

        private async void OnAuthStateChanged(Task<AuthenticationState> task)
        {
            await LoadUserInfoFromClaimsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadUserInfoFromClaimsAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Người dùng";
                var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                userRole = roles.Any() ? string.Join(", ", roles) : "Nhân viên";
                var avatarClaim = user.FindFirst("AvatarUrl")?.Value;

                if (!string.IsNullOrEmpty(avatarClaim))
                {
                    // Nối Domain API nếu là đường dẫn tương đối /images/avatars/...
                    avatarUrl = avatarClaim.StartsWith("http") ? avatarClaim : $"https://localhost:7188{avatarClaim}";
                }
                else
                {
                    avatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(userName)}&background=E31837&color=fff";
                }
            }
        }

        public void Dispose()
        {
            // Hủy đăng ký sự kiện khi Component bị hủy để tránh rò rỉ bộ nhớ
            AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
        }

        private void ToggleProfileDropdown() 
        { 
            isProfileDropdownOpen = !isProfileDropdownOpen; 
            isNotiOpen = false; 
        }

        private void ToggleNotifications() 
        { 
            isNotiOpen = !isNotiOpen; 
            isProfileDropdownOpen = false; 
        }

        private void ToggleMobileMenu() => isMobileMenuOpen = !isMobileMenuOpen;

        // Hàm dùng để đóng tất cả các dropdown khi bấm ra ngoài hoặc click chọn menu item
        private void CloseAllDropdowns()
        {
            isProfileDropdownOpen = false;
            isNotiOpen = false;
        }
    }
}
