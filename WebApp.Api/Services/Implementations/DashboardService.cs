using Microsoft.EntityFrameworkCore;
using WebApp.Api.Data;
using WebApp.Api.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.CustomerDtos.Order;
using WebApp.Shared.Dtos.Management.Common;
using WebApp.Shared.Enums;

namespace WebApp.Api.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
                var sixMonthsAgo = firstDayThisMonth.AddMonths(-5);

                // 1. TÀI CHÍNH & DOANH THU
                var completedOrdersQuery = _context.Orders.Where(o => o.Status == (int)OrderStatus.Completed);
                var totalRevenue = await completedOrdersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
                var revenueThisMonth = await completedOrdersQuery
                    .Where(o => o.OrderDate >= firstDayThisMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
                var revenueLastMonth = await completedOrdersQuery
                    .Where(o => o.OrderDate >= firstDayLastMonth && o.OrderDate < firstDayThisMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

                // Tính % Tăng trưởng doanh thu so với tháng trước
                decimal revenueGrowthPercent = 0m;
                if (revenueLastMonth > 0)
                {
                    revenueGrowthPercent = Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100, 1);
                }
                else if (revenueThisMonth > 0)
                {
                    revenueGrowthPercent = 100m;
                }
                var completedOrdersCount = await completedOrdersQuery.CountAsync();
                decimal averageOrderValue = completedOrdersCount > 0 ? Math.Round(totalRevenue / completedOrdersCount, 0) : 0m;

                // 2. VẬN HÀNH & ĐƠN HÀNG
                var totalOrders = await _context.Orders.CountAsync();
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == (int)OrderStatus.Pending);
                var processingOrders = await _context.Orders.CountAsync(o => o.Status == (int)OrderStatus.Processing);
                var shippingOrders = await _context.Orders.CountAsync(o => o.Status == (int)OrderStatus.Shipping);
                var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == (int)OrderStatus.Cancelled);
                var returnedOrders = await _context.Orders.CountAsync(o => o.Status == (int)OrderStatus.Returned);
                double orderSuccessRate = totalOrders > 0
                    ? Math.Round(((double)completedOrdersCount / totalOrders) * 100, 1)
                    : 0.0;

                // 3. KHO BÃI & SẢN PHẨM
                var totalProducts = await _context.Products.CountAsync();
                var totalStockQuantity = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;
                var totalInventoryValue = await _context.Products.SumAsync(p => (decimal?)(p.Price * p.StockQuantity)) ?? 0m;
                var healthyStockCount = await _context.Products.CountAsync(p => p.StockQuantity > 5);
                var lowStockCount = await _context.Products.CountAsync(p => p.StockQuantity >= 1 && p.StockQuantity <= 5);
                var outOfStockCount = await _context.Products.CountAsync(p => p.StockQuantity == 0);

                // 4. KHÁCH HÀNG & PHÂN KHÚC
                var totalCustomers = await _context.Users.CountAsync();
                var newCustomersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= firstDayThisMonth);

                // Thống kê khách đã mua hàng vs chưa mua hàng
                var customerOrderCounts = await _context.Orders
                    .GroupBy(o => o.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync();
                int customersWithOrders = customerOrderCounts.Count;
                int customersWithoutOrders = Math.Max(0, totalCustomers - customersWithOrders);
                int repeatCustomers = customerOrderCounts.Count(c => c.Count >= 2);
                double repeatCustomerRate = customersWithOrders > 0
                    ? Math.Round(((double)repeatCustomers / customersWithOrders) * 100, 1)
                    : 0.0;

                // 5. BIỂU ĐỒ 1: DOANH THU 6 THÁNG GẦN NHẤT
                var ordersLast6Months = await _context.Orders
                    .Where(o => o.Status == (int)OrderStatus.Completed && o.OrderDate >= sixMonthsAgo)
                    .Select(o => new { o.OrderDate, o.TotalAmount })
                    .ToListAsync();
                var monthlyRevenues = new List<MonthlyRevenueDto>();
                for (int i = 5; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var label = $"T{targetMonth.Month}/{targetMonth.Year}";
                    var monthOrders = ordersLast6Months
                        .Where(o => o.OrderDate.Month == targetMonth.Month && o.OrderDate.Year == targetMonth.Year)
                        .ToList();
                    monthlyRevenues.Add(new MonthlyRevenueDto(
                        label,
                        monthOrders.Sum(x => x.TotalAmount),
                        monthOrders.Count
                    ));
                }

                // 6. BIỂU ĐỒ 2: DOANH THU THEO THỂ LOẠI SÁCH
                var categoryRaw = await _context.OrderItems
                    .Where(oi => oi.Order.Status == (int)OrderStatus.Completed)
                    .GroupBy(oi => oi.Product.Category.CategoryName)
                    .Select(g => new
                    {
                        CategoryName = g.Key ?? "Chưa phân loại",
                        TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity),
                        TotalSold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(5)
                    .ToListAsync();
                var categoryRevenues = categoryRaw
                    .Select(x => new CategoryRevenueDto(x.CategoryName, x.TotalRevenue, x.TotalSold))
                    .ToList();

                // 7. THỐNG KÊ MÔ HÌNH SÁCH
                int freeBooksCount = await _context.FreeBooks.CountAsync();
                int donationBooksCount = await _context.BookDonations.CountAsync();
                var bookDistribution = new BookModelDistributionDto(
                    CommercialBookCount: totalProducts,
                    FreeBookCount: freeBooksCount,
                    DonationBookCount: donationBooksCount
                );

                var statsDto = new DashboardStatsDto(
                    TotalRevenue: totalRevenue,
                    RevenueThisMonth: revenueThisMonth,
                    RevenueGrowthPercent: revenueGrowthPercent,
                    AverageOrderValue: averageOrderValue,
                    TotalOrders: totalOrders,
                    OrderSuccessRate: orderSuccessRate,
                    PendingOrders: pendingOrders,
                    ProcessingOrders: processingOrders,
                    ShippingOrders: shippingOrders,
                    CompletedOrders: completedOrdersCount,
                    CancelledOrders: cancelledOrders,
                    ReturnedOrders: returnedOrders,
                    TotalProducts: totalProducts,
                    TotalStockQuantity: totalStockQuantity,
                    TotalInventoryValue: totalInventoryValue,
                    HealthyStockCount: healthyStockCount,
                    LowStockCount: lowStockCount,
                    OutOfStockCount: outOfStockCount,
                    TotalCustomers: totalCustomers,
                    NewCustomersThisMonth: newCustomersThisMonth,
                    RepeatCustomerRate: repeatCustomerRate,
                    CustomersWithOrders: customersWithOrders,
                    CustomersWithoutOrders: customersWithoutOrders,
                    MonthlyRevenues: monthlyRevenues,
                    CategoryRevenues: categoryRevenues,
                    BookDistribution: bookDistribution
                );

                return ApiResponse<DashboardStatsDto>.SuccessResult(statsDto, "Lấy dữ liệu phân tích BI thành công.");
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardStatsDto>.FailureResult("Lỗi khi tổng hợp báo cáo.", new List<string> { ex.Message });
            }
        }
    }
}
