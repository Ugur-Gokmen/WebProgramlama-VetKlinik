using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels;

public class KayitViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    [Required, EmailAddress]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [StringLength(50, MinimumLength = 3)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre tekrar")]
    public string SifreTekrar { get; set; } = "";
}
