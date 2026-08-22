using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using WebApp.Admin.Auth;
using WebApp.Admin.Components;
using WebApp.Admin.Middlewares;
using WebApp.Admin.Services.Implementations;
using WebApp.Admin.Services.Interfaces;
using WebApp.Admin.Utilities;
using WebApp.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

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

// Đăng ký Authentication Cookie cho Blazor Server
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = ".WebBanSach.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.LoginPath = "/management/login";
        options.AccessDeniedPath = "/management/login";
    });

builder.Services.AddAuthorizationCore();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddTransient<CookieHandler>();


var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7188/";

// Đăng ký HttpClient
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));
builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<IUserClientService, UserClientService>();
builder.Services.AddScoped<IDashboardClientService, DashboardClientService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>(lifetime: ServiceLifetime.Singleton);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
// Dù NotFound.razor đã xóa, dòng này vẫn có tác dụng
// vì Blazor Router sẽ bắt /not-found bằng thẻ <NotFound> trong Routes.razor
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware để mỗi lần gửi, nhận request response thì phải đọc cookie
app.UseMiddleware<InitialSessionMiddleware>();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();
app.Run();

