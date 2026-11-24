using System.ComponentModel.DataAnnotations;
using VetKlinik.Models;

namespace VetKlinik.ViewModels;

public class KlinikFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    [Display(Name = "Klinik adı")]
    public string Ad { get; set; } = "";

    [StringLength(200)]
    public string? Adres { get; set; }

    [StringLength(20)]
    public string? Telefon { get; set; }

    public List<KlinikCalismaSaati> CalismaSaatleri { get; set; } = new();
}
