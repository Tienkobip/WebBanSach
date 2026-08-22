using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Common;

namespace WebApp.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    }
}
