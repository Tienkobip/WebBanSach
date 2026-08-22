using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Common;

namespace WebApp.Admin.Services.Interfaces
{
    public interface IDashboardClientService
    {
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    }
}
