using Microsoft.EntityFrameworkCore;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Services;

// Randevu iş kurallarının tek adresi. MVC, API ve MCP burayı çağırır.
public class RandevuServisi
{
    public const int SlotAdimiDakika = 30;

    private readonly UygulamaDbContext _db;
    private readonly TimeProvider _saat;

    public RandevuServisi(UygulamaDbContext db, TimeProvider saat)
    {
        _db = db;
        _saat = saat;
    }

    // İki aralık çakışır mı? Tam bitişte başlayan randevu çakışmaz.
    public static bool CakisiyorMu(DateTime yeniBas, DateTime yeniBit, DateTime mevcutBas, DateTime mevcutBit)
        => yeniBas < mevcutBit && yeniBit > mevcutBas;

    public static bool BloklayanDurum(RandevuDurumu durum)
        => durum == RandevuDurumu.Bekliyor || durum == RandevuDurumu.Onaylandi;

    public async Task<List<Randevu>> UyeRandevulariAsync(string sahipId)
    {
        return await _db.Randevular
            .Include(r => r.EvcilHayvan)
            .Include(r => r.Hizmet)
            .Include(r => r.Veteriner)
            .Where(r => r.EvcilHayvan.SahipId == sahipId)
            .OrderByDescending(r => r.Baslangic)
            .ToListAsync();
    }

    public async Task<Randevu?> UyeRandevuGetirAsync(int id, string sahipId)
    {
        return await _db.Randevular
            .Include(r => r.EvcilHayvan)
            .Include(r => r.Hizmet)
            .Include(r => r.Veteriner)
            .Include(r => r.MuayeneNotu)
            .FirstOrDefaultAsync(r => r.Id == id && r.EvcilHayvan.SahipId == sahipId);
    }

    public async Task<List<Randevu>> VeterinerRandevulariAsync(string kullaniciId, bool adminMi)
    {
        var sorgu = _db.Randevular
            .Include(r => r.EvcilHayvan).ThenInclude(h => h.Sahip)
            .Include(r => r.Hizmet)
            .Include(r => r.Veteriner)
            .Include(r => r.MuayeneNotu)
            .AsQueryable();

        if (!adminMi)
            sorgu = sorgu.Where(r => r.Veteriner.KullaniciId == kullaniciId);

        return await sorgu.OrderBy(r => r.Baslangic).ToListAsync();
    }

    public async Task<Randevu?> VeterinerRandevuGetirAsync(int id, string kullaniciId, bool adminMi)
    {
        var randevu = await _db.Randevular
            .Include(r => r.EvcilHayvan).ThenInclude(h => h.Sahip)
            .Include(r => r.Hizmet)
            .Include(r => r.Veteriner)
            .Include(r => r.MuayeneNotu)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (randevu == null)
            return null;
        if (!adminMi && randevu.Veteriner.KullaniciId != kullaniciId)
            return null;
        return randevu;
    }

    public async Task<List<MusaitSlot>> UygunSlotlariGetirAsync(int hizmetId, DateTime gun, int? veterinerId = null)
    {
        var hizmet = await _db.Hizmetler.FindAsync(hizmetId);
        if (hizmet == null)
            return new List<MusaitSlot>();

        var hekimler = await _db.Veterinerler
            .Include(v => v.Musaitlikler)
            .Include(v => v.Hizmetler)
            .Where(v => v.Hizmetler.Any(h => h.HizmetId == hizmetId))
            .Where(v => !veterinerId.HasValue || v.Id == veterinerId.Value)
            .ToListAsync();

        var klinikSaat = await _db.KlinikCalismaSaatleri
            .FirstOrDefaultAsync(s => s.KlinikId == hizmet.KlinikId && s.Gun == gun.DayOfWeek);

        if (klinikSaat == null || !klinikSaat.Acik)
            return new List<MusaitSlot>();

        var gunBas = gun.Date;
        var gunBit = gunBas.AddDays(1);
        var mevcutlar = await _db.Randevular
            .Where(r => BloklayanDurum(r.Durum) && r.Baslangic < gunBit && r.Bitis > gunBas)
            .ToListAsync();

        var simdi = _saat.GetLocalNow().DateTime;
        var liste = new List<MusaitSlot>();

        foreach (var hekim in hekimler)
        {
            var musait = hekim.Musaitlikler.FirstOrDefault(m => m.Gun == gun.DayOfWeek);
            if (musait == null || !musait.Musait)
                continue;

            var pencereBas = MaxTime(klinikSaat.Acilis, musait.Baslangic);
            var pencereBit = MinTime(klinikSaat.Kapanis, musait.Bitis);
            if (pencereBit - pencereBas < TimeSpan.FromMinutes(hizmet.SureDakika))
                continue;

            for (var t = gunBas.Add(pencereBas);
                 t.AddMinutes(hizmet.SureDakika) <= gunBas.Add(pencereBit);
                 t = t.AddMinutes(SlotAdimiDakika))
            {
                var bitis = t.AddMinutes(hizmet.SureDakika);
                if (t < simdi)
                    continue;

                var hekimCakisma = mevcutlar.Any(r =>
                    r.VeterinerId == hekim.Id && CakisiyorMu(t, bitis, r.Baslangic, r.Bitis));
                if (hekimCakisma)
                    continue;

                liste.Add(new MusaitSlot
                {
                    VeterinerId = hekim.Id,
                    VeterinerAdi = hekim.AdSoyad,
                    Baslangic = t,
                    Bitis = bitis
                });
            }
        }

        return liste.OrderBy(s => s.Baslangic).ThenBy(s => s.VeterinerAdi).ToList();
    }

