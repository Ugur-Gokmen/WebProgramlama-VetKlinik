using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class HizmetServisi
{
    private readonly UygulamaDbContext _db;

    public HizmetServisi(UygulamaDbContext db)
    {
        _db = db;
    }

    public Task<List<Hizmet>> ListeleAsync()
        => _db.Hizmetler.Include(h => h.Klinik).OrderBy(h => h.Ad).ToListAsync();

    public Task<Hizmet?> GetirAsync(int id)
        => _db.Hizmetler.Include(h => h.Klinik).FirstOrDefaultAsync(h => h.Id == id);

    public async Task EkleAsync(Hizmet hizmet)
    {
        _db.Hizmetler.Add(hizmet);
        await _db.SaveChangesAsync();
    }

    public async Task GuncelleAsync(Hizmet hizmet)
    {
        _db.Hizmetler.Update(hizmet);
        await _db.SaveChangesAsync();
    }

    public async Task<IslemSonucu> SilAsync(int id)
    {
        var hizmet = await GetirAsync(id);
        if (hizmet == null)
            return IslemSonucu.Hata("Hizmet bulunamadı.");
        if (await _db.Randevular.AnyAsync(r => r.HizmetId == id))
            return IslemSonucu.Hata("Randevusu bulunan hizmet silinemez.");

        _db.Hizmetler.Remove(hizmet);
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Hizmet silindi.");
    }
}
