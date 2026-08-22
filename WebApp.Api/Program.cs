using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebApp.Api.Data;
using WebApp.Api.Entities;
using WebApp.Api.Hubs;
using WebApp.Api.Services.Implementations;
using WebApp.Api.Services.Interfaces;
using WebApp.Api.Utilities;
using WebApp.Shared;

var builder = WebApplication.CreateBuilder(args);

// Tránh việc gọi Attribute [Authorize] lên các Controller
// Lưu ý: cần [AllowAnonymous] cho các endpoint mà không cần đăng nhập
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Cors 
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ??
    new[] { "https://localhost:7135", "https://localhost:7035" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApps", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Đăng ký và sử dụng Cookie
builder.Services.AddIdentity<User, Role>(options =>
{
    // Cấu hình mật khẩu
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Tránh bot Brute-Force mật khẩu
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình tài khoản
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


// TODO: CẦN CHUYỂN SANG X509Certificate ĐỂ MÃ HÓA COOKIE PHÙ HỢP TẤT CẢ HỆ ĐIỀU HÀNH
var keysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? (OperatingSystem.IsWindows() 
        ? @"C:\SharedKeys\WebBanSach"
        : "/app/SharedKeys/WebBanSach");

var keysDir = new DirectoryInfo(keysPath);
if (!keysDir.Exists)
{
    keysDir.Create();
}

var dataProtection = builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(keysDir)
        .SetApplicationName("SharedCookieWebBanSach");
        
if (OperatingSystem.IsWindows())
{
    dataProtection.ProtectKeysWithDpapi();
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".WebBanSach.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true; 

    // Trả về mã 403/401 thay vì Redirect
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddControllers();

// Thêm Context vào các request để Service có thể đọc thông tin User
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Auth Services
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Thêm Hub để cập nhập các update từ Admin
builder.Services.AddSignalR();

// Load các Validation được thiết lập ở WebApp.Shared
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorApps");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<UpdateBroadcastHub>("/hubs/updates");
app.Run();
