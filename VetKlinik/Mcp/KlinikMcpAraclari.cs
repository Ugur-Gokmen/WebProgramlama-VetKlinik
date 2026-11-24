using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using VetKlinik.Auth;
using VetKlinik.Services;

namespace VetKlinik.Mcp;

[McpServerToolType]
public class KlinikMcpAraclari
{
    private readonly IHttpContextAccessor _http;
    private readonly HizmetServisi _hizmetler;
    private readonly HayvanServisi _hayvanlar;
    private readonly AsiServisi _asilar;
    private readonly RandevuServisi _randevular;

    public KlinikMcpAraclari(
        IHttpContextAccessor http,
        HizmetServisi hizmetler,
        HayvanServisi hayvanlar,
        AsiServisi asilar,
        RandevuServisi randevular)
    {
        _http = http;
        _hizmetler = hizmetler;
        _hayvanlar = hayvanlar;
        _asilar = asilar;
        _randevular = randevular;
    }

    [McpServerTool, Description("Kliniğin sunduğu hizmetleri id, süre ve ücret ile listeler.")]
    public async Task<string> hizmetleri_listele()
    {
        var liste = await _hizmetler.ListeleAsync();
        return JsonSerializer.Serialize(liste.Select(h => new
        {
            h.Id,
            h.Ad,
            h.SureDakika,
            h.Ucret
        }));
    }

    [McpServerTool, Description("Çağıran kullanıcının kayıtlı evcil hayvanlarını listeler.")]
    public async Task<string> evcil_hayvanlarim()
    {
        var sahipId = KullaniciId();
        if (sahipId == null)
            return "Giriş yapılmamış. Authorization: Bearer API_ANAHTARI gönderin.";

        var liste = await _hayvanlar.ListeleAsync(sahipId);
        return JsonSerializer.Serialize(liste.Select(h => new { h.Id, h.Ad, h.Tur, h.Irk, h.Kilo }));
    }

    [McpServerTool, Description("Belirtilen hizmet ve tarihte boş hekim/saat slotlarını listeler.")]
    public async Task<string> uygun_saatler(int hizmetId, string tarih)
    {
        if (!DateTime.TryParse(tarih, out var gun))
            return "Tarih okunamadı. Örnek: 2026-12-01";

        var slotlar = await _randevular.UygunSlotlariGetirAsync(hizmetId, gun);
        if (slotlar.Count == 0)
            return "Bu tarihte uygun slot yok (klinik kapalı, hekim müsait değil veya tüm saatler dolu).";

        return JsonSerializer.Serialize(slotlar.Select(s => new
        {
            s.VeterinerId,
            s.VeterinerAdi,
            Baslangic = s.Baslangic.ToString("yyyy-MM-dd HH:mm"),
            Bitis = s.Bitis.ToString("HH:mm")
        }));
    }

    [McpServerTool, Description("İsimle evcil hayvanın aşı geçmişini ve yaklaşan dozlarını döner. Aynı isimde birden fazla hayvan varsa tahmin etmez.")]
    public async Task<string> asi_takvimi(string hayvanAdi)
    {
        var sahipId = KullaniciId();
        if (sahipId == null)
            return "Giriş yapılmamış.";

        var hayvanlar = await _hayvanlar.AdaGoreGetirAsync(sahipId, hayvanAdi);
        if (hayvanlar.Count == 0)
            return "Bu isimde evcil hayvanınız yok.";
        if (hayvanlar.Count > 1)
        {
            return "Aynı isimde birden fazla hayvan var, tahmin edilmedi: " +
                   JsonSerializer.Serialize(hayvanlar.Select(h => new { h.Id, h.Ad, h.Tur, h.Irk }));
        }

        var takvim = await _asilar.TakvimAsync(hayvanlar[0].Id, sahipId);
        return JsonSerializer.Serialize(new
        {
            Hayvan = hayvanlar[0].Ad,
            Gecmis = takvim!.Value.Gecmis.Select(a => new { a.AsiAdi, a.YapildigiTarih, a.SonrakiDozTarihi }),
            Yaklasan = takvim.Value.Yaklasan.Select(a => new { a.AsiAdi, a.SonrakiDozTarihi })
        });
    }

    [McpServerTool, Description("Bekliyor durumunda randevu oluşturur. Çakışma varsa nedenini Türkçe açıklar.")]
    public async Task<string> randevu_olustur(string hayvanAdi, int hizmetId, int hekimId, string baslangic)
    {
        var sahipId = KullaniciId();
        if (sahipId == null)
            return "Giriş yapılmamış.";

        if (!DateTime.TryParse(baslangic, out var bas))
            return "Başlangıç tarihi okunamadı. Örnek: 2026-12-01 10:00";

        var hayvanlar = await _hayvanlar.AdaGoreGetirAsync(sahipId, hayvanAdi);
        if (hayvanlar.Count == 0)
            return "Bu isimde evcil hayvanınız yok.";
        if (hayvanlar.Count > 1)
        {
            return "Aynı isimde birden fazla hayvan var, tahmin edilmedi: " +
                   JsonSerializer.Serialize(hayvanlar.Select(h => new { h.Id, h.Ad, h.Tur }));
        }

        var sonuc = await _randevular.OlusturAsync(sahipId, hayvanlar[0].Id, hizmetId, hekimId, bas);
        return sonuc.Basarili
            ? $"Randevu oluşturuldu. Id={sonuc.Id}. {sonuc.Mesaj}"
            : sonuc.Mesaj;
    }

    private string? KullaniciId()
    {
        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;
        var id = user.IdAl();
        return string.IsNullOrEmpty(id) ? null : id;
    }
}
