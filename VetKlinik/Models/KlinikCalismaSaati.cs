using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VetKlinik.Models;

public class KlinikCalismaSaati
{
    public int Id { get; set; }

    public int KlinikId { get; set; }
    [ValidateNever]
    public Klinik Klinik { get; set; } = null!;

    [Display(Name = "Gün")]
    public DayOfWeek Gun { get; set; }

    [Display(Name = "Açık mı?")]
    public bool Acik { get; set; }

    [Display(Name = "Açılış")]
    public TimeSpan Acilis { get; set; }

    [Display(Name = "Kapanış")]
    public TimeSpan Kapanis { get; set; }
}
