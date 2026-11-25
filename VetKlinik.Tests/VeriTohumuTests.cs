using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VetKlinik.Data;
using VetKlinik.Models;

namespace VetKlinik.Tests;

public class VeriTohumuTests
{
    [Fact]
    public async Task Ilk_migration_ve_seed_verileri_eksiksiz_olusturulur()
    {
        using var baglanti = new SqliteConnection("Data Source=:memory:");
        await baglanti.OpenAsync();

        var servisler = new ServiceCollection();
        servisler.AddLogging();
        servisler.AddDbContext<UygulamaDbContext>(o => o.UseSqlite(baglanti));
        servisler.AddIdentity<UygulamaKullanicisi, IdentityRole>(secenek =>
            {
                secenek.Password.RequireDigit = false;
                secenek.Password.RequireLowercase = false;
                secenek.Password.RequireUppercase = false;
                secenek.Password.RequireNonAlphanumeric = false;
                secenek.Password.RequiredLength = 3;
                secenek.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UygulamaDbContext>();

        using var saglayici = servisler.BuildServiceProvider();
        using var kapsam = saglayici.CreateScope();

        var db = kapsam.ServiceProvider.GetRequiredService<UygulamaDbContext>();
        await db.Database.MigrateAsync();

        await VeriTohumu.CalistirAsync(kapsam.ServiceProvider);
        // İkinci kez çalıştırıldığında yinelenen kayıt oluşmamalı.
        await VeriTohumu.CalistirAsync(kapsam.ServiceProvider);

        var rolYoneticisi = kapsam.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True(await rolYoneticisi.RoleExistsAsync("Admin"));
        Assert.True(await rolYoneticisi.RoleExistsAsync("Veteriner"));
        Assert.True(await rolYoneticisi.RoleExistsAsync("Uye"));

        var kullaniciYoneticisi = kapsam.ServiceProvider.GetRequiredService<UserManager<UygulamaKullanicisi>>();
        var admin = await kullaniciYoneticisi.FindByEmailAsync(VeriTohumu.AdminEposta);
        Assert.NotNull(admin);
        Assert.True(await kullaniciYoneticisi.IsInRoleAsync(admin, "Admin"));
        Assert.True(await kullaniciYoneticisi.CheckPasswordAsync(admin, VeriTohumu.AdminSifre));

        Assert.Equal(1, await db.Klinikler.CountAsync());
        Assert.Equal(7, await db.KlinikCalismaSaatleri.CountAsync());
        Assert.Equal(5, await db.Hizmetler.CountAsync());
        Assert.Equal(4, await db.Uzmanliklar.CountAsync());
        Assert.Equal(2, await db.Veterinerler.CountAsync());
        Assert.Equal(2, await db.EvcilHayvanlar.CountAsync());
        Assert.Equal(2, await db.AsiKayitlari.CountAsync());
    }
}
