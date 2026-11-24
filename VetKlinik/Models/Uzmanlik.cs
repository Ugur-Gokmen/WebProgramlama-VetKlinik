using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Models;

public class Uzmanlik
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Ad { get; set; } = "";

    public ICollection<VeterinerUzmanlik> VeterinerUzmanliklari { get; set; } = new List<VeterinerUzmanlik>();
}
