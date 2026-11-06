using System;
using UnityEngine;

public class GemiYonetici : MonoBehaviour
{
    private Gemi aktifGemi;

    [SerializeField]
    private OyuncuPanelUi oyuncuPanelUı;

    [SerializeField]
    private GemiHareket _gemiHareket;

    [SerializeField]
    private OyunYonetici _oyunYonetici;



    private void Awake()
    {
        aktifGemi = SolorisUzayGemisi.GemiInitilaze();
        _gemiHareket.GemiAta(aktifGemi);
         GemiData gemiData = aktifGemi.getGemiVerileri();
        oyuncuPanelUı.CanGuncelle(gemiData.GetAnlıkCan(),gemiData.GetmaxsimumCan());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Gemi GetAktifGemiyi()
    {
        return aktifGemi;
    }


    private void CanArayuzunuGuncelle()
    {
        if (aktifGemi == null || oyuncuPanelUı == null)
            return;

        GemiData gemiData = aktifGemi.getGemiVerileri();

        oyuncuPanelUı.CanGuncelle(
            gemiData.GetAnlıkCan(),
            gemiData.GetmaxsimumCan()
        );
    }
    public void HasarVer(float hasarMiktari)
    {
        if (aktifGemi == null)
            return;

        aktifGemi.HasarAl(hasarMiktari);

        GemiDurumunuKontrolEt();
    }

    public void GemiDurumunuKontrolEt()
    {
        CanArayuzunuGuncelle();

        GemiData gemiData = aktifGemi.getGemiVerileri();

        if (gemiData.GetAnlıkCan() <= 0)
        {
            _oyunYonetici.GemiPatladi();
        }
    }
}
