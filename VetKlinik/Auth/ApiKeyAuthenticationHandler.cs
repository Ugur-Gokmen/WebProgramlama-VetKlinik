using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Auth;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SemaAdi = "ApiKey";

    private readonly UygulamaDbContext _db;
    private readonly UserManager<UygulamaKullanicisi> _kullanicilar;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        UygulamaDbContext db,
        UserManager<UygulamaKullanicisi> kullanicilar)
        : base(options, logger, encoder)
    {
        _db = db;
        _kullanicilar = kullanicilar;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerDeger))
            return AuthenticateResult.NoResult();

        var header = headerDeger.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var anahtar = header["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(anahtar))
            return AuthenticateResult.Fail("API anahtarı boş.");

        var hash = ApiAnahtarServisi.Hashle(anahtar);
        var kayit = await _db.ApiAnahtarlari.FirstOrDefaultAsync(a => a.AnahtarHash == hash);
        if (kayit == null)
            return AuthenticateResult.Fail("Geçersiz API anahtarı.");

        var user = await _kullanicilar.FindByIdAsync(kayit.KullaniciId);
        if (user == null)
            return AuthenticateResult.Fail("Kullanıcı bulunamadı.");

        var roller = await _kullanicilar.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? "")
        };
        foreach (var rol in roller)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var kimlik = new ClaimsIdentity(claims, Scheme.Name);
        var bilet = new AuthenticationTicket(new ClaimsPrincipal(kimlik), Scheme.Name);
        return AuthenticateResult.Success(bilet);
    }
}
