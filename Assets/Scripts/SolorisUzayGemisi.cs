using UnityEngine;

public class SolorisUzayGemisi : Gemi
{
    private  static SolorisUzayGemisi solorisGemi;

    private static GemiData solorisGemiData;
    private SolorisUzayGemisi(GemiData gemiverisi):base(gemiverisi)
    {

    }

    public static SolorisUzayGemisi GemiInitilaze()
    {
        if(solorisGemi == null)
        {   GemiData gemiVerisi = new GemiData(GemiTuru.Soloris,15f,210f,900f,900f,20,100);
            solorisGemi = new SolorisUzayGemisi(gemiVerisi);
        }
        solorisGemiData = solorisGemi.getGemiVerileri();

        return solorisGemi;
    }


    public override void AtesEt(float hasar)
    {

    }

    public override void HasarAl(float hasar)
    {
        solorisGemi.getGemiVerileri().HasarUygulama(hasar);

    }

    public override void Iyiles()
    {
        solorisGemi.getGemiVerileri().TamirEt(solorisGemiData.GetTamirCani());
    }

}
