namespace VetKlinik.Models;

public class VeterinerHizmet
{
    public int VeterinerId { get; set; }
    public Veteriner Veteriner { get; set; } = null!;

    public int HizmetId { get; set; }
    public Hizmet Hizmet { get; set; } = null!;
}
