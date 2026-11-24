using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Models;

namespace VetKlinik.Data;

public class UygulamaDbContext : IdentityDbContext<UygulamaKullanicisi>
{
    public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
    {
    }

    public DbSet<Klinik> Klinikler => Set<Klinik>();
    public DbSet<KlinikCalismaSaati> KlinikCalismaSaatleri => Set<KlinikCalismaSaati>();
    public DbSet<Hizmet> Hizmetler => Set<Hizmet>();
    public DbSet<Uzmanlik> Uzmanliklar => Set<Uzmanlik>();
    public DbSet<Veteriner> Veterinerler => Set<Veteriner>();
    public DbSet<VeterinerHizmet> VeterinerHizmetleri => Set<VeterinerHizmet>();
    public DbSet<VeterinerUzmanlik> VeterinerUzmanliklari => Set<VeterinerUzmanlik>();
    public DbSet<VeterinerMusaitlik> VeterinerMusaitlikleri => Set<VeterinerMusaitlik>();
    public DbSet<EvcilHayvan> EvcilHayvanlar => Set<EvcilHayvan>();
    public DbSet<AsiKaydi> AsiKayitlari => Set<AsiKaydi>();
    public DbSet<Randevu> Randevular => Set<Randevu>();
    public DbSet<MuayeneNotu> MuayeneNotlari => Set<MuayeneNotu>();
    public DbSet<ApiAnahtari> ApiAnahtarlari => Set<ApiAnahtari>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VeterinerHizmet>()
            .HasKey(x => new { x.VeterinerId, x.HizmetId });

        builder.Entity<VeterinerUzmanlik>()
            .HasKey(x => new { x.VeterinerId, x.UzmanlikId });

        builder.Entity<Veteriner>()
            .HasOne(v => v.Kullanici)
            .WithMany()
            .HasForeignKey(v => v.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EvcilHayvan>()
            .HasOne(h => h.Sahip)
            .WithMany()
            .HasForeignKey(h => h.SahipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Randevu>()
            .HasOne(r => r.MuayeneNotu)
            .WithOne(n => n.Randevu)
            .HasForeignKey<MuayeneNotu>(n => n.RandevuId);

        builder.Entity<ApiAnahtari>()
            .HasIndex(a => a.AnahtarHash)
            .IsUnique();

        builder.Entity<ApiAnahtari>()
            .HasIndex(a => a.KullaniciId)
            .IsUnique();
    }
}
