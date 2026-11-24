using VetKlinik.Models;
using VetKlinik.Services;

namespace VetKlinik.Tests;

public class CakismaHesabiTests
{
    [Fact]
    public void Tam_bitiste_baslayan_randevu_cakismaz()
    {
        var mevcutBas = new DateTime(2026, 12, 1, 10, 0, 0);
        var mevcutBit = new DateTime(2026, 12, 1, 10, 30, 0);
        var yeniBas = new DateTime(2026, 12, 1, 10, 30, 0);
        var yeniBit = new DateTime(2026, 12, 1, 11, 0, 0);

        Assert.False(RandevuServisi.CakisiyorMu(yeniBas, yeniBit, mevcutBas, mevcutBit));
    }

    [Fact]
    public void Hizmetin_ortasina_denk_gelen_cakisir()
    {
        var mevcutBas = new DateTime(2026, 12, 1, 10, 0, 0);
        var mevcutBit = new DateTime(2026, 12, 1, 11, 0, 0);
        var yeniBas = new DateTime(2026, 12, 1, 10, 30, 0);
        var yeniBit = new DateTime(2026, 12, 1, 11, 0, 0);

        Assert.True(RandevuServisi.CakisiyorMu(yeniBas, yeniBit, mevcutBas, mevcutBit));
    }

    [Fact]
    public void Reddedilen_durum_bloklamaz()
    {
        Assert.False(RandevuServisi.BloklayanDurum(RandevuDurumu.Reddedildi));
        Assert.False(RandevuServisi.BloklayanDurum(RandevuDurumu.Iptal));
        Assert.True(RandevuServisi.BloklayanDurum(RandevuDurumu.Bekliyor));
        Assert.True(RandevuServisi.BloklayanDurum(RandevuDurumu.Onaylandi));
    }
}
