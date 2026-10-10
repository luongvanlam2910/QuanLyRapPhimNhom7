using Microsoft.AspNetCore.Mvc.Razor;

namespace DatVeXemPhim.Infrastructure;

/// <summary>
/// Các controller trong Controllers/Admin không đặt view theo đúng quy ước mặc định
/// ("/Views/{TênController}/{Action}.cshtml"), mà theo 2 kiểu khác nhau:
///   1) Controller đơn chức năng (VD: AdminDashboardController) → thư mục đặt theo tên controller
///      đã bỏ tiền tố "Admin": "/Views/Admin/Dashboard/Index.cshtml".
///   2) AdminQuanLyController gộp nhiều trang (KhachHang, NhanVien, RapChieu, KhuyenMai, ThanhToan)
///      → thư mục đặt theo TÊN ACTION, file luôn là Index.cshtml: "/Views/Admin/KhachHang/Index.cshtml".
/// Thiếu bộ mở rộng này, mọi trang trong khu quản trị sẽ báo lỗi "The view 'X' was not found"
/// (lỗi chỉ lộ ra khi có người thực sự đăng nhập vào /admin, không bị bắt bởi test trang công khai).
/// </summary>
public class AdminViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        const string prefix = "Admin";
        if (context.ControllerName != null && context.ControllerName.StartsWith(prefix, StringComparison.Ordinal))
        {
            var tenRutGon = context.ControllerName[prefix.Length..]; // "AdminDashboard" -> "Dashboard", "AdminQuanLy" -> "QuanLy"

            var themVaoDau = new[]
            {
                // Kiểu 1: controller đơn chức năng, {0} = tên view (Index/Them/Sua...)
                $"/Views/Admin/{tenRutGon}/{{0}}.cshtml",
                // Kiểu 2: AdminQuanLyController — {0} = tên action (KhachHang/NhanVien/...), file luôn Index.cshtml
                "/Views/Admin/{0}/Index.cshtml",
            };

            return themVaoDau.Concat(viewLocations);
        }

        return viewLocations;
    }
}
