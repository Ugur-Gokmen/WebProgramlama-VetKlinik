namespace VetKlinik.Models;

public class VeterinerUzmanlik
{
    public int VeterinerId { get; set; }
    public Veteriner Veteriner { get; set; } = null!;

    public int UzmanlikId { get; set; }
    public Uzmanlik Uzmanlik { get; set; } = null!;
}
