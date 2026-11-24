using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class ApiAnahtari
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = "";
    public UygulamaKullanicisi Kullanici { get; set; } = null!;

    [Required, StringLength(64)]
    public string AnahtarHash { get; set; } = "";

    public DateTime OlusturulmaTarihi { get; set; }
}