    public async Task<List<Veteriner>> UygunHekimleriGetirAsync(int hizmetId, DateTime gun)
    {
        var slotlar = await UygunSlotlariGetirAsync(hizmetId, gun);
        var idler = slotlar.Select(s => s.VeterinerId).Distinct().ToList();
        return await _db.Veterinerler
            .Include(v => v.Uzmanliklar).ThenInclude(u => u.Uzmanlik)
            .Where(v => idler.Contains(v.Id))
            .ToListAsync();
    }

    public async Task<IslemSonucu> OlusturAsync(string sahipId, int hayvanId, int hizmetId, int veterinerId, DateTime baslangic)
    {
        var hayvan = await _db.EvcilHayvanlar.FirstOrDefaultAsync(h => h.Id == hayvanId && h.SahipId == sahipId);
        if (hayvan == null)
            return IslemSonucu.Hata("Evcil hayvan bulunamadı.");

        var hizmet = await _db.Hizmetler.FindAsync(hizmetId);
        if (hizmet == null)
            return IslemSonucu.Hata("Hizmet bulunamadı.");

        var hekim = await _db.Veterinerler
            .Include(v => v.Hizmetler)
            .Include(v => v.Musaitlikler)
            .FirstOrDefaultAsync(v => v.Id == veterinerId);
        if (hekim == null)
            return IslemSonucu.Hata("Veteriner bulunamadı.");

        if (!hekim.Hizmetler.Any(h => h.HizmetId == hizmetId))
            return IslemSonucu.Hata("Bu hekim seçilen hizmeti vermiyor.");

        var bitis = baslangic.AddMinutes(hizmet.SureDakika);
        var kontrol = await UygunlukKontrolAsync(hizmet, hekim, hayvanId, baslangic, bitis);
        if (!kontrol.Basarili)
            return kontrol;

        var randevu = new Randevu
        {
            EvcilHayvanId = hayvanId,
            HizmetId = hizmetId,
            VeterinerId = veterinerId,
            Baslangic = baslangic,
            Bitis = bitis,
            SureDakika = hizmet.SureDakika,
            Ucret = hizmet.Ucret,
            Durum = RandevuDurumu.Bekliyor
        };
        _db.Randevular.Add(randevu);
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Randevu Bekliyor durumunda oluşturuldu.", randevu.Id);
    }

    public async Task<IslemSonucu> IptalEtAsync(int id, string sahipId)
    {
        var randevu = await UyeRandevuGetirAsync(id, sahipId);
        if (randevu == null)
            return IslemSonucu.Hata("Randevu bulunamadı.");
        if (randevu.Durum is RandevuDurumu.Tamamlandi or RandevuDurumu.Iptal or RandevuDurumu.Reddedildi)
            return IslemSonucu.Hata("Bu randevu iptal edilemez.");

        randevu.Durum = RandevuDurumu.Iptal;
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Randevu iptal edildi.");
    }

