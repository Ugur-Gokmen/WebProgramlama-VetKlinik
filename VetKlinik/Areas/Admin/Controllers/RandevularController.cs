using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RandevularController : Controller
{
    private readonly RandevuServisi _randevular;
    private readonly RaporServisi _raporlar;

    public RandevularController(RandevuServisi randevular, RaporServisi raporlar)
    {
        _randevular = randevular;
        _raporlar = raporlar;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Ozet = await _raporlar.HizmetOzetiAsync();
        return View(await _randevular.VeterinerRandevulariAsync(User.IdAl(), true));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        await _randevular.DurumDegistirAsync(id, User.IdAl(), true, RandevuDurumu.Onaylandi);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id)
    {
        await _randevular.DurumDegistirAsync(id, User.IdAl(), true, RandevuDurumu.Reddedildi);
        return RedirectToAction(nameof(Index));
    }
}
