using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data;

// Uygulama ilk açıldığında roller, admin, örnek klinik/hekim/üye ekler.
public static class VeriTohumu
{
    public const string AdminEposta = "B241210053@sakarya.edu.tr";
    public const string AdminSifre = "sau";

    public static async Task CalistirAsync(IServiceProvider servisler)
    {
        var roller = servisler.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { "Admin", "Veteriner", "Uye" })
        {
            if (!await roller.RoleExistsAsync(rol))
                await roller.CreateAsync(new IdentityRole(rol));
        }

        var kullanicilar = servisler.GetRequiredService<UserManager<UygulamaKullanicisi>>();
        var db = servisler.GetRequiredService<UygulamaDbContext>();

        var admin = await AdminOlustur(kullanicilar);
        if (!await db.Klinikler.AnyAsync())
            await KlinikVeHizmetleriEkle(db);

        if (!await db.Veterinerler.AnyAsync())
            await VeterinerleriEkle(db, kullanicilar);

        if (!await kullanicilar.Users.AnyAsync(u => u.Email == "uye@patiev.local"))
            await OrnekUyeEkle(db, kullanicilar);

        _ = admin;
    }

    private static async Task<UygulamaKullanicisi> AdminOlustur(UserManager<UygulamaKullanicisi> kullanicilar)
    {
        var admin = await kullanicilar.FindByEmailAsync(AdminEposta);
        if (admin != null)
            return admin;

        admin = new UygulamaKullanicisi
        {
            UserName = AdminEposta,
            Email = AdminEposta,
            EmailConfirmed = true,
            AdSoyad = "Proje Yöneticisi"
        };
        var sonuc = await kullanicilar.CreateAsync(admin, AdminSifre);
        if (!sonuc.Succeeded)
            throw new InvalidOperationException("Admin oluşturulamadı: " + string.Join(", ", sonuc.Errors.Select(e => e.Description)));

        await kullanicilar.AddToRoleAsync(admin, "Admin");
        return admin;
    }

    private static async Task KlinikVeHizmetleriEkle(UygulamaDbContext db)
    {
        var klinik = new Klinik
        {
            Ad = "Pati Ev Veteriner Kliniği",
            Adres = "Serdivan, Sakarya",
            Telefon = "0264 000 00 00"
        };

        foreach (DayOfWeek gun in Enum.GetValues<DayOfWeek>())
        {
            var pazar = gun == DayOfWeek.Sunday;
            klinik.CalismaSaatleri.Add(new KlinikCalismaSaati
            {
                Gun = gun,
                Acik = !pazar,
                Acilis = new TimeSpan(9, 0, 0),
                Kapanis = new TimeSpan(18, 0, 0)
            });
        }

        klinik.Hizmetler.Add(new Hizmet { Ad = "Muayene", SureDakika = 30, Ucret = 500 });
        klinik.Hizmetler.Add(new Hizmet { Ad = "Aşı", SureDakika = 15, Ucret = 350 });
        klinik.Hizmetler.Add(new Hizmet { Ad = "Kısırlaştırma", SureDakika = 90, Ucret = 2500 });
        klinik.Hizmetler.Add(new Hizmet { Ad = "Diş temizliği", SureDakika = 45, Ucret = 1200 });
        klinik.Hizmetler.Add(new Hizmet { Ad = "Tıraş ve bakım", SureDakika = 40, Ucret = 600 });

        db.Uzmanliklar.AddRange(
            new Uzmanlik { Ad = "Küçük hayvan" },
            new Uzmanlik { Ad = "Egzotik hayvan" },
            new Uzmanlik { Ad = "Cerrahi" },
            new Uzmanlik { Ad = "Dermatoloji" });

        db.Klinikler.Add(klinik);
        await db.SaveChangesAsync();
    }

    private static async Task VeterinerleriEkle(UygulamaDbContext db, UserManager<UygulamaKullanicisi> kullanicilar)
    {
        var klinik = await db.Klinikler.Include(k => k.Hizmetler).FirstAsync();
        var uzmanliklar = await db.Uzmanliklar.ToListAsync();
        var muayene = klinik.Hizmetler.First(h => h.Ad == "Muayene");
        var asi = klinik.Hizmetler.First(h => h.Ad == "Aşı");
        var kisir = klinik.Hizmetler.First(h => h.Ad == "Kısırlaştırma");
        var dis = klinik.Hizmetler.First(h => h.Ad == "Diş temizliği");
        var bakim = klinik.Hizmetler.First(h => h.Ad == "Tıraş ve bakım");

        var elif = await HekimKullanicisiOlustur(kullanicilar, "elif.kaya@patiev.local", "Elif Kaya");
        var mert = await HekimKullanicisiOlustur(kullanicilar, "mert.yilmaz@patiev.local", "Mert Yılmaz");

        var vetElif = new Veteriner
        {
            KlinikId = klinik.Id,
            KullaniciId = elif.Id,
            AdSoyad = "Dt. Elif Kaya"
        };
        vetElif.Uzmanliklar.Add(new VeterinerUzmanlik { UzmanlikId = uzmanliklar.First(u => u.Ad == "Küçük hayvan").Id });
        vetElif.Uzmanliklar.Add(new VeterinerUzmanlik { UzmanlikId = uzmanliklar.First(u => u.Ad == "Dermatoloji").Id });
        vetElif.Hizmetler.Add(new VeterinerHizmet { HizmetId = muayene.Id });
        vetElif.Hizmetler.Add(new VeterinerHizmet { HizmetId = asi.Id });
        vetElif.Hizmetler.Add(new VeterinerHizmet { HizmetId = bakim.Id });
        HaftalikMusaitlikEkle(vetElif, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), pazarHaric: true, cumartesiKapali: true);

        var vetMert = new Veteriner
        {
            KlinikId = klinik.Id,
            KullaniciId = mert.Id,
            AdSoyad = "Dt. Mert Yılmaz"
        };
        vetMert.Uzmanliklar.Add(new VeterinerUzmanlik { UzmanlikId = uzmanliklar.First(u => u.Ad == "Cerrahi").Id });
        vetMert.Uzmanliklar.Add(new VeterinerUzmanlik { UzmanlikId = uzmanliklar.First(u => u.Ad == "Küçük hayvan").Id });
        vetMert.Hizmetler.Add(new VeterinerHizmet { HizmetId = muayene.Id });
        vetMert.Hizmetler.Add(new VeterinerHizmet { HizmetId = kisir.Id });
        vetMert.Hizmetler.Add(new VeterinerHizmet { HizmetId = dis.Id });
        HaftalikMusaitlikEkle(vetMert, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0), pazarHaric: true, cumartesiKapali: false);

        db.Veterinerler.AddRange(vetElif, vetMert);
        await db.SaveChangesAsync();
    }

    private static void HaftalikMusaitlikEkle(Veteriner vet, TimeSpan bas, TimeSpan bit, bool pazarHaric, bool cumartesiKapali)
    {
        foreach (DayOfWeek gun in Enum.GetValues<DayOfWeek>())
        {
            var kapali = (pazarHaric && gun == DayOfWeek.Sunday) || (cumartesiKapali && gun == DayOfWeek.Saturday);
            vet.Musaitlikler.Add(new VeterinerMusaitlik
            {
                Gun = gun,
                Musait = !kapali,
                Baslangic = bas,
                Bitis = bit
            });
        }
    }

    private static async Task<UygulamaKullanicisi> HekimKullanicisiOlustur(UserManager<UygulamaKullanicisi> kullanicilar, string eposta, string ad)
    {
        var varOlan = await kullanicilar.FindByEmailAsync(eposta);
        if (varOlan != null)
            return varOlan;

        var user = new UygulamaKullanicisi
        {
            UserName = eposta,
            Email = eposta,
            EmailConfirmed = true,
            AdSoyad = ad
        };
        var sonuc = await kullanicilar.CreateAsync(user, "vet123");
        if (!sonuc.Succeeded)
            throw new InvalidOperationException("Veteriner hesabı oluşturulamadı: " + string.Join(", ", sonuc.Errors.Select(e => e.Description)));
        await kullanicilar.AddToRoleAsync(user, "Veteriner");
        return user;
    }

    private static async Task OrnekUyeEkle(UygulamaDbContext db, UserManager<UygulamaKullanicisi> kullanicilar)
    {
        var uye = new UygulamaKullanicisi
        {
            UserName = "uye@patiev.local",
            Email = "uye@patiev.local",
            EmailConfirmed = true,
            AdSoyad = "Ayşe Demir"
        };
        var sonuc = await kullanicilar.CreateAsync(uye, "uye123");
        if (!sonuc.Succeeded)
            throw new InvalidOperationException("Üye oluşturulamadı: " + string.Join(", ", sonuc.Errors.Select(e => e.Description)));
        await kullanicilar.AddToRoleAsync(uye, "Uye");

        var pamuk = new EvcilHayvan
        {
            SahipId = uye.Id,
            Ad = "Pamuk",
            Tur = "Kedi",
            Irk = "Tekir",
            DogumTarihi = new DateTime(2022, 4, 12),
            Kilo = 4.2m
        };
        pamuk.AsiKayitlari.Add(new AsiKaydi
        {
            AsiAdi = "Karma (FVRCP)",
            YapildigiTarih = new DateTime(2025, 10, 1),
            SonrakiDozTarihi = new DateTime(2026, 10, 1)
        });

        var karabas = new EvcilHayvan
        {
            SahipId = uye.Id,
            Ad = "Karabaş",
            Tur = "Köpek",
            Irk = "Kangal",
            DogumTarihi = new DateTime(2021, 8, 3),
            Kilo = 38m
        };
        karabas.AsiKayitlari.Add(new AsiKaydi
        {
            AsiAdi = "Kuduz",
            YapildigiTarih = new DateTime(2025, 6, 15),
            SonrakiDozTarihi = new DateTime(2026, 6, 15)
        });

        db.EvcilHayvanlar.AddRange(pamuk, karabas);
        await db.SaveChangesAsync();
    }
}
