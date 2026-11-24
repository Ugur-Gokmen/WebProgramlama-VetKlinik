using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class Klinik
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    [Display(Name = "Klinik adı")]
    public string Ad { get; set; } = "";

    [StringLength(200)]
    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [StringLength(20)]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    public ICollection<KlinikCalismaSaati> CalismaSaatleri { get; set; } = new List<KlinikCalismaSaati>();
    public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
    public ICollection<Veteriner> Veterinerler { get; set; } = new List<Veteriner>();
}
