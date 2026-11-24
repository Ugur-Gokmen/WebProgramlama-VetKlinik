using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinik.Models;

public class EvcilHayvan
{
    public int Id { get; set; }

    [Required]
    public string SahipId { get; set; } = "";
    public UygulamaKullanicisi Sahip { get; set; } = null!;

    [Required, StringLength(60)]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = "";

    [Required, StringLength(40)]
    [Display(Name = "Tür")]
    public string Tur { get; set; } = "";

    [Required, StringLength(60)]
    [Display(Name = "Irk")]
    public string Irk { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Doğum tarihi")]
    public DateTime DogumTarihi { get; set; }

    [Range(0.1, 200)]
    [Column(TypeName = "decimal(6,2)")]
    [Display(Name = "Kilo (kg)")]
    public decimal Kilo { get; set; }

    [StringLength(260)]
    [Display(Name = "Fotoğraf")]
    public string? FotoYolu { get; set; }

    public ICollection<AsiKaydi> AsiKayitlari { get; set; } = new List<AsiKaydi>();
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
}
