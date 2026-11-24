using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class VeterinerMusaitlik
{
    public int Id { get; set; }

    public int VeterinerId { get; set; }
    public Veteriner Veteriner { get; set; } = null!;

    [Display(Name = "Gün")]
    public DayOfWeek Gun { get; set; }

    [Display(Name = "Müsait mi?")]
    public bool Musait { get; set; }

    [Display(Name = "Başlangıç")]
    public TimeSpan Baslangic { get; set; }

    [Display(Name = "Bitiş")]
    public TimeSpan Bitis { get; set; }
}
