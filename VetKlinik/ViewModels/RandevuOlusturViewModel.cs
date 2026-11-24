using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VetKlinik.ViewModels;

public class RandevuOlusturViewModel
{
    [Required]
    [Display(Name = "Evcil hayvan")]
    public int EvcilHayvanId { get; set; }

    [Required]
    [Display(Name = "Hizmet")]
    public int HizmetId { get; set; }

    [Required]
    [Display(Name = "Veteriner")]
    public int VeterinerId { get; set; }

    [Required]
    [Display(Name = "Tarih ve saat")]
    public DateTime Baslangic { get; set; } = DateTime.Now.AddHours(1);

    public SelectList? Hayvanlar { get; set; }
    public SelectList? Hizmetler { get; set; }
    public SelectList? Veterinerler { get; set; }
}
