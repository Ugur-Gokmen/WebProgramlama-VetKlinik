using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VetKlinik.Models;

public class Hizmet
{
    public int Id { get; set; }

    public int KlinikId { get; set; }
    [ValidateNever]
    public Klinik Klinik { get; set; } = null!;

    [Required, StringLength(80)]
    [Display(Name = "Hizmet adı")]
    public string Ad { get; set; } = "";

    [Range(15, 240)]
    [Display(Name = "Süre (dk)")]
    public int SureDakika { get; set; }

    [Range(0, 100000)]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Ücret (₺)")]
    public decimal Ucret { get; set; }

    public ICollection<VeterinerHizmet> VeterinerHizmetleri { get; set; } = new List<VeterinerHizmet>();
}
