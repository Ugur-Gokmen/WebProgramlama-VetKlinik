using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class HayvanServisi
{
    private readonly UygulamaDbContext _db;

    public HayvanServisi(UygulamaDbContext db)
    {
        _db = db;
    }

    public async Task<List<EvcilHayvan>> ListeleAsync(string sahipId)
    {
        return await _db.EvcilHayvanlar
            .Where(h => h.SahipId == sahipId)
            .OrderBy(h => h.Ad)
            .ToListAsync();
    }

    public async Task<EvcilHayvan?> GetirAsync(int id, string sahipId)
    {
        return await _db.EvcilHayvanlar
            .Include(h => h.AsiKayitlari)
            .FirstOrDefaultAsync(h => h.Id == id && h.SahipId == sahipId);
    }

    public async Task<List<EvcilHayvan>> AdaGoreGetirAsync(string sahipId, string ad)
    {
        return await _db.EvcilHayvanlar
            .Include(h => h.AsiKayitlari)
            .Where(h => h.SahipId == sahipId && h.Ad.ToLower() == ad.ToLower())
            .ToListAsync();
    }

    public async Task EkleAsync(EvcilHayvan hayvan)
    {
        _db.EvcilHayvanlar.Add(hayvan);
        await _db.SaveChangesAsync();
    }

    public async Task GuncelleAsync(EvcilHayvan hayvan)
    {
        _db.EvcilHayvanlar.Update(hayvan);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> SilAsync(int id, string sahipId)
    {
        var hayvan = await GetirAsync(id, sahipId);
        if (hayvan == null)
            return false;
        _db.EvcilHayvanlar.Remove(hayvan);
        await _db.SaveChangesAsync();
        return true;
    }
}
