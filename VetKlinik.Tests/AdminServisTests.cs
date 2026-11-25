using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Tests;

public class AdminServisTests
{
    private static async Task<(ServiceProvider Saglayici, SqliteConnection Baglanti)> HazirlaAsync()
    {
        var baglanti = new SqliteConnection("Data Source=:memory:");
        await baglanti.OpenAsync();

        var servisler = new ServiceCollection();
        servisler.AddLogging();
        servisler.AddDbContext<UygulamaDbContext>(o => o.UseSqlite(baglanti));
        servisler.AddIdentity<UygulamaKullanicisi, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 3;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UygulamaDbContext>();

        servisler.AddScoped<KlinikServisi>();
        servisler.AddScoped<HizmetServisi>();
        servisler.AddScoped<VeterinerServisi>();

        var saglayici = servisler.BuildServiceProvider();
        using var kapsam = saglayici.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        await db.Database.MigrateAsync();
        await VeriTohumu.CalistirAsync(kapsam.ServiceProvider);

        return (saglayici, baglanti);
    }

    [Fact]
    public async Task Klinik_eklenir_ve_bagimli_kayit_varken_silinemez()
    {
        var (saglayici, baglanti) = await HazirlaAsync();
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var klinikler = kapsam.ServiceProvider.GetRequiredService<KlinikServisi>();
        var yeni = new Klinik { Ad = "Şube Kliniği", Adres = "Adapazarı", Telefon = "0264 111 22 33" };
        await klinikler.EkleAsync(yeni);

        var getirilen = await klinikler.GetirAsync(yeni.Id);
        Assert.NotNull(getirilen);
        Assert.Equal(7, getirilen!.CalismaSaatleri.Count);

        // Tohumlanan ilk klinik hizmet/veteriner barındırdığı için silinememeli.
        var ilkKlinik = (await klinikler.ListeleAsync()).First(k => k.Id != yeni.Id);
        var engelliSilme = await klinikler.SilAsync(ilkKlinik.Id);
        Assert.False(engelliSilme.Basarili);

        // Boş şube kliniği silinebilmeli.
        var bosSilme = await klinikler.SilAsync(yeni.Id);
        Assert.True(bosSilme.Basarili);
    }

    [Fact]
    public async Task Hizmet_eklenir_guncellenir_ve_randevusu_yoksa_silinir()
    {
        var (saglayici, baglanti) = await HazirlaAsync();
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var klinikler = kapsam.ServiceProvider.GetRequiredService<KlinikServisi>();
        var hizmetler = kapsam.ServiceProvider.GetRequiredService<HizmetServisi>();
        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();

        var klinik = (await klinikler.ListeleAsync()).First();
        var hizmet = new Hizmet { KlinikId = klinik.Id, Ad = "Röntgen", SureDakika = 30, Ucret = 950 };
        await hizmetler.EkleAsync(hizmet);

        hizmet.Ucret = 1100;
        await hizmetler.GuncelleAsync(hizmet);
        Assert.Equal(1100, (await hizmetler.GetirAsync(hizmet.Id))!.Ucret);

        // Randevusu olan hizmet silinememeli.
        var hayvan = await db.EvcilHayvanlar.FirstAsync();
        var vet = await db.Veterinerler.FirstAsync();
        db.Randevular.Add(new Randevu
        {
            EvcilHayvanId = hayvan.Id,
            HizmetId = hizmet.Id,
            VeterinerId = vet.Id,
            Baslangic = new DateTime(2026, 12, 5, 10, 0, 0),
            Bitis = new DateTime(2026, 12, 5, 10, 30, 0),
            SureDakika = 30,
            Ucret = 1100,
            Durum = RandevuDurumu.Bekliyor
        });
        await db.SaveChangesAsync();

        var engelliSilme = await hizmetler.SilAsync(hizmet.Id);
        Assert.False(engelliSilme.Basarili);

        // Randevusu olmayan yeni bir hizmet silinebilmeli.
        var bosHizmet = new Hizmet { KlinikId = klinik.Id, Ad = "Tırnak Kesimi", SureDakika = 15, Ucret = 150 };
        await hizmetler.EkleAsync(bosHizmet);
        var silme = await hizmetler.SilAsync(bosHizmet.Id);
        Assert.True(silme.Basarili);
    }

    [Fact]
    public async Task Veteriner_eklenince_rolu_atanir_ve_silinince_kullanicisi_da_temizlenir()
    {
        var (saglayici, baglanti) = await HazirlaAsync();
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var klinikler = kapsam.ServiceProvider.GetRequiredService<KlinikServisi>();
        var hizmetler = kapsam.ServiceProvider.GetRequiredService<HizmetServisi>();
        var veterinerler = kapsam.ServiceProvider.GetRequiredService<VeterinerServisi>();
        var kullanicilar = kapsam.ServiceProvider.GetRequiredService<UserManager<UygulamaKullanicisi>>();

        var klinik = (await klinikler.ListeleAsync()).First();
        var hizmet = (await hizmetler.ListeleAsync()).First();
        var uzmanlik = (await veterinerler.UzmanliklariGetirAsync()).First();

        var musaitlikler = Enum.GetValues<DayOfWeek>().Select(g => new VeterinerMusaitlik
        {
            Gun = g,
            Musait = g != DayOfWeek.Sunday,
            Baslangic = new TimeSpan(9, 0, 0),
            Bitis = new TimeSpan(17, 0, 0)
        });

        var ekleSonuc = await veterinerler.OlusturAsync(
            "Vet. Hek. Can Demir",
            "can.demir@patiev.local",
            "vet123",
            klinik.Id,
            new[] { hizmet.Id },
            new[] { uzmanlik.Id },
            musaitlikler);

        Assert.True(ekleSonuc.Basarili);
        var kullanici = await kullanicilar.FindByEmailAsync("can.demir@patiev.local");
        Assert.NotNull(kullanici);
        Assert.True(await kullanicilar.IsInRoleAsync(kullanici!, "Veteriner"));

        var silSonuc = await veterinerler.SilAsync(ekleSonuc.Id!.Value);
        Assert.True(silSonuc.Basarili);
        Assert.Null(await kullanicilar.FindByEmailAsync("can.demir@patiev.local"));
    }
}
