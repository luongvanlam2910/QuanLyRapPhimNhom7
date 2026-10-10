using Microsoft.AspNetCore.Identity;

namespace DatVeXemPhim.Models;

public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; } = string.Empty;
}
