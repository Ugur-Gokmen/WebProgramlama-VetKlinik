using Microsoft.AspNetCore.Mvc;

namespace VetKlinik.Controllers;

public class HataController : Controller
{
    [Route("Hata/Kod")]
    public IActionResult Kod(int kod)
    {
        Response.StatusCode = kod;
        ViewBag.Kod = kod;
        ViewBag.Mesaj = kod switch
        {
            404 => "Aradığınız sayfa veya kayıt bulunamadı.",
            403 => "Bu işlem için yetkiniz yok.",
            401 => "Bu sayfa için giriş yapmalısınız.",
            _ => "Bir hata oluştu."
        };
        return View("Index");
    }

    public IActionResult Sunucu()
    {
        ViewBag.Kod = 500;
        ViewBag.Mesaj = "Sunucu hatası oluştu.";
        return View("Index");
    }
}
