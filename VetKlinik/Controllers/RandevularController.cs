using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Auth;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers;

[Authorize(Roles = "Uye,Admin")]
public class RandevularController : Controller
{
    private readonly RandevuServisi _randevular;
    private readonly HayvanServisi _hayvanlar;
    private readonly HizmetServisi _hizmetler;
    private readonly VeterinerServisi _veterinerler;

    public RandevularController(RandevuServisi randevular, HayvanServisi hayvanlar, HizmetServisi hizmetler, VeterinerServisi veterinerler)
    {
        _randevular = randevular;
        _hayvanlar = hayvanlar;
        _hizmetler = hizmetler;
        _veterinerler = veterinerler;
    }

    public async Task<IActionResult> Index()
    {
        var liste = await _randevular.UyeRandevulariAsync(User.IdAl());
        return View(liste);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var randevu = await _randevular.UyeRandevuGetirAsync(id, User.IdAl());
        if (randevu == null)
            return NotFound();
        return View(randevu);
    }

    [HttpGet]
    public async Task<IActionResult> Olustur()
    {
        return View(await FormDoldur(new RandevuOlusturViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(RandevuOlusturViewModel model)
    {
        await FormDoldur(model);
        if (!ModelState.IsValid)
            return View(model);

        var sonuc = await _randevular.OlusturAsync(User.IdAl(), model.EvcilHayvanId, model.HizmetId, model.VeterinerId, model.Baslangic);
        if (!sonuc.Basarili)
        {
            ModelState.AddModelError("", sonuc.Mesaj);
            return View(model);
        }

        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Detay), new { id = sonuc.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Slotlar(int hizmetId, DateTime tarih, int? veterinerId)
    {
        var slotlar = await _randevular.UygunSlotlariGetirAsync(hizmetId, tarih, veterinerId);
        return Json(slotlar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Iptal(int id)
    {
        var sonuc = await _randevular.IptalEtAsync(id, User.IdAl());
        if (!sonuc.Basarili && sonuc.Mesaj.Contains("bulunamadı"))
            return NotFound();
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Index));
    }

    private async Task<RandevuOlusturViewModel> FormDoldur(RandevuOlusturViewModel model)
    {
        var hayvanlar = await _hayvanlar.ListeleAsync(User.IdAl());
        var hizmetler = await _hizmetler.ListeleAsync();
        var hekimler = await _veterinerler.ListeleAsync();
        model.Hayvanlar = new SelectList(hayvanlar, "Id", "Ad", model.EvcilHayvanId);
        model.Hizmetler = new SelectList(hizmetler, "Id", "Ad", model.HizmetId);
        model.Veterinerler = new SelectList(hekimler, "Id", "AdSoyad", model.VeterinerId);
        return model;
    }
}
