using DatVeXemPhim.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Data;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        // ---- Roles ----
        foreach (var role in AppRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Không seed phòng ban/rạp/phim/khách hàng mẫu nào — toàn bộ dữ liệu nghiệp vụ
        // phải là dữ liệu thật do Admin tự nhập qua giao diện quản trị.

        // ---- Tài khoản Admin đầu tiên ----
        // Không seed dữ liệu nghiệp vụ ảo (rạp/phim/phòng) — dữ liệu thật sẽ được
        // nhập qua trang Admin. Chỉ tạo sẵn 1 tài khoản quản trị để có thể đăng nhập lần đầu.
        // Cấu hình qua appsettings/user-secrets: app:AdminEmail, app:AdminPassword.
        var adminEmail = configuration["app:AdminEmail"];
        var adminPassword = configuration["app:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    HoTen = "Quản trị hệ thống"
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
            {
                // Nâng cấp tài khoản admin seed từ lần chạy trước (role "QuanLy" cũ) lên "Admin".
                if (await userManager.IsInRoleAsync(adminUser, AppRoles.QuanLy))
                {
                    await userManager.RemoveFromRoleAsync(adminUser, AppRoles.QuanLy);
                }
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
        }
    }
}
