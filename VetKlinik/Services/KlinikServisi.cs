using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class KlinikServisi
{
    private readonly UygulamaDbContext _db;

    public KlinikServisi(UygulamaDbContext db)
    {
        _db = db;
    }

    public Task<List<Klinik>> ListeleAsync()
        => _db.Klinikler.Include(k => k.CalismaSaatleri).OrderBy(k => k.Ad).ToListAsync();

    public Task<Klinik?> GetirAsync(int id)
        => _db.Klinikler.Include(k => k.CalismaSaatleri).FirstOrDefaultAsync(k => k.Id == id);

    public async Task EkleAsync(Klinik klinik)
    {
        SaatleriTamamla(klinik);
        _db.Klinikler.Add(klinik);
        await _db.SaveChangesAsync();
    }

    public async Task GuncelleAsync(Klinik klinik)
    {
        _db.Klinikler.Update(klinik);
        await _db.SaveChangesAsync();
    }

    public async Task SilAsync(Klinik klinik)
    {
        _db.Klinikler.Remove(klinik);
        await _db.SaveChangesAsync();
    }

    private static void SaatleriTamamla(Klinik klinik)
    {
        if (klinik.CalismaSaatleri.Count > 0)
            return;
        foreach (DayOfWeek gun in Enum.GetValues<DayOfWeek>())
        {
            klinik.CalismaSaatleri.Add(new KlinikCalismaSaati
            {
                Gun = gun,
                Acik = gun != DayOfWeek.Sunday,
                Acilis = new TimeSpan(9, 0, 0),
                Kapanis = new TimeSpan(18, 0, 0)
            });
        }
    }
}
