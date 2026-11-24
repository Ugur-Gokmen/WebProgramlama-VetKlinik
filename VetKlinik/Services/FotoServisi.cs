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

        var klasor = Path.Combine(env.WebRootPath, "uploads", "hayvanlar");
        Directory.CreateDirectory(klasor);
        var ad = Guid.NewGuid().ToString("N") + uzanti;
        var tamYol = Path.Combine(klasor, ad);
        await using var akis = File.Create(tamYol);
        await dosya.CopyToAsync(akis);
        return (true, "", "/uploads/hayvanlar/" + ad);
    }
}
