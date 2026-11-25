using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HizmetlerController : Controller
{
    private readonly HizmetServisi _hizmetler;
    private readonly KlinikServisi _klinikler;

    public HizmetlerController(HizmetServisi hizmetler, KlinikServisi klinikler)
    {
        _hizmetler = hizmetler;
        _klinikler = klinikler;
    }

    public async Task<IActionResult> Index()
        => View(await _hizmetler.ListeleAsync());

    public async Task<IActionResult> Detay(int id)
    {
        var h = await _hizmetler.GetirAsync(id);
        return h == null ? NotFound() : View(h);
    }

    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        await KlinikleriDoldur();
        return View(new Hizmet { SureDakika = 30, Ucret = 500 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(Hizmet model)
    {
        if (!ModelState.IsValid)
        {
            await KlinikleriDoldur();
            return View(model);
        }
        await _hizmetler.EkleAsync(model);
        TempData["Mesaj"] = "Hizmet eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var h = await _hizmetler.GetirAsync(id);
        if (h == null)
            return NotFound();
        await KlinikleriDoldur();
        return View(h);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, Hizmet model)
    {
        if (id != model.Id)
            return NotFound();
        var mevcut = await _hizmetler.GetirAsync(id);
        if (mevcut == null)
            return NotFound();
        if (!ModelState.IsValid)
        {
            await KlinikleriDoldur();
            return View(model);
        }
        mevcut.Ad = model.Ad;
        mevcut.KlinikId = model.KlinikId;
        mevcut.SureDakika = model.SureDakika;
        mevcut.Ucret = model.Ucret;
        await _hizmetler.GuncelleAsync(mevcut);
        TempData["Mesaj"] = "Hizmet güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var sonuc = await _hizmetler.SilAsync(id);
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Index));
    }

    private async Task KlinikleriDoldur()
    {
        ViewBag.Klinikler = new SelectList(await _klinikler.ListeleAsync(), "Id", "Ad");
    }
}
