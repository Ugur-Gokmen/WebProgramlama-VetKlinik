namespace VetKlinik.Services;

public class FotoServisi
{
    private static readonly string[] IzinliUzantilar = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaksBayt = 2 * 1024 * 1024;

    public async Task<(bool Tamam, string Mesaj, string? Yol)> KaydetAsync(IWebHostEnvironment env, IFormFile? dosya)
    {
        if (dosya == null || dosya.Length == 0)
            return (true, "", null);

        if (dosya.Length > MaksBayt)
            return (false, "Fotoğraf en fazla 2 MB olabilir.", null);

        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        if (!IzinliUzantilar.Contains(uzanti))
            return (false, "Yalnızca jpg, png veya webp yükleyebilirsiniz.", null);

        if (!await GecerliGorselIcerigiMiAsync(dosya, uzanti))
            return (false, "Dosya içeriği geçerli bir jpg, png veya webp görseli değil.", null);

        var kok = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var klasor = Path.Combine(kok, "uploads", "hayvanlar");
        Directory.CreateDirectory(klasor);

        var ad = Guid.NewGuid().ToString("N") + uzanti;
        var tamYol = Path.Combine(klasor, ad);
        await using var akis = File.Create(tamYol);
        await dosya.CopyToAsync(akis);
        return (true, "", "/uploads/hayvanlar/" + ad);
    }

    public void Sil(IWebHostEnvironment env, string? goreliYol)
    {
        if (string.IsNullOrWhiteSpace(goreliYol) || !goreliYol.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        var kok = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var tamYol = Path.Combine(kok, goreliYol.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(tamYol))
            File.Delete(tamYol);
    }

    private static async Task<bool> GecerliGorselIcerigiMiAsync(IFormFile dosya, string uzanti)
    {
        var baslik = new byte[12];
        await using var akis = dosya.OpenReadStream();
        var okunan = await akis.ReadAsync(baslik);

        return uzanti switch
        {
            ".jpg" or ".jpeg" => okunan >= 3 && baslik[0] == 0xFF && baslik[1] == 0xD8 && baslik[2] == 0xFF,
            ".png" => okunan >= 4 && baslik[0] == 0x89 && baslik[1] == 0x50 && baslik[2] == 0x4E && baslik[3] == 0x47,
            ".webp" => okunan >= 12
                && baslik[0] == 'R' && baslik[1] == 'I' && baslik[2] == 'F' && baslik[3] == 'F'
                && baslik[8] == 'W' && baslik[9] == 'E' && baslik[10] == 'B' && baslik[11] == 'P',
            _ => false
        };
    }
}
