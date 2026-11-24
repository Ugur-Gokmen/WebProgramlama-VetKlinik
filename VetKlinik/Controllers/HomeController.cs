using Microsoft.AspNetCore.Mvc;
using VetKlinik.Services;

namespace VetKlinik.Controllers;

public class HomeController : Controller
{
    private readonly HizmetServisi _hizmetler;
    private readonly VeterinerServisi _veterinerler;

    public HomeController(HizmetServisi hizmetler, VeterinerServisi veterinerler)
    {
        _hizmetler = hizmetler;
        _veterinerler = veterinerler;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Hizmetler = await _hizmetler.ListeleAsync();
        ViewBag.Veterinerler = await _veterinerler.ListeleAsync();
        return View();
    }
}
