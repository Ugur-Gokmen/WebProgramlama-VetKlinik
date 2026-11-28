using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Tests;

public class RandevuServisiTests
{
    private sealed class SabitSaat(DateTimeOffset simdi) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => simdi;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static async Task<(ServiceProvider Saglayici, SqliteConnection Baglanti)> HazirlaAsync(DateTimeOffset simdi)
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

        servisler.AddSingleton<TimeProvider>(new SabitSaat(simdi));
        servisler.AddScoped<RandevuServisi>();

        var saglayici = servisler.BuildServiceProvider();
        using var kapsam = saglayici.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        await db.Database.MigrateAsync();
        await VeriTohumu.CalistirAsync(kapsam.ServiceProvider);

        return (saglayici, baglanti);
    }

    [Fact]
    public async Task Gecmis_tarih_kapali_gun_ve_musaitlik_disi_saatler_reddedilir()
    {
        // 2026-12-01 Salı günü 10:00
        var (saglayici, baglanti) = await HazirlaAsync(new DateTimeOffset(2026, 12, 1, 10, 0, 0, TimeSpan.Zero));
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        var servis = kapsam.ServiceProvider.GetRequiredService<RandevuServisi>();

        var hayvan = await db.EvcilHayvanlar.FirstAsync();
        var muayene = await db.Hizmetler.FirstAsync(h => h.Ad == "Muayene");
        var elif = await db.Veterinerler.FirstAsync(v => v.AdSoyad.Contains("Elif"));

        // Geçmiş tarih
        var gecmis = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 1, 9, 30, 0));
        Assert.False(gecmis.Basarili);
        Assert.Contains("Geçmiş", gecmis.Mesaj);

        // Pazar günü (2026-12-06): klinik kapalı
        var pazar = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 6, 11, 0, 0));
        Assert.False(pazar.Basarili);
        Assert.Contains("kapalı", pazar.Mesaj);

        // Elif Cumartesi müsait değil (2026-12-05)
        var cumartesi = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 5, 11, 0, 0));
        Assert.False(cumartesi.Basarili);
        Assert.Contains("müsait değil", cumartesi.Mesaj);

        // Elif 17:00'da bitiriyor, 30 dk muayene 16:45'te başlarsa 17:15'e taşar
        var saatDisi = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 2, 16, 45, 0));
        Assert.False(saatDisi.Basarili);
        Assert.Contains("müsaitlik", saatDisi.Mesaj);

        // Elif Kısırlaştırma hizmetini vermiyor
        var kisir = await db.Hizmetler.FirstAsync(h => h.Ad == "Kısırlaştırma");
        var yanlisHizmet = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, kisir.Id, elif.Id, new DateTime(2026, 12, 2, 11, 0, 0));
        Assert.False(yanlisHizmet.Basarili);
        Assert.Contains("vermiyor", yanlisHizmet.Mesaj);
    }

    [Fact]
    public async Task Sure_cakismasi_bloklar_tam_bitiste_ve_reddedilende_izin_verir()
    {
        var (saglayici, baglanti) = await HazirlaAsync(new DateTimeOffset(2026, 12, 1, 8, 0, 0, TimeSpan.Zero));
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        var servis = kapsam.ServiceProvider.GetRequiredService<RandevuServisi>();

        var hayvanlar = await db.EvcilHayvanlar.ToListAsync();
        var pamuk = hayvanlar[0];
        var karabas = hayvanlar[1];
        var kisir = await db.Hizmetler.FirstAsync(h => h.Ad == "Kısırlaştırma"); // 90 dk
        var muayene = await db.Hizmetler.FirstAsync(h => h.Ad == "Muayene"); // 30 dk
        var mert = await db.Veterinerler.FirstAsync(v => v.AdSoyad.Contains("Mert"));
        var elif = await db.Veterinerler.FirstAsync(v => v.AdSoyad.Contains("Elif"));

        // Mert 10:00 - 11:30 arası Kısırlaştırma (90 dk) randevusu (Bekliyor)
        var ilk = await servis.OlusturAsync(pamuk.SahipId, pamuk.Id, kisir.Id, mert.Id, new DateTime(2026, 12, 2, 10, 0, 0));
        Assert.True(ilk.Basarili);

        var kayit = await db.Randevular.FindAsync(ilk.Id!.Value);
        Assert.NotNull(kayit);
        Assert.Equal(RandevuDurumu.Bekliyor, kayit!.Durum);
        Assert.Equal(90, kayit.SureDakika);
        Assert.Equal(kisir.Ucret, kayit.Ucret);

        // 11:00 - 11:30 Mert ile çakışır (mevcut 11:30'da bitiyor)
        var cakisan = await servis.OlusturAsync(karabas.SahipId, karabas.Id, muayene.Id, mert.Id, new DateTime(2026, 12, 2, 11, 0, 0));
        Assert.False(cakisan.Basarili);
        Assert.Contains("çakışma", cakisan.Mesaj);

        // Aynı hayvan (Pamuk) aynı saatte başka hekimle (Elif) randevu alamaz
        var hayvanCakisma = await servis.OlusturAsync(pamuk.SahipId, pamuk.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 2, 10, 30, 0));
        Assert.False(hayvanCakisma.Basarili);
        Assert.Contains("hayvan", hayvanCakisma.Mesaj);

        // Tam bitişte (11:30) Mert ile yeni randevu çakışmaz
        var tamBitis = await servis.OlusturAsync(karabas.SahipId, karabas.Id, muayene.Id, mert.Id, new DateTime(2026, 12, 2, 11, 30, 0));
        Assert.True(tamBitis.Basarili);

        // İlk randevu reddedilirse 10:00 slotu yeniden açılmalı
        var redSonuc = await servis.DurumDegistirAsync(ilk.Id!.Value, mert.KullaniciId, false, RandevuDurumu.Reddedildi);
        Assert.True(redSonuc.Basarili);

        var tekrar = await servis.OlusturAsync(pamuk.SahipId, pamuk.Id, muayene.Id, mert.Id, new DateTime(2026, 12, 2, 10, 0, 0));
        Assert.True(tekrar.Basarili);
    }

    [Fact]
    public async Task Uygun_slot_uretimi_saat_ve_cakismalari_filtreler()
    {
        // Bugün 2026-12-02 Çarşamba saat 14:15
        var (saglayici, baglanti) = await HazirlaAsync(new DateTimeOffset(2026, 12, 2, 14, 15, 0, TimeSpan.Zero));
        using var _ = baglanti;
        using var __ = saglayici;
        using var kapsam = saglayici.CreateScope();

        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        var servis = kapsam.ServiceProvider.GetRequiredService<RandevuServisi>();

        var hayvan = await db.EvcilHayvanlar.FirstAsync();
        var muayene = await db.Hizmetler.FirstAsync(h => h.Ad == "Muayene"); // 30 dk
        var elif = await db.Veterinerler.FirstAsync(v => v.AdSoyad.Contains("Elif")); // 09:00-17:00

        // Elif için 15:00-15:30 dolu olsun
        var olustur = await servis.OlusturAsync(hayvan.SahipId, hayvan.Id, muayene.Id, elif.Id, new DateTime(2026, 12, 2, 15, 0, 0));
        Assert.True(olustur.Basarili);

        var slotlar = await servis.UygunSlotlariGetirAsync(muayene.Id, new DateTime(2026, 12, 2), elif.Id);
        // 14:15'ten büyük slotlar: 14:30, 15:30, 16:00, 16:30 (15:00 dolu olduğu için hariç, 17:00 kapanış sonrası hariç)
        Assert.Equal(new[]
        {
            new DateTime(2026, 12, 2, 14, 30, 0),
            new DateTime(2026, 12, 2, 15, 30, 0),
            new DateTime(2026, 12, 2, 16, 0, 0),
            new DateTime(2026, 12, 2, 16, 30, 0)
        }, slotlar.Select(s => s.Baslangic).ToArray());

        // Pazar günü hiç slot üretilmemeli
        var pazarSlotlar = await servis.UygunSlotlariGetirAsync(muayene.Id, new DateTime(2026, 12, 6));
        Assert.Empty(pazarSlotlar);
    }
}
