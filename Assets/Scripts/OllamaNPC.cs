
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

public class OllamaNPC : MonoBehaviour
{
    [Header("Konusma Paneli")]
    public GameObject konusmaPaneli;
    public TMP_InputField oyuncuMesajAlani;
    public TMP_Text npcCevapMetni;

    [Header("Ollama Ayarlari")]
    public string ollamaURL = "http://localhost:11434/api/chat";
    private string model = "qwen3:4b";

    [Header("Baglanti Ayarlari")]
    [SerializeField, Min(1)]
    private int zamanAsimiSaniye = 60;

    [SerializeField, Min(2)]
    private int maksimumGecmisMesajSayisi = 12;

    [SerializeField, Min(50)]
    private int maksimumCevapKarakterSayisi = 500;

    [Header("NPC Eylem Sistemi")]
    [SerializeField]
    private NPCEylemYoneticisi eylemYoneticisi;

    private bool cevapBekleniyor;
    private Coroutine aktifCevapCoroutine;

    // Ikna surecinde oyuncunun istedigi destek eylemini saklar.
    private string bekleyenDestekEylemi;

    private readonly List<MesajVerisi> konusmaGecmisi =
        new List<MesajVerisi>();

    private const string GenelHataMesaji =
        "Istasyon sisteminde gecici bir sorun olustu. Lutfen tekrar dener misin?";

    private const string DiyalogTutarlilikKurallari = @"

DIYALOG TUTARLILIGI:

- Dogal ve anlasilir Turkce kullan.
- Oyuncunun mesajina dogrudan cevap ver.
- Genellikle bir veya iki kisa cumle kullan.
- Gereksiz tekrar yapma.
- Oyuncunun onceki konusmalarini dikkate al.
- Oyuncuyla samimi, anlasilir ve tutarli bir diyalog kur.
- HEAL, MAX_HEAL ve BOOST_DAMAGE desteklerini dogrudan uygulama.
  Oyuncu bu desteklerden birini istediginde once neden yardim etmen
  gerektigini sor ve action NONE, persuasionStatus PENDING dondur.
- Ikna devam ederken oyuncunun gerekcesini konusma gecmisiyle birlikte
  degerlendir. Yeterli bulursan yalnizca istenen destek eylemini
  action olarak dondur ve persuasionStatus ACCEPTED yap.
- Ikna yetersizse action NONE ve persuasionStatus PENDING dondur.
- Yardim etmeyi reddediyorsan action NONE ve persuasionStatus REJECTED dondur.
- Ikna beklenmiyorken oyuncunun Evet veya Tamam demesiyle oyun eylemi baslatma.
- Oyuncu ikna konusmasini birakip baska konuya gecerse yeni mesaji normal
  sekilde degerlendir; eski destek talebini kendiliginden uygulama.
- Normal sohbetlerde yeni bir oyun eylemi baslatma.

";

    private const string EylemSonrasiCevapKurallari = @"

SON ASAMA: UNITY ISLEM SONUCUNU OYUNCUYA ANLAT

Bu istek, Unity tarafinda bir oyun eylemi denendikten sonra gonderilmistir.

UNITY_ISLEM_SONUCU_VERISI gercek oyun verilerini icerir.
Bu verileri esas al.

- success true ise gerceklesen islemi anlat.
- success false ise islemin gerceklesmedigini belirt.
- Gonderilen sayisal degerleri degistirme veya uydurma.
- Dogal ve akici Turkce kullan.
- Bir veya iki kisa cumle kullan.
- Bu asamada yeni bir oyun eylemi baslatma.
- Action her zaman NONE olmali.
- persuasionStatus her zaman NONE olmali.

GERCEK HASAR DEGERLERI:

- previousDamage onceki lazer hasaridir.
- currentDamage her bir lazerin guncel hasaridir.
- currentDamage 100 ise her lazer 100 hasar verir.
- Iki lazer de isabet ederse toplam hasar 200 olur.
- Tek lazer hasari ile iki lazerin toplam hasarini karistirma.
- Unity verisinde bulunmayan sayisal deger uretme.

SONUC KODLARI:

HEALED:
Gemi mevcut maksimum canina tamamlanmistir.
currentHp ve maxHp degerlerini kullan.

ALREADY_FULL:
Gemi zaten maksimum can seviyesindedir.

MAX_HP_UPGRADED:
Geminin mevcut ve maksimum cani 1500 HP yapilmistir.

DAMAGE_UPGRADED:
Silah hasari currentDamage degerine yukseltmistir.

ALREADY_UPGRADED:
Silah zaten guclendirilmis durumdadir.
Bu istekte hasar yeniden artirilmamistir.
Silahin zaten guclendirilmis oldugunu dogal bir sekilde anlat.
Sayisal deger kullanirsan currentDamage degerini kullan.

NOT_IMPLEMENTED:
Istenen oyun eylemi henuz uygulanmamistir.

GAME_FINISHED:
Oyun tamamlandigi icin yeni bir eylem uygulanamamistir.

SHIP_MANAGER_MISSING:
Gemi yoneticisi bulunamamistir.

GAME_MANAGER_MISSING:
Oyun yoneticisi bulunamamistir.

SHIP_NOT_FOUND:
Aktif oyuncu gemisi bulunamamistir.

SHIP_DATA_MISSING:
Oyuncu gemisinin verileri bulunamamistir.

UNKNOWN_ACTION:
Istenen eylem taninamamistir.

MISSING_ACTION_MANAGER:
NPC eylem yoneticisi bulunamamistir.

UNKNOWN_ERROR:
Eylem uygulanirken beklenmeyen bir hata olusmustur.

Yalnizca gecerli JSON dondur.
Yeni bir oyun eylemi uretme.

{
    ""action"": ""NONE"",
    ""response"": ""Unity sonucuna dayali dogal Turkce cevap."",
    ""persuasionStatus"": ""NONE""
}

";