    public async Task<IslemSonucu> DurumDegistirAsync(int id, string kullaniciId, bool adminMi, RandevuDurumu yeniDurum)
    {
        var randevu = await VeterinerRandevuGetirAsync(id, kullaniciId, adminMi);
        if (randevu == null)
            return IslemSonucu.Hata("Randevu bulunamadı.");
        if (randevu.Durum != RandevuDurumu.Bekliyor)
            return IslemSonucu.Hata("Yalnızca bekleyen randevu onaylanır veya reddedilir.");
        if (yeniDurum is not (RandevuDurumu.Onaylandi or RandevuDurumu.Reddedildi))
            return IslemSonucu.Hata("Geçersiz durum.");

        randevu.Durum = yeniDurum;
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam(yeniDurum == RandevuDurumu.Onaylandi ? "Randevu onaylandı." : "Randevu reddedildi.");
    }

    public async Task<IslemSonucu> MuayeneNotuEkleAsync(int randevuId, string kullaniciId, bool adminMi, string notMetni)
    {
        var randevu = await VeterinerRandevuGetirAsync(randevuId, kullaniciId, adminMi);
        if (randevu == null)
            return IslemSonucu.Hata("Randevu bulunamadı.");
        if (randevu.Durum != RandevuDurumu.Onaylandi && randevu.Durum != RandevuDurumu.Tamamlandi)
            return IslemSonucu.Hata("Yalnızca onaylı randevuya muayene notu girilir.");
        if (string.IsNullOrWhiteSpace(notMetni))
            return IslemSonucu.Hata("Muayene notu boş olamaz.");

        if (randevu.MuayeneNotu == null)
        {
            randevu.MuayeneNotu = new MuayeneNotu
            {
                NotMetni = notMetni.Trim(),
                YazilmaTarihi = _saat.GetLocalNow().DateTime
            };
        }
        else
        {
            randevu.MuayeneNotu.NotMetni = notMetni.Trim();
            randevu.MuayeneNotu.YazilmaTarihi = _saat.GetLocalNow().DateTime;
        }

        randevu.Durum = RandevuDurumu.Tamamlandi;
        await _db.SaveChangesAsync();
        return IslemSonucu.Tamam("Muayene notu kaydedildi, randevu tamamlandı.");
    }

    public async Task<IslemSonucu> UygunlukKontrolAsync(Hizmet hizmet, Veteriner hekim, int hayvanId, DateTime baslangic, DateTime bitis)
    {
        var simdi = _saat.GetLocalNow().DateTime;
        if (baslangic < simdi)
            return IslemSonucu.Hata("Geçmiş tarihe randevu alınamaz.");

        var klinikSaat = await _db.KlinikCalismaSaatleri
            .FirstOrDefaultAsync(s => s.KlinikId == hizmet.KlinikId && s.Gun == baslangic.DayOfWeek);
        if (klinikSaat == null || !klinikSaat.Acik)
            return IslemSonucu.Hata("Klinik bu günde kapalı.");

        var gunSaat = baslangic.TimeOfDay;
        var bitisSaat = bitis.TimeOfDay;
        if (bitis.Date > baslangic.Date)
            bitisSaat = new TimeSpan(24, 0, 0);

        if (gunSaat < klinikSaat.Acilis || bitisSaat > klinikSaat.Kapanis)
            return IslemSonucu.Hata("Seçilen saat klinik çalışma saatleri dışında.");

        var musait = hekim.Musaitlikler.FirstOrDefault(m => m.Gun == baslangic.DayOfWeek);
        if (musait == null || !musait.Musait)
            return IslemSonucu.Hata("Hekim bu günde müsait değil.");
        if (gunSaat < musait.Baslangic || bitisSaat > musait.Bitis)
            return IslemSonucu.Hata("Seçilen saat hekimin müsaitlik aralığının dışında.");

        var hekimRandevulari = await _db.Randevular
            .Where(r => r.VeterinerId == hekim.Id && BloklayanDurum(r.Durum))
            .ToListAsync();
        if (hekimRandevulari.Any(r => CakisiyorMu(baslangic, bitis, r.Baslangic, r.Bitis)))
            return IslemSonucu.Hata("Hekimin bu saatte başka bir randevusu var (süre çakışması).");

        var hayvanRandevulari = await _db.Randevular
            .Where(r => r.EvcilHayvanId == hayvanId && BloklayanDurum(r.Durum))
            .ToListAsync();
        if (hayvanRandevulari.Any(r => CakisiyorMu(baslangic, bitis, r.Baslangic, r.Bitis)))
            return IslemSonucu.Hata("Bu hayvanın aynı saatte başka bir randevusu var.");

        return IslemSonucu.Tamam();
    }

    private static TimeSpan MaxTime(TimeSpan a, TimeSpan b) => a > b ? a : b;
    private static TimeSpan MinTime(TimeSpan a, TimeSpan b) => a < b ? a : b;
}
