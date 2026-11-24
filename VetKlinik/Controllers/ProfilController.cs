using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Services;

namespace VetKlinik.Controllers;

[Authorize]
public class ProfilController : Controller
{
    private readonly ApiAnahtarServisi _anahtarlar;

    public ProfilController(ApiAnahtarServisi anahtarlar)
    {
        _anahtarlar = anahtarlar;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.AnahtarVar = await _anahtarlar.AnahtarVarMiAsync(User.IdAl());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnahtarUret()
    {
        var ham = await _anahtarlar.YeniAnahtarUretAsync(User.IdAl());
        TempData["YeniAnahtar"] = ham;
        return RedirectToAction(nameof(Index));
    }
}
