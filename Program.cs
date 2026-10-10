using System.Globalization;
using DatVeXemPhim.Data;
using DatVeXemPhim.Models;
using DatVeXemPhim.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    // API trả JSON cho các entity có quan hệ 2 chiều (Phim <-> TheLoai): tránh lỗi vòng lặp
    // khi serialize (System.Text.Json mặc định ném lỗi trên cycle).
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddLocalization();

// Các controller trong Controllers/Admin không đặt view theo quy ước mặc định
// (xem chi tiết trong AdminViewLocationExpander) — thiếu dòng này mọi trang /admin sẽ báo lỗi "view not found".
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new DatVeXemPhim.Infrastructure.AdminViewLocationExpander());
});

// Quốc tế hóa: tiếng Việt là ngôn ngữ mặc định, hỗ trợ thêm tiếng Anh.
// Người dùng chọn ngôn ngữ qua CultureController, lưu vào cookie ".AspNetCore.Culture".
var supportedCultures = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // Server thực tế là SQL Server 2008 R2 (compat level 100): ép EF Core sinh
        // SQL tương thích (không dùng OFFSET/FETCH, OPENJSON, ... của SQL 2012+).
        sql => sql.UseCompatibilityLevel(100)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Khóa tài khoản sau 5 lần sai mật khẩu (chặn brute-force)
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    // Dịch toàn bộ thông báo lỗi mặc định của Identity (mật khẩu yếu, trùng email...) sang tiếng Việt.
    .AddErrorDescriber<VietnameseIdentityErrorDescriber>();

// Cookie login paths (replaces SecurityConfig.java formLogin/logout config)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.LogoutPath = "/Auth/Logout";
    // Bảo mật cookie đăng nhập: chặn JS đọc (chống XSS đánh cắp) và hạn chế gửi chéo site (chống CSRF)
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    // REST API: trả 401/403 đúng chuẩn thay vì chuyển hướng sang trang đăng nhập
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        else ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// Kiểm tra lại SecurityStamp mỗi phút: khi admin khóa/đổi quyền một nhân viên,
// phiên đăng nhập cũ của họ bị vô hiệu trong tối đa 1 phút thay vì còn hiệu lực
// cho tới khi cookie hết hạn.
builder.Services.Configure<Microsoft.AspNetCore.Identity.SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

// Named authorization policies (replaces SecurityConfig.java authorizeHttpRequests rules)
// Admin (quản trị toàn hệ thống) luôn được gộp vào mọi policy dành cho QuanLy/NhanVien,
// nhưng phạm vi dữ liệu Admin thấy được không bị giới hạn theo rạp (xem ICurrentStaffService).
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("QuanLyOnly", p => p.RequireRole("Admin", "QuanLy"));
    options.AddPolicy("NhanVienHoacQuanLy", p => p.RequireRole("Admin", "QuanLy", "NhanVien"));
});

builder.Services.AddMemoryCache();

// Session: lưu danh sách "phim đã xem gần đây" của từng khách (xem PhimController.Detail / HomeController.Index).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".UTCCinema.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IRapChieuService, RapChieuService>();
builder.Services.AddScoped<IGheService, GheService>();
builder.Services.AddSingleton<ISeatHoldService, SeatHoldService>();
builder.Services.AddScoped<IPhimService, PhimService>();
builder.Services.AddScoped<ILichChieuService, LichChieuService>();
builder.Services.AddScoped<IDatVeService, DatVeService>();
builder.Services.AddScoped<IKhuyenMaiService, KhuyenMaiService>();
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
builder.Services.AddScoped<IPhongBanService, PhongBanService>();
builder.Services.AddScoped<ICurrentStaffService, CurrentStaffService>();
builder.Services.AddScoped<IKiemTraDoTuoiService, KiemTraDoTuoiService>();
builder.Services.AddScoped<ISoatVeService, SoatVeService>();
builder.Services.AddHostedService<HuyDonQuaHanService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers: chống clickjacking, MIME sniffing, lộ referrer
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});
app.UseStaticFiles();

app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

// Serve poster images from an external uploads folder (replaces WebConfig.java addResourceHandlers),
// e.g. a request to /images/posters/xxx.jpg resolves to {UploadDir}/images/posters/xxx.jpg on disk.
var uploadDirConfig = app.Configuration["app:UploadDir"];
var uploadDir = string.IsNullOrWhiteSpace(uploadDirConfig)
    ? Path.Combine(app.Environment.ContentRootPath, "uploads", "images")
    : Path.GetFullPath(uploadDirConfig, app.Environment.ContentRootPath);

if (Directory.Exists(uploadDir))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadDir),
        RequestPath = "/images"
    });
}

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await SeedData.EnsureSeedDataAsync(scope.ServiceProvider);
}

app.Run();
