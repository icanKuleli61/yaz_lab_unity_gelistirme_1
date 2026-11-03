
using UnityEngine;

public class NPCEylemYoneticisi : MonoBehaviour
{
    [Header("Oyun Bilesenleri")]
    [SerializeField] private GemiYonetici gemiYonetici;
    [SerializeField] private OyunYonetici oyunYonetici;

    [Header("Silah Guclendirme")]
    private const float GuclendirilmisHasar = 100f;

    // Eski metodu kullanan baska kodlar varsa calismaya devam eder.
    public bool EylemiGerceklestir(string eylem)
    {
        NPCIslemSonucu sonuc =
            EylemiGerceklestirSonucuyla(eylem);

        return sonuc != null && sonuc.success;
    }

    // Eylemi uygular ve oyuncuya gosterilecek metin yerine
    // Unity tarafindaki gercek oyun verilerini dondurur.
    public NPCIslemSonucu EylemiGerceklestirSonucuyla(
        string eylem
    )
    {
        if (string.IsNullOrWhiteSpace(eylem))
        {
            return YeniSonuc(
                "",
                false,
                "INVALID_ACTION"
            );
        }

        string secilenEylem =
            eylem.Trim().ToUpperInvariant();

        // Normal sohbet oyun bilesenlerine bagli degildir.
        if (secilenEylem == "NONE")
        {
            return YeniSonuc(
                secilenEylem,
                true,
                "NO_GAME_ACTION"
            );
        }

        // Boss tavsiyesi oyun durumunu degistirmez.
        if (secilenEylem == "BOSS_HINT")
        {
            return YeniSonuc(
                secilenEylem,
                true,
                "INFORMATION_RESPONSE"
            );
        }

        // Kapi mekanizmasi henuz tamamlanmadi.
        if (secilenEylem == "UNLOCK_DOOR")
        {
            Debug.LogWarning(
                "UNLOCK_DOOR henuz uygulanmadi."
            );

            return YeniSonuc(
                secilenEylem,
                false,
                "NOT_IMPLEMENTED"
            );
        }

        // Yalnizca gercekten desteklenen eylemler.
        if (secilenEylem != "HEAL" &&
            secilenEylem != "MAX_HEAL" &&
            secilenEylem != "BOOST_DAMAGE")
        {
            Debug.LogWarning(
                "Bilinmeyen NPC eylemi: " + eylem
            );

            return YeniSonuc(
                secilenEylem,
                false,
                "UNKNOWN_ACTION"
            );
        }

        // Oyun bilesenleri kontrolu.
        if (gemiYonetici == null)
        {
            Debug.LogWarning(
                "GemiYonetici Inspector baglantisi eksik."
            );

            return YeniSonuc(
                secilenEylem,
                false,
                "SHIP_MANAGER_MISSING"
            );
        }

        if (oyunYonetici == null)
        {
            Debug.LogWarning(
                "OyunYonetici Inspector baglantisi eksik."
            );

            return YeniSonuc(
                secilenEylem,
                false,
                "GAME_MANAGER_MISSING"
            );
        }

        if (oyunYonetici.OyunBittiMi)
        {
            Debug.LogWarning(
                "Oyun bittigi icin NPC eylemi uygulanamadi."
            );

            return YeniSonuc(
                secilenEylem,
                false,
                "GAME_FINISHED"
            );
        }

        string veriHatasi;

        GemiData gemiData =
            AktifGemiVerisiniAl(out veriHatasi);

        if (gemiData == null)
        {
            return YeniSonuc(
                secilenEylem,
                false,
                veriHatasi
            );
        }

        switch (secilenEylem)
        {
            case "HEAL":
                return GemiyiIyilestir(gemiData);

            case "MAX_HEAL":
                return MaksimumCanYukselt(gemiData);

            case "BOOST_DAMAGE":
                return SilahiGuclendir(gemiData);

            default:
                return YeniSonuc(
                    secilenEylem,
                    false,
                    "UNKNOWN_ACTION"
                );
        }
    }

    // --------------------------------------------------
    // AKTIF GEMI VERISINI AL
    // --------------------------------------------------

