using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class AsiServisi
{
    private readonly UygulamaDbContext _db;
    private readonly TimeProvider _saat;

    public AsiServisi(UygulamaDbContext db, TimeProvider saat)
    {
        _db = db;
        _saat = saat;
    }

    public async Task<AsiKaydi?> GetirAsync(int id, string sahipId)
    {
        return await _db.AsiKayitlari
            .Include(a => a.EvcilHayvan)
            .FirstOrDefaultAsync(a => a.Id == id && a.EvcilHayvan.SahipId == sahipId);
    }

    public async Task EkleAsync(AsiKaydi kayit)
    {
        _db.AsiKayitlari.Add(kayit);
        await _db.SaveChangesAsync();
    }

    public async Task GuncelleAsync(AsiKaydi kayit)
    {
        _db.AsiKayitlari.Update(kayit);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> SilAsync(int id, string sahipId)
    {
        var kayit = await GetirAsync(id, sahipId);
        if (kayit == null)
            return false;
        _db.AsiKayitlari.Remove(kayit);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(List<AsiKaydi> Gecmis, List<AsiKaydi> Yaklasan)?> TakvimAsync(int hayvanId, string sahipId)
    {
        var hayvan = await _db.EvcilHayvanlar
            .Include(h => h.AsiKayitlari)
            .FirstOrDefaultAsync(h => h.Id == hayvanId && h.SahipId == sahipId);
        if (hayvan == null)
            return null;

        var bugun = _saat.GetLocalNow().Date;
        var gecmis = hayvan.AsiKayitlari.OrderByDescending(a => a.YapildigiTarih).ToList();
        var yaklasan = hayvan.AsiKayitlari
            .Where(a => a.SonrakiDozTarihi.HasValue && a.SonrakiDozTarihi.Value.Date >= bugun)
            .OrderBy(a => a.SonrakiDozTarihi)
            .ToList();
        return (gecmis, yaklasan);
    }
}
