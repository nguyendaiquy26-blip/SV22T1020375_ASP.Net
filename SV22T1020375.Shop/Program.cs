using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020375.BusinessLayers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "AuthenticationCookie";
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==============================================================================
// 1. ĐỌC CHUỖI KẾT NỐI TỪ APPSETTINGS.JSON
// ==============================================================================
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB") ?? "";

// ==============================================================================
// 2. KHỞI TẠO TẦNG BUSINESS LAYER (Truyền chuỗi kết nối vào)
// ==============================================================================

// 👉 ĐÚNG YÊU CẦU DỰ ÁN: Dùng chung cách khởi tạo với Admin!
// Truyền chuỗi kết nối vào class Configuration của tầng BusinessLayers.
// Lúc này PartnerDataService bên dưới (và các file gốc khác) sẽ tự động hoạt động mượt mà.
Configuration.Initialize(connectionString);

// Khởi tạo các Service khác
CatalogDataService.Init(connectionString);
UserAccountService.Initialize(connectionString);




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();