    private GemiData AktifGemiVerisiniAl(
        out string hataKodu
    )
    {
        hataKodu = "";

        if (gemiYonetici == null)
        {
            hataKodu = "SHIP_MANAGER_MISSING";
            return null;
        }

        Gemi aktifGemi =
            gemiYonetici.GetAktifGemiyi();

        if (aktifGemi == null)
        {
            Debug.LogWarning(
                "Aktif gemi bulunamadi."
            );

            hataKodu = "SHIP_NOT_FOUND";
            return null;
        }

        GemiData gemiData =
            aktifGemi.getGemiVerileri();

        if (gemiData == null)
        {
            Debug.LogWarning(
                "Aktif geminin verileri bulunamadi."
            );

            hataKodu = "SHIP_DATA_MISSING";
            return null;
        }

        return gemiData;
    }

    // --------------------------------------------------
    // NORMAL IYILESTIRME
    // Mevcut cani maksimum cana tamamlar.
    // Maksimum can kapasitesini degistirmez.
    // --------------------------------------------------

    private NPCIslemSonucu GemiyiIyilestir(
        GemiData gemiData
    )
    {
        float oncekiCan =
            gemiData.GetAnlıkCan();

        float maksimumCan =
            gemiData.GetmaxsimumCan();

        if (oncekiCan < maksimumCan)
        {
            float eksikCan =
                maksimumCan - oncekiCan;

            gemiData.TamirEt(eksikCan);
        }

        gemiYonetici.GemiDurumunuKontrolEt();

        float guncelCan =
            gemiData.GetAnlıkCan();

        float guncelMaksimumCan =
            gemiData.GetmaxsimumCan();

        string reasonCode =
            oncekiCan >= maksimumCan
                ? "ALREADY_FULL"
                : "HEALED";

        Debug.Log(
            "HEAL SONUCU: "
            + oncekiCan
            + " -> "
            + guncelCan
            + " / "
            + guncelMaksimumCan
            + " HP"
        );

        NPCIslemSonucu sonuc =
            YeniSonuc(
                "HEAL",
                true,
                reasonCode
            );

        sonuc.previousHp = oncekiCan;
        sonuc.currentHp = guncelCan;
        sonuc.maxHp = guncelMaksimumCan;

        return sonuc;
    }

    // --------------------------------------------------
    // MAKSIMUM CAN YUKSELTME
    // Mevcut ve maksimum cani 1500 yapar.
    // --------------------------------------------------

    private NPCIslemSonucu MaksimumCanYukselt(
        GemiData gemiData
    )
    {
        float oncekiCan =
            gemiData.GetAnlıkCan();

        gemiData.SetCan(1500f);

        gemiYonetici.GemiDurumunuKontrolEt();

        float guncelCan =
            gemiData.GetAnlıkCan();

        float guncelMaksimumCan =
            gemiData.GetmaxsimumCan();

        Debug.Log(
            "MAX_HEAL SONUCU: "
            + oncekiCan
            + " -> "
            + guncelCan
            + " / "
            + guncelMaksimumCan
            + " HP"
        );

        NPCIslemSonucu sonuc =
            YeniSonuc(
                "MAX_HEAL",
                true,
                "MAX_HP_UPGRADED"
            );

        sonuc.previousHp = oncekiCan;
        sonuc.currentHp = guncelCan;
        sonuc.maxHp = guncelMaksimumCan;

        return sonuc;
    }

    // --------------------------------------------------
    // SILAH GUCLENDIRME
    // Normal hasar: 30
    // Guclendirilmis hasar: 100
    // --------------------------------------------------

    private NPCIslemSonucu SilahiGuclendir(
        GemiData gemiData
    )
    {
        float oncekiHasar =
            gemiData.GetHasar();

        if (oncekiHasar < GuclendirilmisHasar)
        {
            gemiData.SetHasar(
                GuclendirilmisHasar
            );
        }

        float guncelHasar =
            gemiData.GetHasar();

        string reasonCode =
            oncekiHasar >= GuclendirilmisHasar
                ? "ALREADY_UPGRADED"
                : "DAMAGE_UPGRADED";

        Debug.Log(
            "BOOST_DAMAGE SONUCU: "
            + oncekiHasar
            + " -> "
            + guncelHasar
        );

        NPCIslemSonucu sonuc =
            YeniSonuc(
                "BOOST_DAMAGE",
                true,
                reasonCode
            );

        sonuc.previousDamage = oncekiHasar;
        sonuc.currentDamage = guncelHasar;

        return sonuc;
    }

    // --------------------------------------------------
    // SONUC NESNESI OLUSTUR
    // --------------------------------------------------

    private NPCIslemSonucu YeniSonuc(
        string eylem,
        bool basarili,
        string reasonCode
    )
    {
        return new NPCIslemSonucu
        {
            action = eylem,
            success = basarili,
            reasonCode = reasonCode
        };
    }
}
