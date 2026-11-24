using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VetKlinik.Auth;
using VetKlinik.Dtos;
using VetKlinik.Services;

namespace VetKlinik.Api;

[ApiController]
[Route("api/raporlar")]
[Authorize(Roles = "Admin", AuthenticationSchemes = ApiKeyAuthenticationHandler.SemaAdi + ",Identity.Application")]
public class RaporlarApiController : ControllerBase
{
    private readonly RaporServisi _raporlar;

    public RaporlarApiController(RaporServisi raporlar)
    {
        _raporlar = raporlar;
    }

    [HttpGet("hizmet-ozeti")]
    public async Task<ActionResult<List<HizmetOzetDto>>> HizmetOzeti()
        => Ok(await _raporlar.HizmetOzetiAsync());
}
