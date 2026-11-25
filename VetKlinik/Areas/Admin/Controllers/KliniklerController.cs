using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Models;
using VetKlinik.Services;
using VetKlinik.ViewModels;

namespace VetKlinik.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class KliniklerController : Controller
{
    private readonly KlinikServisi _klinikler;

    public KliniklerController(KlinikServisi klinikler)
    {
        _klinikler = klinikler;
    }

    public async Task<IActionResult> Index()
        => View(await _klinikler.ListeleAsync());

    public async Task<IActionResult> Detay(int id)
    {
        var klinik = await _klinikler.GetirAsync(id);
        return klinik == null ? NotFound() : View(klinik);
    }

    [HttpGet]
    public IActionResult Ekle()
        => View(BosForm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(KlinikFormViewModel model)
    {
        SaatleriKontrolEt(model);
        if (!ModelState.IsValid)
            return View(model);
        await _klinikler.EkleAsync(ModeleKlinik(model));
        TempData["Mesaj"] = "Klinik eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var klinik = await _klinikler.GetirAsync(id);
        if (klinik == null)
            return NotFound();
        return View(new KlinikFormViewModel
        {
            Id = klinik.Id,
            Ad = klinik.Ad,
            Adres = klinik.Adres,
            Telefon = klinik.Telefon,
            CalismaSaatleri = klinik.CalismaSaatleri.OrderBy(s => s.Gun).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, KlinikFormViewModel model)
    {
        if (model.Id != 0 && id != model.Id)
            return NotFound();
        var klinik = await _klinikler.GetirAsync(id);
        if (klinik == null)
            return NotFound();
        SaatleriKontrolEt(model);
        if (!ModelState.IsValid)
            return View(model);

        klinik.Ad = model.Ad;
        klinik.Adres = model.Adres;
        klinik.Telefon = model.Telefon;
        foreach (var saat in model.CalismaSaatleri)
        {
            var mevcut = klinik.CalismaSaatleri.FirstOrDefault(s => s.Id == saat.Id);
            if (mevcut == null)
                continue;
            mevcut.Acik = saat.Acik;
            mevcut.Acilis = saat.Acilis;
            mevcut.Kapanis = saat.Kapanis;
        }
        await _klinikler.GuncelleAsync(klinik);
        TempData["Mesaj"] = "Klinik güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var sonuc = await _klinikler.SilAsync(id);
        TempData["Mesaj"] = sonuc.Mesaj;
        return RedirectToAction(nameof(Index));
    }

    private void SaatleriKontrolEt(KlinikFormViewModel model)
    {
        foreach (var s in model.CalismaSaatleri.Where(x => x.Acik && x.Acilis >= x.Kapanis))
        {
            ModelState.AddModelError("", $"{s.Gun} günü için açılış saati kapanış saatinden önce olmalıdır.");
        }
    }

    private static KlinikFormViewModel BosForm()
    {
        var model = new KlinikFormViewModel();
        foreach (DayOfWeek gun in Enum.GetValues<DayOfWeek>())
        {
            model.CalismaSaatleri.Add(new KlinikCalismaSaati
            {
                Gun = gun,
                Acik = gun != DayOfWeek.Sunday,
                Acilis = new TimeSpan(9, 0, 0),
                Kapanis = new TimeSpan(18, 0, 0)
            });
        }
        return model;
    }

    private static Klinik ModeleKlinik(KlinikFormViewModel model)
    {
        var klinik = new Klinik { Ad = model.Ad, Adres = model.Adres, Telefon = model.Telefon };
        foreach (var s in model.CalismaSaatleri)
            klinik.CalismaSaatleri.Add(s);
        return klinik;
    }
}
