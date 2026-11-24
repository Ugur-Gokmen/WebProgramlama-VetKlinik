using System.ComponentModel.DataAnnotations;
using VetKlinik.Models;

namespace VetKlinik.ViewModels;

public class VeterinerFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    [Required, EmailAddress]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string? Sifre { get; set; }

    [Required]
    [Display(Name = "Klinik")]
    public int KlinikId { get; set; }

    [Display(Name = "Hizmetler")]
    public List<int> SecilenHizmetler { get; set; } = new();

    [Display(Name = "Uzmanlıklar")]
    public List<int> SecilenUzmanliklar { get; set; } = new();

    public List<VeterinerMusaitlik> Musaitlikler { get; set; } = new();
}
