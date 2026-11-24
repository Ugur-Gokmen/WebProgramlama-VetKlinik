using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class Veteriner
{
    public int Id { get; set; }

    public int KlinikId { get; set; }
    public Klinik Klinik { get; set; } = null!;

    // Identity kullanıcısı (giriş hesabı)
    [Required]
    public string KullaniciId { get; set; } = "";
    public UygulamaKullanicisi Kullanici { get; set; } = null!;

    [Required, StringLength(80)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    public ICollection<VeterinerUzmanlik> Uzmanliklar { get; set; } = new List<VeterinerUzmanlik>();
    public ICollection<VeterinerHizmet> Hizmetler { get; set; } = new List<VeterinerHizmet>();
    public ICollection<VeterinerMusaitlik> Musaitlikler { get; set; } = new List<VeterinerMusaitlik>();
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
}
