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

    public async Task SilAsync(Hizmet hizmet)
    {
        _db.Hizmetler.Remove(hizmet);
        await _db.SaveChangesAsync();
    }
}
