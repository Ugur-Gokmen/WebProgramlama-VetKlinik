using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Models;
using VetKlinik.ViewModels;

namespace VetKlinik.Controllers;

public class HesapController : Controller
{
    private readonly SignInManager<UygulamaKullanicisi> _giris;
    private readonly UserManager<UygulamaKullanicisi> _kullanicilar;

    public HesapController(SignInManager<UygulamaKullanicisi> giris, UserManager<UygulamaKullanicisi> kullanicilar)
    {
        _giris = giris;
        _kullanicilar = kullanicilar;
    }

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new GirisViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var sonuc = await _giris.PasswordSignInAsync(model.Eposta, model.Sifre, model.BeniHatirla, false);
        if (!sonuc.Succeeded)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Kayit() => View(new KayitViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new UygulamaKullanicisi
        {
            UserName = model.Eposta,
            Email = model.Eposta,
            AdSoyad = model.AdSoyad
        };
        var sonuc = await _kullanicilar.CreateAsync(user, model.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            return View(model);
        }

        await _kullanicilar.AddToRoleAsync(user, "Uye");
        await _giris.SignInAsync(user, false);
        return RedirectToAction("Index", "Hayvanlar");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _giris.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ErisimYok() => View();
}
