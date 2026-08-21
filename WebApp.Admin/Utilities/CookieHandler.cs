using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApp.Admin.Utilities
{
    // Cấu hình Blazor phải nhúng cookie vào mỗi https Request cho bên API đọc
    public class CookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserSessionState _userSessionState;
        public CookieHandler(
            IHttpContextAccessor httpContextAccessor,
            UserSessionState userSessionState)
        {
            _httpContextAccessor = httpContextAccessor;
            _userSessionState = userSessionState;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                string? cookieValue = _userSessionState.AuthCookieValue;

                if (string.IsNullOrEmpty(cookieValue))
                {
                    var context = _httpContextAccessor.HttpContext;
                    if (context != null && context.Request.Cookies.TryGetValue(".WebBanSach.Auth", out var val))
                    {
                        cookieValue = val;
                        // Lưu ngược lại vào Session State cho các cuộc gọi SignalR về sau
                        _userSessionState.AuthCookieValue = val;
                    }
                }

                if (!string.IsNullOrEmpty(cookieValue))
                {
                    request.Headers.Add("Cookie", $".WebBanSach.Auth={cookieValue}");
                }
                return await base.SendAsync(request, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
            }
            catch (HttpRequestException)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
