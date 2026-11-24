namespace VetKlinik.Services;

public class MusaitSlot
{
    public int VeterinerId { get; set; }
    public string VeterinerAdi { get; set; } = "";
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
}
