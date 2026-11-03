
using UnityEngine;

public class NPCEylemYoneticisi : MonoBehaviour
{
    [Header("Oyun Bilesenleri")]
    [SerializeField] private GemiYonetici gemiYonetici;
    [SerializeField] private OyunYonetici oyunYonetici;

    public bool EylemiGerceklestir(string eylem)
    {
        if (string.IsNullOrWhiteSpace(eylem))
            return false;

        if (oyunYonetici == null || gemiYonetici == null)
        {
            Debug.LogError(
                "NPC Eylem Yoneticisi Inspector baglantilari eksik!"
            );
            return false;
        }

        if (oyunYonetici.OyunBittiMi)
        {
            Debug.LogWarning(
                "Oyun bittigi icin NPC eylemi uygulanamadi."
            );
            return false;
        }

        switch (eylem.Trim().ToUpperInvariant())
        {
            case "NONE":
                Debug.Log(
                    "NPC herhangi bir oyun eylemi gerceklestirmedi."
                );
                return true;

            case "HEAL":
                return GemiyiIyilestir();

            case "MAX_HEAL":
                return MaksimumCanYukselt();

            case "BOOST_DAMAGE":
                Debug.LogWarning(
                    "BOOST_DAMAGE henuz uygulanmadi."
                );
                return false;

            case "BOSS_HINT":
                Debug.Log(
                    "Boss tavsiyesi oyuncuya sunuldu."
                );
                return true;

            case "UNLOCK_DOOR":
                Debug.LogWarning(
                    "UNLOCK_DOOR henuz uygulanmadi."
                );
                return false;

            default:
                Debug.LogWarning(
                    "Bilinmeyen NPC eylemi: " + eylem
                );
                return false;
        }
    }

    private GemiData AktifGemiVerisiniAl()
    {
        Gemi aktifGemi = gemiYonetici.GetAktifGemiyi();

        if (aktifGemi == null)
        {
            Debug.LogError("Aktif gemi bulunamadi!");
            return null;
        }

        GemiData gemiData = aktifGemi.getGemiVerileri();

        if (gemiData == null)
        {
            Debug.LogError("Gemi verileri bulunamadi!");
            return null;
        }

        return gemiData;
    }

    // NORMAL IYILESTIRME
    // Mevcut maksimum can kapasitesini degistirmez.
    private bool GemiyiIyilestir()
    {
        GemiData gemiData = AktifGemiVerisiniAl();

        if (gemiData == null)
            return false;

        float mevcutCan = gemiData.GetAnlıkCan();
        float maksimumCan = gemiData.GetmaxsimumCan();

        if (mevcutCan >= maksimumCan)
        {
            Debug.Log("Gemi zaten maksimum cana sahip.");
            return true;
        }

        float eksikCan = maksimumCan - mevcutCan;

        gemiData.TamirEt(eksikCan);

        gemiYonetici.GemiDurumunuKontrolEt();

        Debug.Log(
            "HEAL BASARILI: "
            + gemiData.GetAnlıkCan()
            + " / "
            + gemiData.GetmaxsimumCan()
            + " HP"
        );

        return true;
    }

    // MAKSIMUM CAN YUKSELTME
    // Mevcut cani ve maksimum cani 1500 yapar.
    private bool MaksimumCanYukselt()
    {
        GemiData gemiData = AktifGemiVerisiniAl();

        if (gemiData == null)
            return false;

        gemiData.SetCan(1500f);

        gemiYonetici.GemiDurumunuKontrolEt();

        Debug.Log(
            "MAX_HEAL BASARILI: "
            + gemiData.GetAnlıkCan()
            + " / "
            + gemiData.GetmaxsimumCan()
            + " HP"
        );

        return true;
    }
}