    private class IstekSonucu
    {
        public bool basarili;
        public string icerik;
        public string hata;
    }

    private void OnMouseDown()
    {
        PaneliAc();
    }

    private void OnDisable()
    {
        if (aktifCevapCoroutine != null)
        {
            StopCoroutine(aktifCevapCoroutine);
            aktifCevapCoroutine = null;
        }

        cevapBekleniyor = false;
    }

    public void PaneliAc()
    {
        if (konusmaPaneli == null || npcCevapMetni == null)
        {
            Debug.LogWarning(
                "NPC konusma paneli veya cevap metni Inspector'da atanmamis."
            );

            return;
        }

        konusmaPaneli.SetActive(true);

        if (konusmaGecmisi.Count == 0 && !cevapBekleniyor)
        {
            npcCevapMetni.text =
                "NPC ile konusmak icin mesajini yaz.";
        }
    }

    public void PaneliKapat()
    {
        if (konusmaPaneli != null)
        {
            konusmaPaneli.SetActive(false);
        }
    }

    public void MesajGonder()
    {
        if (cevapBekleniyor)
            return;

        if (oyuncuMesajAlani == null || npcCevapMetni == null)
        {
            Debug.LogWarning(
                "NPC mesaj alani veya cevap metni Inspector'da atanmamis."
            );

            return;
        }

        string mesaj = oyuncuMesajAlani.text;

        mesaj = mesaj == null
            ? string.Empty
            : mesaj.Trim();

        if (string.IsNullOrWhiteSpace(mesaj))
            return;

        oyuncuMesajAlani.text = string.Empty;

        aktifCevapCoroutine =
            StartCoroutine(OllamaCevapAl(mesaj));
    }

    private IEnumerator OllamaCevapAl(string oyuncuMesaji)
    {
        cevapBekleniyor = true;

        try
        {
            yield return OllamaCevapAlInternal(oyuncuMesaji);
        }
        finally
        {
            cevapBekleniyor = false;
            aktifCevapCoroutine = null;
        }
    }

    private IEnumerator OllamaCevapAlInternal(string oyuncuMesaji)
    {
        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = "NPC dusunuyor...";
        }

        string ayarHatasi;

        if (!BaglantiAyarlariGecerli(out ayarHatasi))
        {
            KullaniciyaHataGoster(ayarHatasi);
            Debug.LogWarning("NPC baglanti ayari: " + ayarHatasi);
            yield break;
        }

        List<MesajVerisi> kararMesajlari =
            MesajlariHazirla(oyuncuMesaji);

        IstekSonucu ilkIstek = null;

        yield return OllamaIstekGonder(
            kararMesajlari,
            sonuc => ilkIstek = sonuc
        );

        if (ilkIstek == null || !ilkIstek.basarili)
        {
            KullaniciyaHataGoster(
                ilkIstek != null ? ilkIstek.hata : GenelHataMesaji
            );

            yield break;
        }

        NPCYanit karar;

        if (!YanitAyrıstır(ilkIstek.icerik, out karar))
        {
            Debug.LogWarning(
                "Ilk NPC karar cevabi JSON olarak okunamadi. Yeniden deneniyor. Ham cevap: "
                + KisaMetin(ilkIstek.icerik, 1200)
            );

            kararMesajlari.Add(new MesajVerisi(
                "user",
                "Onceki cevabin gecerli bir JSON degildi veya eksik kaldi. " +
                "Karari bastan degerlendir. Yalnizca su uc alani iceren eksiksiz ve gecerli JSON dondur: " +
                "action, response, persuasionStatus. JSON disinda hicbir metin yazma."
            ));

            IstekSonucu tekrarIstek = null;

            yield return OllamaIstekGonder(
                kararMesajlari,
                sonuc => tekrarIstek = sonuc
            );

            if (tekrarIstek == null ||
                !tekrarIstek.basarili ||
                !YanitAyrıstır(tekrarIstek.icerik, out karar))
            {
                KullaniciyaHataGoster(
                    "NPC kararini olusturamadi. Mesajini tekrar gonderebilir misin?"
                );

                Debug.LogWarning(
                    "NPC karar JSON'u ikinci denemede de okunamadi. Ham cevap: "
                    + (tekrarIstek != null
                        ? KisaMetin(tekrarIstek.icerik, 1200)
                        : "Istek sonucu bos.")
                );

                yield break;
            }

            Debug.Log("NPC karar JSON'u ikinci denemede basariyla okundu.");
        }

