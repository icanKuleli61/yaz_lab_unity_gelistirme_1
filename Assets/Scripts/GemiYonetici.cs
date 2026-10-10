using System;
using UnityEngine;

public class GemiYonetici : MonoBehaviour
{
    private Gemi aktifGemi;

    [SerializeField]
    private OyuncuPanelUi oyuncuPanelUı;

    [SerializeField]
    private GemiHareket _gemiHareket;



    void Start()
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
}
