using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class MuayeneNotu
{
    public int Id { get; set; }

    public int RandevuId { get; set; }
    public Randevu Randevu { get; set; } = null!;

    [Required, StringLength(2000)]
    [Display(Name = "Not")]
    public string NotMetni { get; set; } = "";

    public DateTime YazilmaTarihi { get; set; }
}
