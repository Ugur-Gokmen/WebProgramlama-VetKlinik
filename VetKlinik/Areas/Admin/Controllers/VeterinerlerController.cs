using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VeterinerlerController : Controller
{
    private readonly VeterinerServisi _veterinerler;
    private readonly KlinikServisi _klinikler;
    private readonly HizmetServisi _hizmetler;

    public VeterinerlerController(VeterinerServisi veterinerler, KlinikServisi klinikler, HizmetServisi hizmetler)
    {
        _veterinerler = veterinerler;
        _klinikler = klinikler;
        _hizmetler = hizmetler;
    }

    public async Task<IActionResult> Index()
        => View(await _veterinerler.ListeleAsync());

    public async Task<IActionResult> Detay(int id)
    {
        var v = await _veterinerler.GetirAsync(id);
        return v == null ? NotFound() : View(v);
    }

    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        await ListeleriDoldur();
        return View(BosForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(VeterinerFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Sifre))
            ModelState.AddModelError(nameof(model.Sifre), "Yeni hekim için şifre zorunlu.");
        if (!ModelState.IsValid)
        {
            await ListeleriDoldur();
            return View(model);
        }

        var sonuc = await _veterinerler.OlusturAsync(
            model.AdSoyad,
            model.Eposta,
            model.Sifre!,
            model.KlinikId,
            model.SecilenHizmetler,
            model.SecilenUzmanliklar,
            model.Musaitlikler);
        if (!sonuc.Basarili)
        {
            ModelState.AddModelError("", sonuc.Mesaj);
            await ListeleriDoldur();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var v = await _veterinerler.GetirAsync(id);
        if (v == null)
            return NotFound();
        await ListeleriDoldur();
        return View(new VeterinerFormViewModel
        {
            Id = v.Id,
            AdSoyad = v.AdSoyad,
            Eposta = v.Kullanici.Email ?? "",
            KlinikId = v.KlinikId,
            SecilenHizmetler = v.Hizmetler.Select(h => h.HizmetId).ToList(),
            SecilenUzmanliklar = v.Uzmanliklar.Select(u => u.UzmanlikId).ToList(),
            Musaitlikler = v.Musaitlikler.OrderBy(m => m.Gun).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, VeterinerFormViewModel model)
    {
        var v = await _veterinerler.GetirAsync(id);
        if (v == null)
            return NotFound();
        ModelState.Remove(nameof(model.Sifre));
        ModelState.Remove(nameof(model.Eposta));
        if (!ModelState.IsValid)
        {
            await ListeleriDoldur();
            return View(model);
        }

        v.AdSoyad = model.AdSoyad;
        v.KlinikId = model.KlinikId;
        await _veterinerler.GuncelleIliskilerAsync(v, model.SecilenHizmetler, model.SecilenUzmanliklar, model.Musaitlikler);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var sonuc = await _veterinerler.SilAsync(id);
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Index));
    }

    private static VeterinerFormViewModel BosForm()
    {
        var model = new VeterinerFormViewModel();
        foreach (DayOfWeek gun in Enum.GetValues<DayOfWeek>())
        {
            model.Musaitlikler.Add(new VeterinerMusaitlik
            {
                Gun = gun,
                Musait = gun != DayOfWeek.Sunday,
                Baslangic = new TimeSpan(9, 0, 0),
                Bitis = new TimeSpan(17, 0, 0)
            });
        }
        return model;
    }

    private async Task ListeleriDoldur()
    {
        ViewBag.Klinikler = new SelectList(await _klinikler.ListeleAsync(), "Id", "Ad");
        ViewBag.Hizmetler = await _hizmetler.ListeleAsync();
        ViewBag.Uzmanliklar = await _veterinerler.UzmanliklariGetirAsync();
    }
}
