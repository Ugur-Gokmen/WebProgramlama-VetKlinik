using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Dtos;
using VetKlinik.Services;

namespace VetKlinik.Api;

[ApiController]
[Route("api/veterinerler")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SemaAdi + ",Identity.Application")]
public class VeterinerlerApiController : ControllerBase
{
    private readonly VeterinerServisi _veterinerler;
    private readonly RandevuServisi _randevular;

    public VeterinerlerApiController(VeterinerServisi veterinerler, RandevuServisi randevular)
    {
        _veterinerler = veterinerler;
        _randevular = randevular;
    }

    [HttpGet]
    public async Task<ActionResult<List<VeterinerDto>>> TumunuGetir()
    {
        var liste = await _veterinerler.ListeleAsync();
        return Ok(liste.Select(v => new VeterinerDto
        {
            Id = v.Id,
            AdSoyad = v.AdSoyad,
            KlinikAdi = v.Klinik.Ad,
            Uzmanliklar = v.Uzmanliklar.Select(u => u.Uzmanlik.Ad).ToList(),
            Hizmetler = v.Hizmetler.Select(h => h.Hizmet.Ad).ToList()
        }).ToList());
    }

    [HttpGet("uygun")]
    public async Task<ActionResult<List<VeterinerDto>>> UygunGetir(int hizmetId, DateTime tarih)
    {
        if (hizmetId <= 0)
            return BadRequest("hizmetId zorunludur.");

        var hekimler = await _randevular.UygunHekimleriGetirAsync(hizmetId, tarih);
        return Ok(hekimler.Select(v => new VeterinerDto
        {
            Id = v.Id,
            AdSoyad = v.AdSoyad,
            KlinikAdi = v.Klinik?.Ad ?? "",
            Uzmanliklar = v.Uzmanliklar.Select(u => u.Uzmanlik.Ad).ToList(),
            Hizmetler = new List<string>()
        }).ToList());
    }
}
