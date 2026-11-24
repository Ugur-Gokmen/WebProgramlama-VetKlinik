using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Dtos;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class RaporServisi
{
    private readonly UygulamaDbContext _db;

    public RaporServisi(UygulamaDbContext db)
    {
        _db = db;
    }

    public async Task<List<HizmetOzetDto>> HizmetOzetiAsync()
    {
        return await _db.Randevular
            .Where(r => r.Durum != RandevuDurumu.Reddedildi && r.Durum != RandevuDurumu.Iptal)
            .GroupBy(r => new { r.HizmetId, r.Hizmet.Ad })
            .Select(g => new HizmetOzetDto
            {
                HizmetId = g.Key.HizmetId,
                HizmetAdi = g.Key.Ad,
                RandevuSayisi = g.Count(),
                Ciro = g.Sum(x => x.Ucret)
            })
            .OrderByDescending(x => x.Ciro)
            .ToListAsync();
    }
}
