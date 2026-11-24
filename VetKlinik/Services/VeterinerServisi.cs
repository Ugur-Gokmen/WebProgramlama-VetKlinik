using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

public class VeterinerServisi
{
    private readonly UygulamaDbContext _db;
    private readonly UserManager<UygulamaKullanicisi> _kullanicilar;

    public VeterinerServisi(UygulamaDbContext db, UserManager<UygulamaKullanicisi> kullanicilar)
    {
        _db = db;
        _kullanicilar = kullanicilar;
    }

    public Task<List<Veteriner>> ListeleAsync()
        => _db.Veterinerler
            .Include(v => v.Klinik)
            .Include(v => v.Uzmanliklar).ThenInclude(u => u.Uzmanlik)
            .Include(v => v.Hizmetler).ThenInclude(h => h.Hizmet)
            .Include(v => v.Musaitlikler)
            .OrderBy(v => v.AdSoyad)
            .ToListAsync();

    public Task<Veteriner?> GetirAsync(int id)
        => _db.Veterinerler
            .Include(v => v.Klinik)
            .Include(v => v.Uzmanliklar).ThenInclude(u => u.Uzmanlik)
            .Include(v => v.Hizmetler).ThenInclude(h => h.Hizmet)
            .Include(v => v.Musaitlikler)
            .Include(v => v.Kullanici)
            .FirstOrDefaultAsync(v => v.Id == id);

    public Task<Veteriner?> KullaniciyaGoreGetirAsync(string kullaniciId)
        => _db.Veterinerler.FirstOrDefaultAsync(v => v.KullaniciId == kullaniciId);

    public async Task<IslemSonucu> OlusturAsync(string adSoyad, string eposta, string sifre, int klinikId, IEnumerable<int> hizmetIdler, IEnumerable<int> uzmanlikIdler, IEnumerable<VeterinerMusaitlik> musaitlikler)
    {
        var user = new UygulamaKullanicisi
        {
            UserName = eposta,
            Email = eposta,
            EmailConfirmed = true,
            AdSoyad = adSoyad
        };
        var sonuc = await _kullanicilar.CreateAsync(user, sifre);
        if (!sonuc.Succeeded)
            return IslemSonucu.Hata(string.Join(" ", sonuc.Errors.Select(e => e.Description)));

        await _kullanicilar.AddToRoleAsync(user, "Veteriner");

        var vet = new Veteriner
        {
            AdSoyad = adSoyad,
            KlinikId = klinikId,
            KullaniciId = user.Id
        };
        foreach (var hid in hizmetIdler.Distinct())
            vet.Hizmetler.Add(new VeterinerHizmet { HizmetId = hid });
        foreach (var uid in uzmanlikIdler.Distinct())
            vet.Uzmanliklar.Add(new VeterinerUzmanlik { UzmanlikId = uid });
        foreach (var m in musaitlikler)
            vet.Musaitlikler.Add(m);

        _db.Veterinerler.Add(vet);
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Veteriner eklendi.", vet.Id);
    }

    public async Task GuncelleIliskilerAsync(Veteriner vet, IEnumerable<int> hizmetIdler, IEnumerable<int> uzmanlikIdler, IList<VeterinerMusaitlik> musaitlikler)
    {
        var eskiHizmet = await _db.VeterinerHizmetleri.Where(x => x.VeterinerId == vet.Id).ToListAsync();
        _db.VeterinerHizmetleri.RemoveRange(eskiHizmet);
        foreach (var hid in hizmetIdler.Distinct())
            _db.VeterinerHizmetleri.Add(new VeterinerHizmet { VeterinerId = vet.Id, HizmetId = hid });

        var eskiUzman = await _db.VeterinerUzmanliklari.Where(x => x.VeterinerId == vet.Id).ToListAsync();
        _db.VeterinerUzmanliklari.RemoveRange(eskiUzman);
        foreach (var uid in uzmanlikIdler.Distinct())
            _db.VeterinerUzmanliklari.Add(new VeterinerUzmanlik { VeterinerId = vet.Id, UzmanlikId = uid });

        var eskiMusait = await _db.VeterinerMusaitlikleri.Where(x => x.VeterinerId == vet.Id).ToListAsync();
        _db.VeterinerMusaitlikleri.RemoveRange(eskiMusait);
        foreach (var m in musaitlikler)
        {
            m.Id = 0;
            m.VeterinerId = vet.Id;
            _db.VeterinerMusaitlikleri.Add(m);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<IslemSonucu> SilAsync(int id)
    {
        var vet = await GetirAsync(id);
        if (vet == null)
            return IslemSonucu.Hata("Veteriner bulunamadı.");
        if (await _db.Randevular.AnyAsync(r => r.VeterinerId == id))
            return IslemSonucu.Hata("Randevusu olan veteriner silinemez.");

        _db.Veterinerler.Remove(vet);
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Veteriner silindi.");
    }

    public Task<List<Uzmanlik>> UzmanliklariGetirAsync()
        => _db.Uzmanliklar.OrderBy(u => u.Ad).ToListAsync();
}
