using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels;

public class GirisViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }
}
