using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers;

[Authorize(Roles = "Uye,Admin")]
public class AsilarController : Controller
{
    private readonly AsiServisi _asilar;
    private readonly HayvanServisi _hayvanlar;

    public AsilarController(AsiServisi asilar, HayvanServisi hayvanlar)
    {
        _asilar = asilar;
        _hayvanlar = hayvanlar;
    }

    [HttpGet]
    public async Task<IActionResult> Ekle(int hayvanId)
    {
        var hayvan = await _hayvanlar.GetirAsync(hayvanId, User.IdAl());
        if (hayvan == null)
            return NotFound();
        ViewBag.HayvanAd = hayvan.Ad;
        return View(new AsiFormViewModel { EvcilHayvanId = hayvanId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(AsiFormViewModel model)
    {
        var hayvan = await _hayvanlar.GetirAsync(model.EvcilHayvanId, User.IdAl());
        if (hayvan == null)
            return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.HayvanAd = hayvan.Ad;
            return View(model);
        }

        await _asilar.EkleAsync(new AsiKaydi
        {
            EvcilHayvanId = model.EvcilHayvanId,
            AsiAdi = model.AsiAdi,
            YapildigiTarih = model.YapildigiTarih,
            SonrakiDozTarihi = model.SonrakiDozTarihi
        });
        return RedirectToAction("Detay", "Hayvanlar", new { id = model.EvcilHayvanId });
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var kayit = await _asilar.GetirAsync(id, User.IdAl());
        if (kayit == null)
            return NotFound();
        return View(new AsiFormViewModel
        {
            Id = kayit.Id,
            EvcilHayvanId = kayit.EvcilHayvanId,
            AsiAdi = kayit.AsiAdi,
            YapildigiTarih = kayit.YapildigiTarih,
            SonrakiDozTarihi = kayit.SonrakiDozTarihi
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, AsiFormViewModel model)
    {
        var kayit = await _asilar.GetirAsync(id, User.IdAl());
        if (kayit == null)
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);

        kayit.AsiAdi = model.AsiAdi;
        kayit.YapildigiTarih = model.YapildigiTarih;
        kayit.SonrakiDozTarihi = model.SonrakiDozTarihi;
        await _asilar.GuncelleAsync(kayit);
        return RedirectToAction("Detay", "Hayvanlar", new { id = kayit.EvcilHayvanId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var kayit = await _asilar.GetirAsync(id, User.IdAl());
        if (kayit == null)
            return NotFound();
        var hayvanId = kayit.EvcilHayvanId;
        await _asilar.SilAsync(id, User.IdAl());
        return RedirectToAction("Detay", "Hayvanlar", new { id = hayvanId });
    }
}
