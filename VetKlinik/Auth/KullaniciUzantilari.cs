using System.Security.Claims;

namespace VetKlinik.Auth;

public static class KullaniciUzantilari
{
    public static string IdAl(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
}
