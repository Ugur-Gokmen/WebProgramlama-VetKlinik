using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels;

public class HayvanFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = "";

    [Required, StringLength(40)]
    [Display(Name = "Tür")]
    public string Tur { get; set; } = "";

    [Required, StringLength(60)]
    [Display(Name = "Irk")]
    public string Irk { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Doğum tarihi")]
    public DateTime DogumTarihi { get; set; } = DateTime.Today.AddYears(-1);

    [Required, Range(0.1, 200)]
    [Display(Name = "Kilo (kg)")]
    public decimal Kilo { get; set; }

    [Display(Name = "Fotoğraf")]
    public IFormFile? Foto { get; set; }

    public string? MevcutFoto { get; set; }
}
