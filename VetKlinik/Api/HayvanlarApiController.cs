using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Dtos;
using VetKlinik.Services;

namespace VetKlinik.Api;

[ApiController]
[Route("api/hayvanlar")]
// IdentityConstants.ApplicationScheme sabit olmadığı için string karşılığı kullanıldı.
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SemaAdi + ",Identity.Application")]
public class HayvanlarApiController : ControllerBase
{
    private readonly AsiServisi _asilar;
    private readonly HayvanServisi _hayvanlar;

    public HayvanlarApiController(AsiServisi asilar, HayvanServisi hayvanlar)
    {
        _asilar = asilar;
        _hayvanlar = hayvanlar;
    }

    [HttpGet("{id:int}/asi-gecmisi")]
    public async Task<ActionResult<AsiTakvimDto>> AsiGecmisi(int id)
    {
        var sahipId = User.IdAl();
        var takvim = await _asilar.TakvimAsync(id, sahipId);
        if (takvim == null)
            return NotFound();

        var hayvan = await _hayvanlar.GetirAsync(id, sahipId);
        return Ok(new AsiTakvimDto
        {
            HayvanId = id,
            HayvanAdi = hayvan!.Ad,
            Gecmis = takvim.Value.Gecmis.Select(a => new AsiKayitDto
            {
                AsiAdi = a.AsiAdi,
                YapildigiTarih = a.YapildigiTarih,
                SonrakiDozTarihi = a.SonrakiDozTarihi
            }).ToList(),
            Yaklasan = takvim.Value.Yaklasan.Select(a => new AsiKayitDto
            {
                AsiAdi = a.AsiAdi,
                YapildigiTarih = a.YapildigiTarih,
                SonrakiDozTarihi = a.SonrakiDozTarihi
            }).ToList()
        });
    }
}
