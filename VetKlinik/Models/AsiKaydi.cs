using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class AsiKaydi
{
    public int Id { get; set; }

    public int EvcilHayvanId { get; set; }
    public EvcilHayvan EvcilHayvan { get; set; } = null!;

    [Required, StringLength(80)]
    [Display(Name = "Aşı adı")]
    public string AsiAdi { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Yapıldığı tarih")]
    public DateTime YapildigiTarih { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Sonraki doz tarihi")]
    public DateTime? SonrakiDozTarihi { get; set; }
}
