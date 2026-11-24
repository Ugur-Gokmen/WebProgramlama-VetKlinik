using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class ApiAnahtarServisi
{
    private readonly UygulamaDbContext _db;
    private readonly TimeProvider _saat;

    public ApiAnahtarServisi(UygulamaDbContext db, TimeProvider saat)
    {
        _db = db;
        _saat = saat;
    }

    public static string Hashle(string anahtar)
    {
        var bayt = SHA256.HashData(Encoding.UTF8.GetBytes(anahtar));
        return Convert.ToHexString(bayt);
    }

    public async Task<bool> AnahtarVarMiAsync(string kullaniciId)
        => await _db.ApiAnahtarlari.AnyAsync(a => a.KullaniciId == kullaniciId);

    public async Task<string> YeniAnahtarUretAsync(string kullaniciId)
    {
        var ham = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var eski = await _db.ApiAnahtarlari.Where(a => a.KullaniciId == kullaniciId).ToListAsync();
        _db.ApiAnahtarlari.RemoveRange(eski);
        _db.ApiAnahtarlari.Add(new ApiAnahtari
        {
            KullaniciId = kullaniciId,
            AnahtarHash = Hashle(ham),
            OlusturulmaTarihi = _saat.GetLocalNow().DateTime
        });
        await _db.SaveChangesAsync();
        return ham;
    }
}
