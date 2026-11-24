using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinik.Models;

public class Randevu
{
    public int Id { get; set; }

    public int EvcilHayvanId { get; set; }
    public EvcilHayvan EvcilHayvan { get; set; } = null!;

    public int HizmetId { get; set; }
    public Hizmet Hizmet { get; set; } = null!;

    public int VeterinerId { get; set; }
    public Veteriner Veteriner { get; set; } = null!;

    [Display(Name = "Başlangıç")]
    public DateTime Baslangic { get; set; }

    [Display(Name = "Bitiş")]
    public DateTime Bitis { get; set; }

    // Randevu anındaki kopya: hizmet fiyatı/süresi sonradan değişse bile eski randevu bozulmaz.
    [Display(Name = "Süre (dk)")]
    public int SureDakika { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Ücret (₺)")]
    public decimal Ucret { get; set; }

    public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

    public MuayeneNotu? MuayeneNotu { get; set; }
}
