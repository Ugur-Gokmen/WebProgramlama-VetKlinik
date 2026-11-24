namespace VetKlinik.Dtos;

public class AsiTakvimDto
{
    public int HayvanId { get; set; }
    public string HayvanAdi { get; set; } = "";
    public List<AsiKayitDto> Gecmis { get; set; } = new();
    public List<AsiKayitDto> Yaklasan { get; set; } = new();
}

public class AsiKayitDto
{
    public string AsiAdi { get; set; } = "";
    public DateTime YapildigiTarih { get; set; }
    public DateTime? SonrakiDozTarihi { get; set; }
}
