using System.Net;
using System.Net.Http.Json;
using WebApp.Shared.Dtos.Common;
namespace WebApp.Admin.Services.Base
{
    public abstract class BaseApiClient
    {
        /// <summary>
        /// Hàm bọc dùng chung cho toàn bộ các API Call trong WebApp.Admin
        /// Tự động kiểm tra lỗi HTTP 401, 403, 500, lỗi mạng và ép kiểu JSON sang ApiResponse<T>
        /// </summary>
        protected async Task<ApiResponse<T>> ExecuteApiAsync<T>(Func<Task<HttpResponseMessage>> apiCall)
        {
            try
            {
                var response = await apiCall();
                // 1. Xử lý trường hợp hết hạn Session / Cookie (401)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return ApiResponse<T>.FailureResult("Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.");
                }
                // 2. Xử lý trường hợp không có quyền (403)
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return ApiResponse<T>.FailureResult("Bạn không có quyền thực hiện thao tác này.");
                }

                // 2. THỬ ĐỌC NỘI DUNG JSON TRẢ VỀ TRƯỚC (Kể cả khi StatusCode là HTTP 400 Bad Request)
                try
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                    if (result != null)
                    {
                        // Trả về trọn vẹn Message & Errors chi tiết từ Backend WebApp.Api!
                        return result;
                    }
                }
                catch
                {
                    // Nếu không đọc được JSON (ví dụ Server sập 500 HTML hoặc 502) thì bỏ qua nhảy xuống bước 3
                }

                // 3. Fallback dự phòng nếu không đọc được JSON body
                if (!response.IsSuccessStatusCode)
                {
                    return ApiResponse<T>.FailureResult($"Máy chủ phản hồi lỗi (HTTP {(int)response.StatusCode}).");
                }
                
                return ApiResponse<T>.FailureResult("Không thể đọc dữ liệu phản hồi từ máy chủ.");
            }
            catch (OperationCanceledException)
            {
                // Bắt lỗi khi request bị hủy (ví dụ: timeout)
                return ApiResponse<T>.FailureResult("Yêu cầu đã bị hủy. Vui lòng thử lại.");
            }
            catch (HttpRequestException ex)
            {
                // Bắt lỗi không kết nối được tới API (mất mạng hoặc Server API sập)
                return ApiResponse<T>.FailureResult($"Không thể kết nối đến máy chủ: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống không lường trước
                return ApiResponse<T>.FailureResult($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}