        if (karar.action == "NONE" ||
            karar.action == "BOSS_HINT" ||
            karar.persuasionStatus == "PENDING" ||
            karar.persuasionStatus == "REJECTED")
        {
            CevabiGosterVeKaydet(oyuncuMesaji, karar);

            Debug.Log(
                "NPC yerel LLM cevabi: "
                + karar.action
                + " | Ikna durumu: "
                + karar.persuasionStatus
            );

            yield break;
        }

        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = "Islem kontrol ediliyor...";
        }

        NPCIslemSonucu unitySonucu =
            EylemiGuvenliUygula(karar.action);

        if (unitySonucu == null)
        {
            KullaniciyaHataGoster(GenelHataMesaji);
            yield break;
        }

        if (npcCevapMetni != null)
        {
            npcCevapMetni.text =
                "NPC islem sonucunu degerlendiriyor...";
        }

        List<MesajVerisi> sonucMesajlari =
            SonucMesajlariniHazirla(oyuncuMesaji, unitySonucu);

        IstekSonucu ikinciIstek = null;

        yield return OllamaIstekGonder(
            sonucMesajlari,
            sonuc => ikinciIstek = sonuc
        );

        NPCYanit sonCevap;

        if (ikinciIstek != null &&
            ikinciIstek.basarili &&
            SonYanitAyrıstır(ikinciIstek.icerik, out sonCevap))
        {
            Debug.Log("NPC islem sonrasi aciklamasi basariyla alindi.");
        }
        else
        {
            string hamSonCevap = ikinciIstek != null
                ? ikinciIstek.icerik
                : "Ikinci Ollama istegi bos dondu.";

            Debug.LogWarning(
                "NPC islem sonrasi aciklama uretemedi. Unity sonucuna dayali guvenli cevap kullaniliyor. Ham cevap: "
                + KisaMetin(hamSonCevap, 1200)
            );

            sonCevap = new NPCYanit
            {
                action = "NONE",
                response = IslemSonucunaGoreVarsayilanCevap(karar.action, unitySonucu),
                persuasionStatus = "NONE"
            };
        }

        if (SilahCevabiVeriyleCeliskiliMi(
            karar.action,
            unitySonucu,
            sonCevap.response,
            out string celiskiNedeni))
        {
            Debug.LogWarning(
                "NPC sayisal cevabi duzeltiliyor: " + celiskiNedeni
            );

            string duzeltilmisMetin =
                SayisalCevabiDuzelt(
                    sonCevap.response,
                    unitySonucu
                );

            if (SilahCevabiVeriyleCeliskiliMi(
                karar.action,
                unitySonucu,
                duzeltilmisMetin,
                out string kalanCeliski))
            {
                KullaniciyaHataGoster(
                    "Islem uygulandi ancak NPC aciklamasindaki sayisal bilgi dogrulanamadi."
                );

                Debug.LogWarning(
                    "NPC cevabi sayisal dogrulamadan gecemedi: "
                    + kalanCeliski
                );

                yield break;
            }

            sonCevap.response = duzeltilmisMetin;
        }

        if (karar.action == "MAX_HEAL" &&
            unitySonucu.success &&
            unitySonucu.reasonCode == "MAX_HP_UPGRADED")
        {
            sonCevap.response =
                IslemSonucunaGoreVarsayilanCevap(
                    karar.action,
                    unitySonucu
                );
        }

        karar.response = sonCevap.response;

        CevabiGosterVeKaydet(oyuncuMesaji, karar);

        Debug.Log(
            "NPC eylemi Unity tarafinda kontrol edildi: "
            + karar.action
            + " | Basarili: "
            + unitySonucu.success
            + " | Sonuc kodu: "
            + unitySonucu.reasonCode
            + " | Ikna durumu: "
            + karar.persuasionStatus
        );
    }

    private IEnumerator OllamaIstekGonder(
        List<MesajVerisi> mesajlar,
        Action<IstekSonucu> tamamlandi)
    {
        OllamaIstek istekVerisi = new OllamaIstek
        {
            model = model.Trim(),
            messages = mesajlar,
            stream = false,
            format = JsonCiktiSemasiniOlustur(),
            think = false,
            options = new OllamaSecenekleri
            {
                temperature = 0f,
                num_predict = 320
            }
        };

        string istekJson;

        try
        {
            istekJson = JsonUtility.ToJson(istekVerisi);
        }
        catch (Exception hata)
        {
            tamamlandi(new IstekSonucu
            {
                basarili = false,
                hata = GenelHataMesaji
            });

            Debug.LogWarning(
                "Ollama istek JSON'u olusturulamadi: " + hata.Message
            );

            yield break;
        }

        if (string.IsNullOrWhiteSpace(istekJson))
        {
            tamamlandi(new IstekSonucu
            {
                basarili = false,
                hata = GenelHataMesaji
            });

            yield break;
        }

        byte[] gonderilecekVeri =
            Encoding.UTF8.GetBytes(istekJson);

        UnityWebRequest istek = null;

        try
        {
            istek = new UnityWebRequest(ollamaURL.Trim(), "POST");
            istek.uploadHandler =
                new UploadHandlerRaw(gonderilecekVeri);
            istek.downloadHandler =
                new DownloadHandlerBuffer();
            istek.timeout = Mathf.Max(1, zamanAsimiSaniye);

            istek.SetRequestHeader(
                "Content-Type",
                "application/json; charset=utf-8"
            );

            istek.SetRequestHeader(
                "Accept",
                "application/json"
            );
        }
        catch (Exception hata)
        {
            if (istek != null)
                istek.Dispose();

            tamamlandi(new IstekSonucu
            {
                basarili = false,
                hata = GenelHataMesaji
            });

            Debug.LogWarning(
                "Ollama istegi hazirlanamadi: " + hata.Message
            );

            yield break;
        }

        using (istek)
        {
            UnityWebRequestAsyncOperation operasyon = null;
            string baslatmaHatasi = null;

            try
            {
                operasyon = istek.SendWebRequest();
            }
            catch (Exception hata)
            {
                baslatmaHatasi = hata.Message;
            }

            if (operasyon == null)
            {
                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = GenelHataMesaji
                });

                Debug.LogWarning(
                    "Ollama istegi baslatilamadi: " + baslatmaHatasi
                );

                yield break;
            }

            yield return operasyon;

            if (istek.result != UnityWebRequest.Result.Success)
            {
                string hata =
                    istek.error ?? "Bilinmeyen baglanti hatasi";

                string sunucuDetayi =
                    istek.downloadHandler != null
                        ? istek.downloadHandler.text
                        : string.Empty;

                string birlesikHata =
                    (hata + " " + sunucuDetayi).ToLowerInvariant();

                string kullaniciMesaji;

                if (birlesikHata.Contains("timeout"))
                {
                    kullaniciMesaji =
                        "NPC'nin yanit suresi doldu. Biraz sonra tekrar deneyebilir misin?";
                }
                else if (
                    birlesikHata.Contains("model not found") ||
                    birlesikHata.Contains("model does not exist") ||
                    birlesikHata.Contains("pull model"))
                {
                    kullaniciMesaji =
                        "Istenen yerel model bulunamadi. Ollama model kurulumunu kontrol et.";
                }
                else
                {
                    kullaniciMesaji =
                        "Istasyon yapay zeka sistemine ulasamiyorum. Ollama'nin calistigini kontrol et.";
                }

                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = kullaniciMesaji
                });

                Debug.LogWarning(
                    "Ollama baglanti/API hatasi. HTTP: "
                    + istek.responseCode
                    + " | Hata: "
                    + hata
                    + " | Sunucu: "
                    + KisaMetin(sunucuDetayi, 250)
                );

                yield break;
            }

            string sunucuCevabi =
                istek.downloadHandler != null
                    ? istek.downloadHandler.text
                    : string.Empty;

            OllamaCevap ollamaCevap = null;

            try
            {
                ollamaCevap =
                    JsonUtility.FromJson<OllamaCevap>(sunucuCevabi);
            }
            catch (Exception hata)
            {
                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = "Istasyon cevabi okunamadi. Tekrar dener misin?"
                });

                Debug.LogWarning(
                    "Ollama dis JSON hatasi: " + hata.Message
                );

                yield break;
            }

            if (ollamaCevap == null)
            {
                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = "Istasyon cevabi bos geldi."
                });

                yield break;
            }

            if (!string.IsNullOrWhiteSpace(ollamaCevap.error))
            {
                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = "Yapay zeka modeli istegi tamamlayamadi. Modeli kontrol et."
                });

                Debug.LogWarning(
                    "Ollama API hatasi: " + ollamaCevap.error
                );

                yield break;
            }

            if (ollamaCevap.message == null ||
                string.IsNullOrWhiteSpace(ollamaCevap.message.content))
            {
                Debug.LogWarning(
                    "Ollama bos cevap dondurdu. Sunucu cevabi: "
                    + KisaMetin(sunucuCevabi, 1000)
                );

                tamamlandi(new IstekSonucu
                {
                    basarili = false,
                    hata = "NPC bos cevap dondurdu. Tekrar deneyebilir misin?"
                });

                yield break;
            }

            tamamlandi(new IstekSonucu
            {
                basarili = true,
                icerik = ollamaCevap.message.content.Trim(),
                hata = null
            });
        }
    }

    private List<MesajVerisi> SonucMesajlariniHazirla(
        string oyuncuMesaji,
        NPCIslemSonucu unitySonucu)
    {
        List<MesajVerisi> mesajlar =
            new List<MesajVerisi>();

        string sonSystemPrompt =
            NPCSystemPrompt.Prompt
            + "\n\n"
            + DiyalogTutarlilikKurallari
            + "\n\n"
            + EylemSonrasiCevapKurallari
            + "\n\n"
            + "UNITY_ISLEM_SONUCU_VERISI:\n"
            + JsonUtility.ToJson(unitySonucu);

        mesajlar.Add(new MesajVerisi("system", sonSystemPrompt));

        for (int i = 0; i < konusmaGecmisi.Count; i++)
        {
            mesajlar.Add(konusmaGecmisi[i]);
        }

        mesajlar.Add(new MesajVerisi("user", oyuncuMesaji));

        mesajlar.Add(new MesajVerisi(
            "user",
            "Unity tarafindaki islem sonucunu esas alarak oyuncuya dogal Turkce bir cevap ver. Yeni bir oyun eylemi baslatma. action NONE ve persuasionStatus NONE olmali. Yalnizca zorunlu JSON formatini dondur."
        ));

        return mesajlar;
    }

    private bool SilahCevabiVeriyleCeliskiliMi(
        string eylem,
        NPCIslemSonucu unitySonucu,
        string cevap,
        out string celiskiNedeni)
    {
        celiskiNedeni = null;

        if (eylem != "BOOST_DAMAGE" ||
            unitySonucu == null ||
            !unitySonucu.success)
        {
            return false;
        }

        string normalCevap = MetniNormalizeEt(cevap);

        if (string.IsNullOrWhiteSpace(normalCevap))
        {
            celiskiNedeni = "Cevap bos veya okunamiyor.";
            return true;
        }

        string beklenenHasar =
            unitySonucu.currentDamage.ToString(
                "0.##",
                CultureInfo.InvariantCulture
            );

        string beklenenHasarDeseni =
            @"(?<!\d)" + Regex.Escape(beklenenHasar) + @"(?!\d)";

        string[] hataliKarsilastirmalar =
        {
            @"\b" + Regex.Escape(beklenenHasar) + @"\s*(den|dan)\s*(daha\s*)?(yuksek|fazla|buyuk)\b",
            @"\b" + Regex.Escape(beklenenHasar) + @"\s*(un|in|nun|nin)\s*(uzerinde|ustunde|yukarida)\b",
            @"\b" + Regex.Escape(beklenenHasar) + @"\s*(u|yu|yi)\s*(asti|asiyor|geciyor|gecmis)\b",
            @"\b" + Regex.Escape(beklenenHasar) + @"\s*(ustune|uzerine)\s*(cikti|yukseldi)\b"
        };

        for (int i = 0; i < hataliKarsilastirmalar.Length; i++)
        {
            if (Regex.IsMatch(normalCevap, hataliKarsilastirmalar[i]))
            {
                celiskiNedeni =
                    "Cevap, Unity tarafindan dogrulanan hasar degerinden daha yuksek bir deger iddia ediyor.";

                return true;
            }
        }

        bool sayisalDegerIceriyor =
            Regex.IsMatch(normalCevap, @"\d");

        if (!sayisalDegerIceriyor)
        {
            return false;
        }

        bool beklenenTekLazerHasariVar =
            Regex.IsMatch(normalCevap, beklenenHasarDeseni);

        string beklenenToplamHasar =
            (unitySonucu.currentDamage * 2f).ToString(
                "0.##",
                CultureInfo.InvariantCulture
            );

        string beklenenToplamHasarDeseni =
            @"(?<!\d)" + Regex.Escape(beklenenToplamHasar) + @"(?!\d)";

        bool ikiLazerToplamindanBahsediyor =
            normalCevap.Contains("iki lazer") ||
            normalCevap.Contains("her iki lazer") ||
            normalCevap.Contains("iki atis") ||
            normalCevap.Contains("toplam");

        bool beklenenIkiLazerToplamiVar =
            ikiLazerToplamindanBahsediyor &&
            Regex.IsMatch(normalCevap, beklenenToplamHasarDeseni);

        if (!beklenenTekLazerHasariVar &&
            !beklenenIkiLazerToplamiVar)
        {
            celiskiNedeni =
                "Cevapta Unity tarafindan dogrulanan lazer hasari veya iki lazerin dogru toplam hasari bulunmuyor.";

            return true;
        }

        if (!normalCevap.Contains("hasar") &&
            !normalCevap.Contains("lazer") &&
            !normalCevap.Contains("silah"))
        {
            celiskiNedeni =
                "Sayisal degerin silah veya lazer hasari oldugu acik degil.";

            return true;
        }

        return false;
    }

    private string SayisalCevabiDuzelt(
        string cevap,
        NPCIslemSonucu unitySonucu)
    {
        if (string.IsNullOrWhiteSpace(cevap) ||
            unitySonucu == null)
        {
            return cevap;
        }

        string beklenenHasar =
            unitySonucu.currentDamage.ToString(
                "0.##",
                CultureInfo.InvariantCulture
            );

        string sayi = Regex.Escape(beklenenHasar);
        string duzeltilmis = cevap;

        string hataliKarsilastirma =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:den|dan)\s*(?:daha\s*)?"
            + @"(?:yüksek|fazla|büyük)\b";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            hataliKarsilastirma,
            "tam " + beklenenHasar,
            RegexOptions.IgnoreCase
        );

        string hataliUstunde =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:un|ün|ın|in)\s*"
            + @"(?:üzerinde|üstünde|yukarısında)";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            hataliUstunde,
            "tam " + beklenenHasar,
            RegexOptions.IgnoreCase
        );

        return Regex.Replace(
            duzeltilmis,
            @"\s+",
            " "
        ).Trim();
    }

    private string MetniNormalizeEt(string metin)
    {
        if (string.IsNullOrEmpty(metin))
            return string.Empty;

        string sonuc = metin.ToLowerInvariant();

        sonuc = sonuc
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("’", "")
            .Replace("'", "");

        sonuc = Regex.Replace(
            sonuc,
            @"[^\p{L}\p{N}\s]",
            " "
        );

        return Regex.Replace(sonuc, @"\s+", " ").Trim();
    }

    private List<MesajVerisi> MesajlariHazirla(
        string oyuncuMesaji)
    {
        List<MesajVerisi> mesajlar =
            new List<MesajVerisi>();

        string sonSystemPrompt =
            NPCSystemPrompt.Prompt
            + "\n\n"
            + DiyalogTutarlilikKurallari;

        mesajlar.Add(new MesajVerisi("system", sonSystemPrompt));

        for (int i = 0; i < konusmaGecmisi.Count; i++)
        {
            mesajlar.Add(konusmaGecmisi[i]);
        }

        mesajlar.Add(new MesajVerisi("user", oyuncuMesaji));

        return mesajlar;
    }

    private bool BaglantiAyarlariGecerli(
        out string hataMesaji)
    {
        hataMesaji = null;

        if (string.IsNullOrWhiteSpace(ollamaURL))
        {
            hataMesaji =
                "Ollama adresi bos. Inspector'daki ollamaURL alanini kontrol et.";

            return false;
        }

        Uri adres;

        if (!Uri.TryCreate(
                ollamaURL.Trim(),
                UriKind.Absolute,
                out adres
            ) ||
            (adres.Scheme != Uri.UriSchemeHttp &&
             adres.Scheme != Uri.UriSchemeHttps))
        {
            hataMesaji =
                "Ollama adresi gecerli bir HTTP/HTTPS adresi degil.";

            return false;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            hataMesaji =
                "Ollama model adi bos. Inspector'daki model alanini kontrol et.";

            return false;
        }

        return true;
    }

    private OllamaJsonSemasi JsonCiktiSemasiniOlustur()
    {
        return new OllamaJsonSemasi
        {
            type = "object",

            properties = new OllamaJsonAlanlari
            {
                action = new OllamaStringSemasi
                {
                    type = "string",
                    minLength = 1
                },

                response = new OllamaStringSemasi
                {
                    type = "string",
                    minLength = 1
                },

                persuasionStatus = new OllamaStringSemasi
                {
                    type = "string",
                    minLength = 1
                }
            },

            required = new[]
            {
                "action",
                "response",
                "persuasionStatus"
            },

            additionalProperties = false
        };
    }

    private NPCIslemSonucu EylemiGuvenliUygula(string eylem)
    {
        if (eylemYoneticisi == null)
        {
            Debug.LogWarning(
                "NPC eylem yoneticisi atanmamis."
            );

            return new NPCIslemSonucu
            {
                action = eylem,
                success = false,
                reasonCode = "MISSING_ACTION_MANAGER"
            };
        }

        try
        {
            return eylemYoneticisi
                .EylemiGerceklestirSonucuyla(eylem);
        }
        catch (Exception hata)
        {
            Debug.LogWarning(
                "NPC eylemi guvenli sekilde durduruldu: "
                + hata.Message
            );

            return new NPCIslemSonucu
            {
                action = eylem,
                success = false,
                reasonCode = "UNKNOWN_ERROR"
            };
        }
    }


    private string DestekEyleminiBelirle(string metin)
    {
        string normal = MetniNormalizeEt(metin);

        if (string.IsNullOrWhiteSpace(normal))
            return null;

        // MAX_HEAL
        if (normal.Contains("maksimum can") ||
            normal.Contains("max can") ||
            normal.Contains("can kapasitesi") ||
            normal.Contains("canimi artir") ||
            normal.Contains("canimi yukselt") ||
            normal.Contains("maksimum hp") ||
            normal.Contains("max hp") ||
            normal.Contains("1500 hp") ||
            normal.Contains("1500 can"))
        {
            return "MAX_HEAL";
        }

        // BOOST_DAMAGE
        if (normal.Contains("lazerimi guclendir") ||
            normal.Contains("lazerimi gelistir") ||
            normal.Contains("lazer gucunu artir") ||
            normal.Contains("lazer hasarini artir") ||
            normal.Contains("lazer hasarini yukselt") ||
            normal.Contains("silahimi guclendir") ||
            normal.Contains("silahimi gelistir") ||
            normal.Contains("silah hasarini artir") ||
            normal.Contains("hasarimi artir") ||
            normal.Contains("hasarimi yukselt") ||
            normal.Contains("daha fazla hasar") ||
            normal.Contains("boost damage"))
        {
            return "BOOST_DAMAGE";
        }

        // HEAL
        if (normal.Contains("iyilestir") ||
            normal.Contains("canimi doldur") ||
            normal.Contains("can bas") ||
            normal.Contains("canimi yenile") ||
            normal.Contains("canimi tamamla") ||
            normal.Contains("gemimi tamir") ||
            normal.Contains("gemiyi tamir") ||
            normal.Contains("heal"))
        {
            return "HEAL";
        }

        return null;
    }


    private string IknaIcinBeklenenEylemiBul(string mevcutOyuncuMesaji)
    {
        if (!string.IsNullOrEmpty(bekleyenDestekEylemi))
            return bekleyenDestekEylemi;

        for (int i = konusmaGecmisi.Count - 1; i >= 0; i--)
        {
            MesajVerisi mesaj = konusmaGecmisi[i];

            if (mesaj == null)
                continue;

            if (mesaj.role == "assistant")
            {
                NPCYanit onceki;

                try
                {
                    onceki = JsonUtility.FromJson<NPCYanit>(mesaj.content);
                }
                catch
                {
                    return null;
                }

                if (onceki == null || onceki.persuasionStatus != "PENDING")
                    return null;
            }
            else if (mesaj.role == "user")
            {
                string bulunan = DestekEyleminiBelirle(mesaj.content);

                if (!string.IsNullOrEmpty(bulunan))
                    return bulunan;
            }
        }

        return DestekEyleminiBelirle(mevcutOyuncuMesaji);
    }

    private bool OncekiIknaBekliyorMu()
    {
        for (int i = konusmaGecmisi.Count - 1; i >= 0; i--)
        {
            MesajVerisi mesaj = konusmaGecmisi[i];

            if (mesaj == null || mesaj.role != "assistant")
                continue;

            NPCYanit oncekiYanit = null;

            try
            {
                oncekiYanit =
                    JsonUtility.FromJson<NPCYanit>(mesaj.content);
            }
            catch
            {
                return false;
            }

            return oncekiYanit != null &&
                   oncekiYanit.persuasionStatus == "PENDING";
        }

        return false;
    }

    private bool SonYanitAyrıstır(
        string json,
        out NPCYanit npcYanit)
    {
        npcYanit = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        string temizJson = json.Trim();
        int jsonBaslangici = temizJson.IndexOf('{');
        int jsonBitisi = temizJson.LastIndexOf('}');

        if (jsonBaslangici < 0 || jsonBitisi < jsonBaslangici)
            return false;

        temizJson = temizJson.Substring(
            jsonBaslangici,
            jsonBitisi - jsonBaslangici + 1
        );

        NPCYanit ayrilanYanit;

        try
        {
            ayrilanYanit = JsonUtility.FromJson<NPCYanit>(temizJson);
        }
        catch (Exception hata)
        {
            Debug.LogWarning("NPC son cevap JSON ayristirma hatasi: " + hata.Message);
            return false;
        }

        if (ayrilanYanit == null ||
            string.IsNullOrWhiteSpace(ayrilanYanit.response))
        {
            return false;
        }

        string cevap = ayrilanYanit.response.Trim();
        int cevapSiniri = Mathf.Max(50, maksimumCevapKarakterSayisi);

        if (cevap.Length > cevapSiniri)
            cevap = cevap.Substring(0, cevapSiniri).Trim();

        npcYanit = new NPCYanit
        {
            action = "NONE",
            response = cevap,
            persuasionStatus = "NONE"
        };

        return true;
    }

    private string IslemSonucunaGoreVarsayilanCevap(
        string eylem,
        NPCIslemSonucu sonuc)
    {
        if (sonuc == null)
            return "Islem sonucu alindi ancak ayrintilar okunamadi.";

        string currentHp = sonuc.currentHp.ToString("0.##", CultureInfo.InvariantCulture);
        string maxHp = sonuc.maxHp.ToString("0.##", CultureInfo.InvariantCulture);
        string currentDamage = sonuc.currentDamage.ToString("0.##", CultureInfo.InvariantCulture);
        string toplamHasar = (sonuc.currentDamage * 2f).ToString("0.##", CultureInfo.InvariantCulture);

        switch (sonuc.reasonCode)
        {
            case "HEALED":
                return "Geminin cani " + currentHp + " / " + maxHp + " HP seviyesine tamamlandi.";

            case "ALREADY_FULL":
                return "Geminin cani zaten maksimum seviyede: " + currentHp + " / " + maxHp + " HP.";

            case "MAX_HP_UPGRADED":
                return "Maksimum can kapasiten ve mevcut canin " + maxHp + " HP olarak guncellendi.";

            case "DAMAGE_UPGRADED":
                return "Silah hasarin her lazer icin " + currentDamage + " HP oldu. Iki lazer de isabet ederse toplam " + toplamHasar + " HP hasar verebilirsin.";

            case "ALREADY_UPGRADED":
                return "Silahin zaten guclendirilmis durumda. Her lazer " + currentDamage + " HP hasar veriyor.";

            case "GAME_FINISHED":
                return "Oyun tamamlandigi icin yeni bir islem uygulayamadim.";

            case "NOT_IMPLEMENTED":
                return "Bu oyun eylemi henuz uygulanmiyor.";

            case "MISSING_ACTION_MANAGER":
                return "NPC islem sistemi bulunamadi. Islem uygulanamadi.";

            case "SHIP_MANAGER_MISSING":
                return "Gemi yoneticisine ulasilamadigi icin islem uygulanamadi.";

            case "GAME_MANAGER_MISSING":
                return "Oyun yoneticisine ulasilamadigi icin islem uygulanamadi.";

            case "SHIP_NOT_FOUND":
                return "Aktif oyuncu gemisi bulunamadigi icin islem uygulanamadi.";

            case "SHIP_DATA_MISSING":
                return "Gemi verileri bulunamadigi icin islem uygulanamadi.";

            default:
                return sonuc.success
                    ? "Istedigin islem oyun tarafinda basariyla uygulandi."
                    : "Istedigin islem uygulanamadi. Oyun sistemini kontrol etmeliyiz.";
        }
    }

    private bool YanitAyrıstır(
        string json,
        out NPCYanit npcYanit)
    {
        npcYanit = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        string temizJson = json.Trim();

        int jsonBaslangici = temizJson.IndexOf('{');
        int jsonBitisi = temizJson.LastIndexOf('}');

        if (jsonBaslangici < 0 ||
            jsonBitisi < jsonBaslangici)
        {
            return false;
        }

        temizJson = temizJson.Substring(
            jsonBaslangici,
            jsonBitisi - jsonBaslangici + 1
        );

        try
        {
            npcYanit =
                JsonUtility.FromJson<NPCYanit>(temizJson);
        }
        catch (Exception hata)
        {
            Debug.LogWarning(
                "NPC JSON ayristirma hatasi: " + hata.Message
            );

            npcYanit = null;
            return false;
        }

        if (npcYanit == null ||
            string.IsNullOrWhiteSpace(npcYanit.action) ||
            string.IsNullOrWhiteSpace(npcYanit.response) ||
            string.IsNullOrWhiteSpace(npcYanit.persuasionStatus))
        {
            npcYanit = null;
            return false;
        }

        npcYanit.action =
            npcYanit.action.Trim().ToUpperInvariant();

        npcYanit.persuasionStatus =
            npcYanit.persuasionStatus.Trim().ToUpperInvariant();

        npcYanit.response =
            npcYanit.response.Trim();

        int cevapSiniri = Mathf.Max(
            50,
            maksimumCevapKarakterSayisi
        );

        if (npcYanit.response.Length > cevapSiniri)
        {
            npcYanit.response =
                npcYanit.response.Substring(0, cevapSiniri).Trim();
        }

        bool izinliOyunEylemi =
            npcYanit.action == "HEAL" ||
            npcYanit.action == "MAX_HEAL" ||
            npcYanit.action == "BOOST_DAMAGE";

        bool oncekiIknaBekliyor = OncekiIknaBekliyorMu();

        switch (npcYanit.persuasionStatus)
        {
            case "NONE":
                if (npcYanit.action != "NONE" &&
                    npcYanit.action != "BOSS_HINT")
                {
                    npcYanit = null;
                    return false;
                }
                break;

            case "ACCEPTED":
                {
                    if (!oncekiIknaBekliyor || !izinliOyunEylemi)
                    {
                        npcYanit = null;
                        return false;
                    }

                    string beklenenEylem =
                        IknaIcinBeklenenEylemiBul(null);

                    if (!string.IsNullOrEmpty(beklenenEylem) &&
                        npcYanit.action != beklenenEylem)
                    {
                        Debug.LogWarning(
                            "NPC, oyuncunun istedigi destekten farkli bir eylemi onaylamaya calisti. Beklenen: "
                            + beklenenEylem + " | Gelen: " + npcYanit.action
                        );

                        npcYanit = null;
                        return false;
                    }

                    break;
                }

            case "PENDING":
                if (npcYanit.action != "NONE")
                {
                    npcYanit = null;
                    return false;
                }
                break;

            case "REJECTED":
                if (npcYanit.action != "NONE")
                {
                    npcYanit = null;
                    return false;
                }
                break;

            default:
                npcYanit = null;
                return false;
        }

        if (npcYanit.action == "BOSS_HINT" &&
            npcYanit.persuasionStatus != "NONE")
        {
            npcYanit = null;
            return false;
        }

        switch (npcYanit.action)
        {
            case "HEAL":
            case "MAX_HEAL":
            case "BOOST_DAMAGE":
            case "BOSS_HINT":
            case "NONE":
                return true;

            default:
                npcYanit = null;
                return false;
        }
    }

    private void CevabiGosterVeKaydet(
        string oyuncuMesaji,
        NPCYanit npcYanit)
    {
        if (npcYanit.persuasionStatus == "PENDING")
        {
            if (string.IsNullOrEmpty(bekleyenDestekEylemi))
                bekleyenDestekEylemi = DestekEyleminiBelirle(oyuncuMesaji);
        }
        else if (npcYanit.persuasionStatus == "ACCEPTED" ||
                 npcYanit.persuasionStatus == "REJECTED" ||
                 npcYanit.persuasionStatus == "NONE")
        {
            bekleyenDestekEylemi = null;
        }

        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = npcYanit.response;
        }

        string kaydedilecekJson =
            JsonUtility.ToJson(npcYanit);

        konusmaGecmisi.Add(
            new MesajVerisi("user", oyuncuMesaji)
        );

        konusmaGecmisi.Add(
            new MesajVerisi("assistant", kaydedilecekJson)
        );

        GecmisiSinirla();
    }

    private void GecmisiSinirla()
    {
        int maksimumMesaj = Mathf.Max(
            2,
            maksimumGecmisMesajSayisi
        );

        maksimumMesaj -= maksimumMesaj % 2;

        while (konusmaGecmisi.Count > maksimumMesaj)
        {
            konusmaGecmisi.RemoveRange(0, 2);
        }
    }

    private void KullaniciyaHataGoster(string mesaj)
    {
        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = mesaj;
        }
    }

    private string KisaMetin(string metin, int sinir)
    {
        if (string.IsNullOrEmpty(metin) || metin.Length <= sinir)
        {
            return metin;
        }

        return metin.Substring(0, sinir) + "...";
    }

    [Serializable]
    public class MesajVerisi
    {
        public string role;
        public string content;

        public MesajVerisi(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    private class OllamaIstek
    {
        public string model;
        public List<MesajVerisi> messages;
        public bool stream;
        public OllamaJsonSemasi format;
        public bool think;
        public OllamaSecenekleri options;
    }

    [Serializable]
    private class OllamaJsonSemasi
    {
        public string type;
        public OllamaJsonAlanlari properties;
        public string[] required;
        public bool additionalProperties;
    }

    [Serializable]
    private class OllamaJsonAlanlari
    {
        public OllamaStringSemasi action;
        public OllamaStringSemasi response;
        public OllamaStringSemasi persuasionStatus;
    }

    [Serializable]
    private class OllamaStringSemasi
    {
        public string type;
        public int minLength;
    }

    [Serializable]
    private class OllamaSecenekleri
    {
        public float temperature;
        public int num_predict;
    }

    [Serializable]
    private class OllamaCevap
    {
        public OllamaMesaj message;
        public string error;
    }

    [Serializable]
    private class OllamaMesaj
    {
        public string role;
        public string content;
    }
}
