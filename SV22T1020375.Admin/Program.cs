using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using SV22T1020375.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "SV22T1020375.Admin";
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(7);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
        option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
//config session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

ApplicationContext.Configure(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// Lấy chuỗi kết nối từ cấu hình (appsettings.json)
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB") ?? "";

if (!string.IsNullOrWhiteSpace(connectionString))
{
    // Khởi tạo các cấu hình chung có sẵn của bạn
    SV22T1020375.BusinessLayers.Configuration.Initialize(connectionString);

    // Khởi tạo CatalogDataService
    SV22T1020375.BusinessLayers.CatalogDataService.Init(connectionString);

    // =====================================================================
    // BỔ SUNG DÒNG NÀY: Khởi tạo AccountDataService để dọn sạch lỗi Null
    // =====================================================================
    SV22T1020375.BusinessLayers.AccountDataService.Init(connectionString);
}

app.Run();