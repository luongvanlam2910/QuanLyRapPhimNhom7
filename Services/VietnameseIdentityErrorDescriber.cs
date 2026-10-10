using Microsoft.AspNetCore.Identity;

namespace DatVeXemPhim.Services;

/// <summary>
/// ASP.NET Core Identity trả lỗi (mật khẩu yếu, trùng email/username, ...) bằng tiếng Anh theo mặc định.
/// Lớp này dịch toàn bộ các thông báo đó sang tiếng Việt để hiển thị nhất quán với phần còn lại của ứng dụng.
/// </summary>
public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = "Đã xảy ra lỗi không xác định."
    };

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = "Dữ liệu đã bị thay đổi bởi một thao tác khác. Vui lòng thử lại."
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = "Mật khẩu không đúng."
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = "Mã xác thực không hợp lệ hoặc đã hết hạn."
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = "Tài khoản đăng nhập này đã được liên kết với một người dùng khác."
    };

    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = $"Tên đăng nhập '{userName}' không hợp lệ — chỉ được chứa chữ, số và một số ký tự đặc biệt."
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = $"Email '{email}' không hợp lệ."
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"Tên đăng nhập '{userName}' đã được sử dụng."
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = $"Email '{email}' đã được đăng ký bởi tài khoản khác."
    };

    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = $"Tên vai trò '{role}' không hợp lệ."
    };

    public override IdentityError DuplicateRoleName(string role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = $"Vai trò '{role}' đã tồn tại."
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = "Người dùng đã có mật khẩu được thiết lập."
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = "Chức năng khóa tài khoản chưa được bật cho người dùng này."
    };

    public override IdentityError UserAlreadyInRole(string role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = $"Người dùng đã thuộc vai trò '{role}'."
    };

    public override IdentityError UserNotInRole(string role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = $"Người dùng không thuộc vai trò '{role}'."
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"Mật khẩu phải có ít nhất {length} ký tự."
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (không phải chữ hoặc số)."
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "Mật khẩu phải chứa ít nhất một chữ số (0-9)."
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "Mật khẩu phải chứa ít nhất một chữ thường (a-z)."
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "Mật khẩu phải chứa ít nhất một chữ hoa (A-Z)."
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = $"Mật khẩu phải chứa ít nhất {uniqueChars} ký tự khác nhau."
    };

    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = "Sử dụng mã khôi phục thất bại."
    };
}
