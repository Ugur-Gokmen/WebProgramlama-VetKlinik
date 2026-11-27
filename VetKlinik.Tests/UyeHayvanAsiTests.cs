using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VetKlinik.Data;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Tests;

public class UyeHayvanAsiTests
{
    private sealed class SabitSaat(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class TestWebHostEnv(string kok) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "VetKlinik";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = kok;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = kok;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static async Task<(UygulamaDbContext Db, SqliteConnection Baglanti)> DbAcAsync()
    {
        var baglanti = new SqliteConnection("Data Source=:memory:");
        await baglanti.OpenAsync();
        var secenek = new DbContextOptionsBuilder<UygulamaDbContext>().UseSqlite(baglanti).Options;
        var db = new UygulamaDbContext(secenek);
        await db.Database.MigrateAsync();

        db.Users.AddRange(
            new UygulamaKullanicisi { Id = "uye-a", UserName = "a@test.local", Email = "a@test.local", AdSoyad = "Üye A" },
            new UygulamaKullanicisi { Id = "uye-b", UserName = "b@test.local", Email = "b@test.local", AdSoyad = "Üye B" });
        await db.SaveChangesAsync();
        return (db, baglanti);
    }

    [Fact]
    public async Task Uye_sadece_kendi_hayvanlarini_ve_asi_kayitlarini_gorebilir()
    {
        var (db, baglanti) = await DbAcAsync();
        using var _ = baglanti;
        using var __ = db;

        var hayvanServisi = new HayvanServisi(db);
        var saat = new SabitSaat(new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero));
        var asiServisi = new AsiServisi(db, saat);

        var pamuk = new EvcilHayvan { SahipId = "uye-a", Ad = "Pamuk", Tur = "Kedi", Irk = "Tekir", DogumTarihi = new DateTime(2023, 5, 1), Kilo = 4.1m };
        var boncuk = new EvcilHayvan { SahipId = "uye-b", Ad = "Boncuk", Tur = "Köpek", Irk = "Poodle", DogumTarihi = new DateTime(2022, 3, 10), Kilo = 6.5m };
        await hayvanServisi.EkleAsync(pamuk);
        await hayvanServisi.EkleAsync(boncuk);

        // Üye A sadece kendi hayvanını listelemeli ve Üye B'nin hayvanına erişememeli.
        var aHayvanlari = await hayvanServisi.ListeleAsync("uye-a");
        Assert.Single(aHayvanlari);
        Assert.Equal("Pamuk", aHayvanlari[0].Ad);
        Assert.Null(await hayvanServisi.GetirAsync(boncuk.Id, "uye-a"));
        Assert.False(await hayvanServisi.SilAsync(boncuk.Id, "uye-a"));

        // Aşı ekleme, takvim ayrımı ve sahiplik kontrolü.
        var gecmisAsi = new AsiKaydi { EvcilHayvanId = pamuk.Id, AsiAdi = "Kuduz", YapildigiTarih = new DateTime(2025, 11, 1), SonrakiDozTarihi = new DateTime(2026, 11, 1) };
        var yaklasanAsi = new AsiKaydi { EvcilHayvanId = pamuk.Id, AsiAdi = "Karma", YapildigiTarih = new DateTime(2026, 1, 15), SonrakiDozTarihi = new DateTime(2027, 1, 15) };
        var bAsi = new AsiKaydi { EvcilHayvanId = boncuk.Id, AsiAdi = "Kuduz", YapildigiTarih = new DateTime(2026, 2, 1) };
        await asiServisi.EkleAsync(gecmisAsi);
        await asiServisi.EkleAsync(yaklasanAsi);
        await asiServisi.EkleAsync(bAsi);

        var takvim = await asiServisi.TakvimAsync(pamuk.Id, "uye-a");
        Assert.NotNull(takvim);
        Assert.Equal(2, takvim!.Value.Gecmis.Count);
        Assert.Single(takvim.Value.Yaklasan);
        Assert.Equal("Karma", takvim.Value.Yaklasan[0].AsiAdi);

        Assert.Null(await asiServisi.GetirAsync(bAsi.Id, "uye-a"));
        Assert.False(await asiServisi.SilAsync(bAsi.Id, "uye-a"));
        Assert.Null(await asiServisi.TakvimAsync(boncuk.Id, "uye-a"));
    }

    [Fact]
    public async Task FotoServisi_tur_boyut_ve_icerik_dogrulamasi_yapar()
    {
        var geciciKok = Path.Combine(Path.GetTempPath(), "vetklinik-foto-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(geciciKok);
        try
        {
            var env = new TestWebHostEnv(geciciKok);
            var servis = new FotoServisi();

            // Yasaklı uzantı
            using var txtAkis = new MemoryStream("merhaba"u8.ToArray());
            var txtDosya = new FormFile(txtAkis, 0, txtAkis.Length, "Foto", "not.txt");
            var txtSonuc = await servis.KaydetAsync(env, txtDosya);
            Assert.False(txtSonuc.Tamam);

            // Sahte içerikli .jpg
            using var sahteAkis = new MemoryStream("MZsahte"u8.ToArray());
            var sahteDosya = new FormFile(sahteAkis, 0, sahteAkis.Length, "Foto", "sahte.jpg");
            var sahteSonuc = await servis.KaydetAsync(env, sahteDosya);
            Assert.False(sahteSonuc.Tamam);

            // 2 MB üstü dosya
            var buyukBayt = new byte[(2 * 1024 * 1024) + 1];
            buyukBayt[0] = 0xFF; buyukBayt[1] = 0xD8; buyukBayt[2] = 0xFF;
            using var buyukAkis = new MemoryStream(buyukBayt);
            var buyukDosya = new FormFile(buyukAkis, 0, buyukAkis.Length, "Foto", "buyuk.jpg");
            var buyukSonuc = await servis.KaydetAsync(env, buyukDosya);
            Assert.False(buyukSonuc.Tamam);

            // Geçerli PNG başlığı
            var pngBayt = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using var pngAkis = new MemoryStream(pngBayt);
            var pngDosya = new FormFile(pngAkis, 0, pngAkis.Length, "Foto", "pamuk.png");
            var pngSonuc = await servis.KaydetAsync(env, pngDosya);
            Assert.True(pngSonuc.Tamam);
            Assert.StartsWith("/uploads/hayvanlar/", pngSonuc.Yol);

            servis.Sil(env, pngSonuc.Yol);
            var beklenenDosya = Path.Combine(geciciKok, pngSonuc.Yol!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.False(File.Exists(beklenenDosya));
        }
        finally
        {
            if (Directory.Exists(geciciKok))
                Directory.Delete(geciciKok, true);
        }
    }
}
