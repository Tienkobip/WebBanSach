using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using WebApp.Admin.Services.Interfaces;
using WebApp.Shared.Dtos.Common;

namespace WebApp.Admin.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthClientService _authService;
        private AuthenticationState _anonymousState => new(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthStateProvider(IAuthClientService authService)
        {
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var response = await _authService.GetCurrentUserAsync();

            if (!response.Success || response.Data == null)
            {
                return _anonymousState;
            }

            var userDto = response.Data;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId ?? ""),
                new Claim(ClaimTypes.Name, userDto.FullName ?? ""),
                new Claim(ClaimTypes.Email, userDto.Email ?? ""),
                new Claim("AvatarUrl", userDto.AvatarUrl ?? "")
            };

            if (userDto.Roles != null)
            {
                foreach (var role in userDto.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var userPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationState(userPrincipal);
        }

        /// <summary>
        /// Thông báo Blazor UI cập nhập lại dựa trạng thái Đăng nhập/Đăng xuất
        /// </summary>
        public void NotifyUserAuthentication(AuthResponseDto userDto)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId ?? ""),
                new Claim(ClaimTypes.Name, userDto.FullName ?? ""),
                new Claim(ClaimTypes.Email, userDto.Email ?? "")
            };

            if (userDto.Roles != null)
            {
                foreach (var role in userDto.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var userPrincipal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
        }
    }
}
