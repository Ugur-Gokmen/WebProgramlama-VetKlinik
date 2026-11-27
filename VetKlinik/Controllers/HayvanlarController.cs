using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers;

[Authorize(Roles = "Uye,Admin")]
public class HayvanlarController : Controller
{
    private readonly HayvanServisi _hayvanlar;
    private readonly FotoServisi _foto;
    private readonly IWebHostEnvironment _env;
    private readonly TimeProvider _saat;

    public HayvanlarController(HayvanServisi hayvanlar, FotoServisi foto, IWebHostEnvironment env, TimeProvider saat)
    {
        _hayvanlar = hayvanlar;
        _foto = foto;
        _env = env;
        _saat = saat;
    }

    public async Task<IActionResult> Index()
    {
        var liste = await _hayvanlar.ListeleAsync(User.IdAl());
        return View(liste);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var hayvan = await _hayvanlar.GetirAsync(id, User.IdAl());
        if (hayvan == null)
            return NotFound();
        return View(hayvan);
    }

    [HttpGet]
    public IActionResult Ekle() => View(new HayvanFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(HayvanFormViewModel model)
    {
        if (model.DogumTarihi.Date > _saat.GetLocalNow().Date)
            ModelState.AddModelError(nameof(model.DogumTarihi), "Doğum tarihi bugünden ileri olamaz.");
        if (!ModelState.IsValid)
            return View(model);

        var foto = await _foto.KaydetAsync(_env, model.Foto);
        if (!foto.Tamam)
        {
            ModelState.AddModelError(nameof(model.Foto), foto.Mesaj);
            return View(model);
        }

        await _hayvanlar.EkleAsync(new EvcilHayvan
        {
            SahipId = User.IdAl(),
            Ad = model.Ad,
            Tur = model.Tur,
            Irk = model.Irk,
            DogumTarihi = model.DogumTarihi,
            Kilo = model.Kilo,
            FotoYolu = foto.Yol
        });
        TempData["Mesaj"] = "Evcil hayvan eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var hayvan = await _hayvanlar.GetirAsync(id, User.IdAl());
        if (hayvan == null)
            return NotFound();
        return View(new HayvanFormViewModel
        {
            Id = hayvan.Id,
            Ad = hayvan.Ad,
            Tur = hayvan.Tur,
            Irk = hayvan.Irk,
            DogumTarihi = hayvan.DogumTarihi,
            Kilo = hayvan.Kilo,
            MevcutFoto = hayvan.FotoYolu
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, HayvanFormViewModel model)
    {
        if (model.Id != 0 && id != model.Id)
            return NotFound();
        var hayvan = await _hayvanlar.GetirAsync(id, User.IdAl());
        if (hayvan == null)
            return NotFound();
        if (model.DogumTarihi.Date > _saat.GetLocalNow().Date)
            ModelState.AddModelError(nameof(model.DogumTarihi), "Doğum tarihi bugünden ileri olamaz.");
        if (!ModelState.IsValid)
        {
            model.MevcutFoto = hayvan.FotoYolu;
            return View(model);
        }

        var foto = await _foto.KaydetAsync(_env, model.Foto);
        if (!foto.Tamam)
        {
            ModelState.AddModelError(nameof(model.Foto), foto.Mesaj);
            model.MevcutFoto = hayvan.FotoYolu;
            return View(model);
        }

        hayvan.Ad = model.Ad;
        hayvan.Tur = model.Tur;
        hayvan.Irk = model.Irk;
        hayvan.DogumTarihi = model.DogumTarihi;
        hayvan.Kilo = model.Kilo;
        if (foto.Yol != null)
        {
            _foto.Sil(_env, hayvan.FotoYolu);
            hayvan.FotoYolu = foto.Yol;
        }

        await _hayvanlar.GuncelleAsync(hayvan);
        TempData["Mesaj"] = "Evcil hayvan güncellendi.";
        return RedirectToAction(nameof(Detay), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var hayvan = await _hayvanlar.GetirAsync(id, User.IdAl());
        if (hayvan == null)
            return NotFound();
        var fotoYolu = hayvan.FotoYolu;
        var silindi = await _hayvanlar.SilAsync(id, User.IdAl());
        if (!silindi)
            return NotFound();
        _foto.Sil(_env, fotoYolu);
        TempData["Mesaj"] = "Evcil hayvan silindi.";
        return RedirectToAction(nameof(Index));
    }
}
