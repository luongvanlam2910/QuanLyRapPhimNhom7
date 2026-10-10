using DatVeXemPhim.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Infrastructure;

/// <summary>
/// Chuyển ngoại lệ thành thông báo tiếng Việt dễ hiểu để hiện cho người dùng, thay vì
/// "An error occurred while saving the entity changes. See the inner exception for details."
/// </summary>
public static class ErrorMessages
{
    public static string From(Exception ex)
    {
        if (ex is BusinessException or ResourceNotFoundException or GheDaDatException)
        {
            return ex.Message;
        }

        var sql = FindSqlException(ex);
        if (sql != null)
        {
            switch (sql.Number)
            {
                case 2601:
                case 2627:
                    return "Dữ liệu bị trùng: đã có bản ghi khác dùng cùng giá trị (tên, số điện thoại, email...).";
                case 547:
                    return "Không thể thực hiện vì dữ liệu này đang được sử dụng ở nơi khác (còn dữ liệu liên quan).";
                case 8152:
                case 2628:
                    return "Dữ liệu nhập quá dài so với quy định.";
                case 8115:
                case 220:
                    return "Giá trị số quá lớn hoặc không hợp lệ.";
            }
        }

        if (ex is DbUpdateException)
        {
            return "Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin đã nhập.";
        }

        return ex.Message;
    }

    private static SqlException? FindSqlException(Exception? ex)
    {
        while (ex != null)
        {
            if (ex is SqlException s) return s;
            ex = ex.InnerException;
        }
        return null;
    }
}
