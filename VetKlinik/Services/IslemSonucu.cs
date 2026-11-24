namespace VetKlinik.Services;

// Servis katmanının controller/MCP'ye döndürdüğü sade sonuç.
public class IslemSonucu
{
    public bool Basarili { get; init; }
    public string Mesaj { get; init; } = "";
    public int? Id { get; init; }

    public static IslemSonucu Tamam(string mesaj = "İşlem başarılı.", int? id = null)
        => new() { Basarili = true, Mesaj = mesaj, Id = id };

    public static IslemSonucu Hata(string mesaj)
        => new() { Basarili = false, Mesaj = mesaj };
}
