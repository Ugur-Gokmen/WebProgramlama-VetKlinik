using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

// Identity kullanıcısı. Veteriner kaydı bu kullanıcıya bağlanır.
public class UygulamaKullanicisi : IdentityUser
{
    [Required, StringLength(80)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";
}
