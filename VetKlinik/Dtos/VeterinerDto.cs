namespace VetKlinik.Dtos;

public class VeterinerDto
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = "";
    public string KlinikAdi { get; set; } = "";
    public List<string> Uzmanliklar { get; set; } = new();
    public List<string> Hizmetler { get; set; } = new();
}
