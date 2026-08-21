using WebApp.Shared.Dtos.CustomerDtos.Order;

namespace WebApp.Shared.Dtos.Management.Common
{
    // Component dùng: Dashboard.razor (Admin)
    public record DashboardStatsDto(
        int TotalProducts,
        int TotalOrders,
        int TotalCustomers,
        decimal TotalRevenue,
        int PendingOrders,
        int ProcessingOrders,
        int ShippingOrders,
        int CompletedOrders,
        int CancelledOrders,
        decimal RevenueThisMonth,
        decimal RevenueLastMonth,
        int NewCustomersThisMonth,
        List<TopProductDto> TopSellingProducts,
        List<OrderListItemDto> RecentOrders
    );

    public record TopProductDto(
        int ProductId,
        string Title,
        string? MainImageUrl,
        int TotalSold,
        decimal TotalRevenue
    );
}
