using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Controllers;

[Authorize(Roles = "Veteriner,Admin")]
public class VeterinerPanelController : Controller
{
    private readonly RandevuServisi _randevular;

    public VeterinerPanelController(RandevuServisi randevular)
    {
        _randevular = randevular;
    }

    public async Task<IActionResult> Index()
    {
        var liste = await _randevular.VeterinerRandevulariAsync(User.IdAl(), User.IsInRole("Admin"));
        return View(liste);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var randevu = await _randevular.VeterinerRandevuGetirAsync(id, User.IdAl(), User.IsInRole("Admin"));
        if (randevu == null)
            return NotFound();
        return View(randevu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onayla(int id)
    {
        var sonuc = await _randevular.DurumDegistirAsync(id, User.IdAl(), User.IsInRole("Admin"), RandevuDurumu.Onaylandi);
        if (!sonuc.Basarili && sonuc.Mesaj.Contains("bulunamadı"))
            return NotFound();
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Detay), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reddet(int id)
    {
        var sonuc = await _randevular.DurumDegistirAsync(id, User.IdAl(), User.IsInRole("Admin"), RandevuDurumu.Reddedildi);
        if (!sonuc.Basarili && sonuc.Mesaj.Contains("bulunamadı"))
            return NotFound();
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Detay), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MuayeneNotu(int id, string notMetni)
    {
        var sonuc = await _randevular.MuayeneNotuEkleAsync(id, User.IdAl(), User.IsInRole("Admin"), notMetni);
        if (!sonuc.Basarili && sonuc.Mesaj.Contains("bulunamadı"))
            return NotFound();
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Detay), new { id });
    }
}
