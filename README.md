# UTC Cinema – Hệ thống đặt vé xem phim (ASP.NET Core MVC 8)

## 1. Yêu cầu
- .NET SDK 8.0, SQL Server (LocalDB / SQL Server 2008 R2 trở lên), `dotnet-ef` (tùy chọn).

## 2. Cài đặt & cấu hình
1. Sửa chuỗi kết nối `ConnectionStrings:DefaultConnection` trong `appsettings.json`.
2. Tài khoản Admin đầu tiên (không lưu trong repo), dùng user-secrets:
   ```
   dotnet user-secrets set "app:AdminEmail" "admin@utc.vn"
   dotnet user-secrets set "app:AdminPassword" "<mật khẩu mạnh>"
   ```
3. (Tùy chọn) `app:UploadDir`: thư mục lưu ảnh poster; `app:SeatHoldMinutes`: thời gian giữ ghế.

## 3. Tạo cơ sở dữ liệu
- Cách 1 (khuyến nghị): `dotnet run` – `SeedData` tự chạy migration và nạp dữ liệu mẫu.
- Cách 2: `dotnet ef database update`.
- Cách 3: chạy script [CreateDatabase.sql](CreateDatabase.sql) bằng SSMS/sqlcmd (idempotent).
- Dữ liệu mẫu để demo (phim, rạp, suất chiếu, đơn đặt vé...): chạy [TestData.sql](TestData.sql) sau khi đã có bảng: `sqlcmd -S <server> -U <user> -P <pwd> -d <db> -f 65001 -I -i TestData.sql`.

## 4. Chạy
```
dotnet run
```
Mở địa chỉ hiển thị trong terminal. Khu quản trị: `/Admin/Dashboard`.

## 5. Đối chiếu yêu cầu đề tài
| Yêu cầu | Thực hiện |
|---|---|
| Nhiều lớp người dùng | Khách vãng lai, Khách hàng, NhanVien, QuanLy, Admin (Identity roles + policy `AdminOnly`/`QuanLyOnly`/`NhanVienHoacQuanLy`); khu `/Admin/*` riêng với `_AdminLayout` |
| Template giao diện | `_Layout.cshtml`, `_AdminLayout.cshtml`, `wwwroot/css/style.css` (giao diện tùy biến) |
| Bootstrap/jQuery/AJAX | Bootstrap 5 (responsive), jQuery (`$.getJSON` tải lịch chiếu), phân trang + lọc tại `/tim-kiem` |
| CRUD | Phim, Thể loại, Rạp, Phòng, Lịch chiếu, Khuyến mãi, Nhân viên… (`Controllers/Admin`) |
| Session, Cookies | Session lưu "phim đã xem gần đây"; cookie đăng nhập (HttpOnly, SameSite), cookie ngôn ngữ |
| Annotation | Data Annotations trên Entity/ViewModel; `[Authorize]`, `[ValidateAntiForgeryToken]`, `[ApiController]`, `[Route]` |
| RESTful API | `api/phim`, `api/rap-chieu`, `api/lich-chieu` (GET); `api/the-loai` đầy đủ GET/POST/PUT/DELETE (201/204/404/409) |
| Bảo mật | ASP.NET Identity (băm mật khẩu, khóa tài khoản sau 5 lần sai), phân quyền theo vai trò, anti-forgery, cookie HttpOnly/SameSite, security headers, EF Core tham số hóa (chống SQL injection), Razor tự mã hóa HTML (chống XSS), HTTPS + HSTS |
| Quốc tế hóa | `SharedResource.resx` (vi) / `.en.resx` (en), `CultureController` lưu cookie ngôn ngữ |

## 6. Ví dụ gọi REST API
```
GET    /api/the-loai
POST   /api/the-loai        {"tenTheLoai":"Kinh dị"}   (cần đăng nhập QuanLy/Admin)
PUT    /api/the-loai/3      {"tenTheLoai":"Hành động"}
DELETE /api/the-loai/3
```

## 7. Phân quyền khu quản trị (nguyên tắc: mỗi vai trò chỉ thấy việc của mình)
| Chức năng | Nhân viên (NhanVien) | Quản lý (QuanLy) | Admin |
|---|---|---|---|
| Công việc của tôi (số đơn/vé đã bán, đã soát — không có tiền) | ✔ | thay bằng Dashboard | Dashboard |
| Bán vé tại quầy, Soát vé | ✔ | ✔ | ✔ |
| Xem đơn đặt vé | chỉ đơn **do mình bán**, chỉ xem | đơn của rạp mình, được hủy/xác nhận thanh toán | tất cả |
| Khách hàng, Thanh toán, Báo cáo/Doanh thu, Khuyến mãi | ✘ | ✔ (trong rạp mình) | ✔ |
| Phim, Thể loại, Lịch chiếu, Rạp & Phòng, Nhân viên | ✘ | ✔ (rạp mình) | ✔ |
| Phòng ban, tạo/xóa rạp | ✘ | ✘ | ✔ |

Tài khoản demo (sau khi chạy `TestData.sql`): `nv1`–`nv3@utccinema.vn` = Quản lý, `nv4`–`nv6@utccinema.vn` = Nhân viên. Mật khẩu của các tài khoản này giống tài khoản Admin (giá trị `app:AdminPassword` bạn cấu hình trong user-secrets); hãy đổi sau khi triển khai thật.
