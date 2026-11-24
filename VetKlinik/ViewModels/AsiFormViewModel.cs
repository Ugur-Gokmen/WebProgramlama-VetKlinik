using System.ComponentModel.DataAnnotations;

namespace VetKlinik.ViewModels;

public class AsiFormViewModel
{
    public int Id { get; set; }
    public int EvcilHayvanId { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Aşı adı")]
    public string AsiAdi { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Yapıldığı tarih")]
    public DateTime YapildigiTarih { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Sonraki doz tarihi")]
    public DateTime? SonrakiDozTarihi { get; set; }
}
