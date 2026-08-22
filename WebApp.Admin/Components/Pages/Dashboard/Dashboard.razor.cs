using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebApp.Shared.Dtos.Management.Common;

namespace WebApp.Admin.Components.Pages.Dashboard
{
    public partial class Dashboard : IAsyncDisposable
    {
        private DashboardStatsDto? dashBoardStateModel;
        private bool isLoading = true;
        private IJSObjectReference? _jsModule;

        // Tab hiện tại đang chọn: 'revenue' | 'orders' | 'warehouse' | 'customers'
        private string activeTab = "revenue";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // 2. Gọi qua Service Client
                var response = await DashboardClientService.GetDashboardStatsAsync();

                // Giả định response của bạn có bọc trạng thái Success/Data
                if (response.Success && response.Data != null)
                {
                    dashBoardStateModel = response.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nạp Dashboard: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (dashBoardStateModel != null && _jsModule == null)
            {
                try
                {
                    _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                        "import", "./Components/Pages/Dashboard/Dashboard.razor.js");
                    await RenderCurrentTabChartAsync();
                }
                catch (JSException) { }  
            }
        }

        // Chuyển Tab khi người dùng bấm vào từng thẻ KPI
        private async Task SelectTabAsync(string tabName)
        {
            if (activeTab == tabName) return;
            activeTab = tabName;
            StateHasChanged();
            await Task.Delay(50); // Chờ DOM render canvas mới
            await RenderCurrentTabChartAsync();
        }
        private async Task RenderCurrentTabChartAsync()
        {
            if (_jsModule == null || dashBoardStateModel == null) return;
            try
            {
                switch (activeTab)
                {
                    case "revenue":
                        var labels = dashBoardStateModel.MonthlyRevenues.Select(x => x.MonthLabel).ToArray();
                        var values = dashBoardStateModel.MonthlyRevenues.Select(x => x.Revenue).ToArray();
                        await _jsModule.InvokeVoidAsync("renderRevenueChart", labels, values);
                        break;
                    case "orders":
                        await _jsModule.InvokeVoidAsync("renderOrderStatusChart",
                            dashBoardStateModel.CompletedOrders,
                            dashBoardStateModel.PendingOrders + dashBoardStateModel.ProcessingOrders,
                            dashBoardStateModel.ShippingOrders,
                            dashBoardStateModel.CancelledOrders + dashBoardStateModel.ReturnedOrders);
                        break;
                    case "warehouse":
                        var catLabels = dashBoardStateModel.CategoryRevenues.Select(x => x.CategoryName).ToArray();
                        var catValues = dashBoardStateModel.CategoryRevenues.Select(x => x.TotalRevenue).ToArray();
                        await _jsModule.InvokeVoidAsync("renderCategoryChart", catLabels, catValues);
                        break;
                    case "customers":
                        await _jsModule.InvokeVoidAsync("renderCustomerChart",
                            dashBoardStateModel.CustomersWithOrders,
                            dashBoardStateModel.CustomersWithoutOrders);
                        break;
                }
            }
            catch (JSException) { }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_jsModule is not null)
                {
                    await _jsModule.DisposeAsync();
                }
            }
            catch (JSDisconnectedException) { }
        }
    }
}
