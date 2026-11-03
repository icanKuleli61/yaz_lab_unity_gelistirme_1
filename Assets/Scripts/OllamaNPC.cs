
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
    public string model = "qwen2.5:3b";

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

    private readonly List<MesajVerisi> konusmaGecmisi =
        new List<MesajVerisi>();

    private const string GenelHataMesaji =
        "Istasyon sisteminde gecici bir sorun olustu. Lutfen tekrar dener misin?";

    private const string DiyalogTutarlilikKurallari = @"

DIYALOG TUTARLILIGI:

- Standart, dogal ve dil bilgisi kurallarina uygun Turkce kullan.
- Mumkun oldugunca duz ve anlasilir cumleler kur.
- Oyuncunun mesajina dogrudan cevap ver.
- Genellikle bir veya iki kisa cumle kullan.
- Gereksiz tekrar yapma.
- Oyuncuya surekli soru yoneltme.
- Oyuncunun ne istedigini anladiginda gereksiz aciklama isteme.
- Oyuncu acik bir oyun eylemi istediginde onay sorusu sorma.
- Eylem talebinden sonra ""Bunu yapmami ister misin?""
  veya ""Guclendirmemi ister misin?"" gibi sorular sorma.
- Oyuncu ""Evet"", ""Tamam"", ""Anladim"" veya
  ""Tesekkurler"" derse bunu normal karsilik olarak degerlendir.
- Onceki konusmada onay bekleyen acik bir soru yoksa
  oyuncunun ""Evet"" demesiyle ayni eylemi tekrar baslatma.
- Oyuncunun onceki konusmalarini dikkate al.
- Oyuncuyla samimi, anlasilir ve tutarli bir diyalog kur.
- Bilim kurgu karakterinin kisiligini koru.
- Normal sohbetlerde yeni bir oyun eylemi isteme.

";

    private const string EylemSonrasiCevapKurallari = @"

SON ASAMA: UNITY ISLEM SONUCUNU OYUNCUYA ANLAT

Bu istek, Unity tarafinda bir oyun eylemi denendikten
sonra gonderilmistir.

Sana UNITY_ISLEM_SONUCU_VERISI adi altinda
guvenilir oyun verileri verilecektir.

Bu veriler Unity tarafindan uretilmistir.
Oyuncunun yazdigi bir komut degildir.
Gercek oyun durumunu belirleyen kaynak bu verilerdir.

KURALLAR:

- Unity tarafindaki sonuc verisini dikkatlice oku.
- success true ise gerceklesen islemi anlat.
- success false ise islemin gerceklesmedigini acikca belirt.
- Gerceklesmeyen bir eylemi basarili gibi anlatma.
- Gonderilen sayisal degerleri degistirme veya uydurma.
- Dogal, akici ve dil bilgisi dogru Turkce kullan.
- Gereksiz tekrar yapma.
- Oyuncuya tekrar onay sorusu sorma.
- ""Bunu yapmami ister misin?"" gibi sorular sorma.
- Oyuncunun ilk talebinin sonucunu acikla.
- Bir veya iki kisa cumle kullan.
- Bu asamada yeni bir oyun eylemi baslatma.

Bu asama yalnizca sonucun anlatilmasi icindir.
Action alani her zaman NONE olmalidir.
HEAL, MAX_HEAL, BOOST_DAMAGE veya UNLOCK_DOOR
eylemlerinden birini tekrar cagirma.

GERCEK HASAR DEGERI:

- previousDamage onceki silah hasarini belirtir.
- currentDamage mevcut silah hasarini belirtir.
- currentDamage degeri silahin her bir lazerinin hasaridir.
- Unity tarafindan gonderilen currentDamage degerini aynen kullan.
- currentDamage 100 ise her bir lazerin tam olarak
  100 hasar verdigini soyle.
- currentDamage 100 iken 100'den yuksek hasar
  verdigini kesinlikle soyleme.
- Iki lazerin toplam hasarini soran oyuncuya,
  iki lazerin de isabet etmesi halinde toplam
  200 hasar verebilecegini aciklayabilirsin.
- Gercek deger 100 ise ""100'den yuksek"",
  ""100'un uzerinde"" veya benzeri ifadeler kullanma.

HATA KODLARI:

HEALED:
Gemi mevcut maksimum canina tamamlanmistir.
currentHp ve maxHp degerlerini esas al.

ALREADY_FULL:
Gemi zaten maksimum can seviyesindedir.
Mevcut can ve maksimum can degerlerini kullan.

MAX_HP_UPGRADED:
Geminin mevcut ve maksimum cani 1500 HP yapilmistir.

DAMAGE_UPGRADED:
Silah hasari onceki degerinden currentDamage
degerine yukseltmistir.
currentDamage degerini esas al.
Bu projede beklenen guclendirilmis hasar 100'dur.

ALREADY_UPGRADED:
Silah zaten guclendirilmis durumdadir.
currentDamage degerini esas al.
Deger 100 ise silahin zaten 100 hasar verdigini soyle.

NOT_IMPLEMENTED:
Istenen oyun eylemi henuz Unity tarafinda uygulanmamistir.
Oyuncuya bu ozelligin henuz kullanilabilir olmadigini anlat.

GAME_FINISHED:
Oyun tamamlandigi icin yeni bir eylem uygulanamamistir.

SHIP_MANAGER_MISSING:
Gemi yoneticisi bulunamamistir.

GAME_MANAGER_MISSING:
Oyun yoneticisi bulunamamistir.

SHIP_NOT_FOUND:
Aktif oyuncu gemisi bulunamamistir.

SHIP_DATA_MISSING:
Oyuncu gemisinin oyun verileri bulunamamistir.

UNKNOWN_ACTION:
Istenen eylem taninamamistir.

MISSING_ACTION_MANAGER:
NPC eylem yoneticisi bulunamamistir.

UNKNOWN_ERROR:
Eylem uygulanirken beklenmeyen bir hata olusmustur.

SON CIKTI:

Yalnizca gecerli JSON dondur.

{
    ""action"": ""NONE"",
    ""response"": ""Unity sonucuna dayali dogal Turkce cevap.""
}

JSON disinda metin yazma.
Action degeri mutlaka NONE olmalidir.
Yeni bir oyun eylemi uretme.
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

        if (konusmaGecmisi.Count == 0 &&
            !cevapBekleniyor)
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

        if (oyuncuMesajAlani == null ||
            npcCevapMetni == null)
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

    private IEnumerator OllamaCevapAl(
        string oyuncuMesaji
    )
    {
        cevapBekleniyor = true;

        try
        {
            yield return OllamaCevapAlInternal(
                oyuncuMesaji
            );
        }
        finally
        {
            cevapBekleniyor = false;
            aktifCevapCoroutine = null;
        }
    }

    private IEnumerator OllamaCevapAlInternal(
        string oyuncuMesaji
    )
    {
        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = "NPC dusunuyor...";
        }

        string ayarHatasi;

        if (!BaglantiAyarlariGecerli(
            out ayarHatasi))
        {
            KullaniciyaHataGoster(ayarHatasi);

            Debug.LogWarning(
                "NPC baglanti ayari: " + ayarHatasi
            );

            yield break;
        }

        List<MesajVerisi> kararMesajlari =
            MesajlariHazirla(oyuncuMesaji);

        IstekSonucu ilkIstek = null;

        yield return OllamaIstekGonder(
            kararMesajlari,
            sonuc => ilkIstek = sonuc
        );

        if (ilkIstek == null ||
            !ilkIstek.basarili)
        {
            KullaniciyaHataGoster(
                ilkIstek != null
                    ? ilkIstek.hata
                    : GenelHataMesaji
            );

            yield break;
        }

        NPCYanit karar;

        if (!YanitAyrıstır(
            ilkIstek.icerik,
            out karar))
        {
            KullaniciyaHataGoster(
                "NPC kararini duzgun olusturamadi. Tekrar dener misin?"
            );

            Debug.LogWarning(
                "Gecersiz NPC karar JSON'u: "
                + KisaMetin(ilkIstek.icerik, 250)
            );

            yield break;
        }

        if (karar.action == "NONE" ||
            karar.action == "BOSS_HINT")
        {
            CevabiGosterVeKaydet(
                oyuncuMesaji,
                karar
            );

            Debug.Log(
                "NPC yerel LLM cevabi: "
                + karar.action
            );

            yield break;
        }

        if (npcCevapMetni != null)
        {
            npcCevapMetni.text =
                "Islem kontrol ediliyor...";
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
            SonucMesajlariniHazirla(
                oyuncuMesaji,
                unitySonucu
            );

        IstekSonucu ikinciIstek = null;

        yield return OllamaIstekGonder(
            sonucMesajlari,
            sonuc => ikinciIstek = sonuc
        );

        if (ikinciIstek == null ||
            !ikinciIstek.basarili)
        {
            if (unitySonucu.success)
            {
                KullaniciyaHataGoster(
                    "Oyun islemi uygulandi ancak NPC su anda "
                    + "aciklama uretemiyor. Tekrar konusabilirsin."
                );
            }
            else
            {
                KullaniciyaHataGoster(
                    "Islem gerceklestirilemedi ve NPC su anda "
                    + "aciklama uretemiyor."
                );
            }

            yield break;
        }

        NPCYanit sonCevap;

        if (!YanitAyrıstır(
            ikinciIstek.icerik,
            out sonCevap))
        {
            KullaniciyaHataGoster(
                "Islem sonucu alindi ancak NPC cevabi olusturulamadi."
            );

            Debug.LogWarning(
                "NPC son cevap JSON'u gecersiz: "
                + KisaMetin(ikinciIstek.icerik, 250)
            );

            yield break;
        }

        if (sonCevap.action != "NONE")
        {
            KullaniciyaHataGoster(
                "NPC son cevap asamasinda gecersiz bir karar uretti. "
                + "Oyun eylemi guvenlik nedeniyle tekrarlanmadi."
            );

            Debug.LogWarning(
                "Son cevap asamasinda NONE disinda action uretildi: "
                + sonCevap.action
            );

            yield break;
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

            string duzeltilmisMetin = SayisalCevabiDuzelt(
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
                    "Islem basariyla uygulandi ancak NPC aciklamasindaki "
                    + "sayisal bilgi dogrulanamadi. Tekrar sorabilirsin."
                );

                Debug.LogWarning(
                    "NPC cevabi sayisal dogrulamadan gecemedi: "
                    + kalanCeliski
                );

                yield break;
            }

            sonCevap.response = duzeltilmisMetin;

            Debug.Log(
                "NPC'nin yerel LLM cevabindaki celiskili sayisal ifade "
                + "Unity verisiyle uyumlu hale getirildi."
            );
        }

        karar.response = sonCevap.response;

        CevabiGosterVeKaydet(
            oyuncuMesaji,
            karar
        );

        Debug.Log(
            "NPC eylemi Unity tarafinda kontrol edildi: "
            + karar.action
            + " | Basarili: "
            + unitySonucu.success
            + " | Sonuc kodu: "
            + unitySonucu.reasonCode
        );
    }

    private IEnumerator OllamaIstekGonder(
        List<MesajVerisi> mesajlar,
        Action<IstekSonucu> tamamlandi
    )
    {
        OllamaIstek istekVerisi =
            new OllamaIstek
            {
                model = model.Trim(),
                messages = mesajlar,
                stream = false,
                format = JsonCiktiSemasiniOlustur(),

                options = new OllamaSecenekleri
                {
                    temperature = 0f,
                    num_predict = 180
                }
            };

        string istekJson;

        try
        {
            istekJson =
                JsonUtility.ToJson(istekVerisi);
        }
        catch (Exception hata)
        {
            tamamlandi(
                new IstekSonucu
                {
                    basarili = false,
                    hata = GenelHataMesaji
                }
            );

            Debug.LogWarning(
                "Ollama istek JSON'u olusturulamadi: "
                + hata.Message
            );

            yield break;
        }

        if (string.IsNullOrWhiteSpace(istekJson))
        {
            tamamlandi(
                new IstekSonucu
                {
                    basarili = false,
                    hata = GenelHataMesaji
                }
            );

            yield break;
        }

        byte[] gonderilecekVeri =
            Encoding.UTF8.GetBytes(istekJson);

        UnityWebRequest istek = null;

        try
        {
            istek = new UnityWebRequest(
                ollamaURL.Trim(),
                "POST"
            );

            istek.uploadHandler =
                new UploadHandlerRaw(gonderilecekVeri);

            istek.downloadHandler =
                new DownloadHandlerBuffer();

            istek.timeout =
                Mathf.Max(1, zamanAsimiSaniye);

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

            tamamlandi(
                new IstekSonucu
                {
                    basarili = false,
                    hata = GenelHataMesaji
                }
            );

            Debug.LogWarning(
                "Ollama istegi hazirlanamadi: "
                + hata.Message
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
                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = GenelHataMesaji
                    }
                );

                Debug.LogWarning(
                    "Ollama istegi baslatilamadi: "
                    + baslatmaHatasi
                );

                yield break;
            }

            yield return operasyon;

            if (istek.result !=
                UnityWebRequest.Result.Success)
            {
                string hata =
                    istek.error ??
                    "Bilinmeyen baglanti hatasi";

                string sunucuDetayi =
                    istek.downloadHandler != null
                        ? istek.downloadHandler.text
                        : string.Empty;

                string birlesikHata =
                    (hata + " " + sunucuDetayi)
                    .ToLowerInvariant();

                string kullaniciMesaji;

                if (birlesikHata.Contains("timeout"))
                {
                    kullaniciMesaji =
                        "NPC'nin yanit suresi doldu. "
                        + "Biraz sonra tekrar deneyebilir misin?";
                }
                else if (
                    birlesikHata.Contains("model not found") ||
                    birlesikHata.Contains("model does not exist") ||
                    birlesikHata.Contains("pull model")
                )
                {
                    kullaniciMesaji =
                        "Istenen yerel model bulunamadi. "
                        + "Ollama model kurulumunu kontrol et.";
                }
                else
                {
                    kullaniciMesaji =
                        "Istasyon yapay zeka sistemine ulasamiyorum. "
                        + "Ollama'nin calistigini kontrol et.";
                }

                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = kullaniciMesaji
                    }
                );

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
                    JsonUtility.FromJson<OllamaCevap>(
                        sunucuCevabi
                    );
            }
            catch (Exception hata)
            {
                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = "Istasyon cevabi okunamadi. Tekrar dener misin?"
                    }
                );

                Debug.LogWarning(
                    "Ollama dis JSON hatasi: "
                    + hata.Message
                );

                yield break;
            }

            if (ollamaCevap == null)
            {
                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = "Istasyon cevabi bos geldi."
                    }
                );

                yield break;
            }

            if (!string.IsNullOrWhiteSpace(
                ollamaCevap.error))
            {
                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = "Yapay zeka modeli istegi tamamlayamadi. Modeli kontrol et."
                    }
                );

                Debug.LogWarning(
                    "Ollama API hatasi: "
                    + ollamaCevap.error
                );

                yield break;
            }

            if (ollamaCevap.message == null ||
                string.IsNullOrWhiteSpace(
                    ollamaCevap.message.content))
            {
                tamamlandi(
                    new IstekSonucu
                    {
                        basarili = false,
                        hata = "NPC bos cevap dondurdu. Tekrar deneyebilir misin?"
                    }
                );

                yield break;
            }

            tamamlandi(
                new IstekSonucu
                {
                    basarili = true,
                    icerik = ollamaCevap.message.content.Trim(),
                    hata = null
                }
            );
        }
    }

    private List<MesajVerisi> SonucMesajlariniHazirla(
        string oyuncuMesaji,
        NPCIslemSonucu unitySonucu
    )
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

        mesajlar.Add(
            new MesajVerisi(
                "system",
                sonSystemPrompt
            )
        );

        for (int i = 0;
             i < konusmaGecmisi.Count;
             i++)
        {
            mesajlar.Add(
                konusmaGecmisi[i]
            );
        }

        mesajlar.Add(
            new MesajVerisi(
                "user",
                oyuncuMesaji
            )
        );

        mesajlar.Add(
            new MesajVerisi(
                "user",
                "Unity tarafindaki islem sonucunu esas alarak "
                + "oyuncuya dogal Turkce bir cevap ver. "
                + "Yeni bir oyun eylemi baslatma. "
                + "Yalnizca zorunlu JSON formatini dondur."
            )
        );

        return mesajlar;
    }

    private bool SilahCevabiVeriyleCeliskiliMi(
        string eylem,
        NPCIslemSonucu unitySonucu,
        string cevap,
        out string celiskiNedeni
    )
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

        string beklenenHasar = unitySonucu.currentDamage.ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );

        if (!Regex.IsMatch(
            normalCevap,
            @"(?<!\d)" + Regex.Escape(beklenenHasar) + @"(?!\d)"))
        {
            celiskiNedeni =
                "Cevap Unity tarafindan dogrulanan lazer hasarini belirtmiyor.";
            return true;
        }

        if (!normalCevap.Contains("hasar") &&
            !normalCevap.Contains("lazer") &&
            !normalCevap.Contains("silah"))
        {
            celiskiNedeni =
                "Cevapta sayisal degerin silah veya lazer hasari oldugu acik degil.";
            return true;
        }

        string[] hataliKarsilastirmalar =
        {
            @"\b100\s*(den|dan)\s*(daha\s*)?(yuksek|fazla|buyuk)\b",
            @"\b100\s*(un|in|nun|nin)\s*(uzerinde|ustunde|yukarida)\b",
            @"\b100\s*(u|yu|yi)\s*(asti|asiyor|geciyor|gecmis)\b",
            @"\b100\s*(ustune|uzerine)\s*(cikti|yukseldi)\b",
            @"\b(yuzden|yuzun)\s*(daha\s*)?(yuksek|fazla|buyuk|uzerinde)\b"
        };

        for (int i = 0; i < hataliKarsilastirmalar.Length; i++)
        {
            if (Regex.IsMatch(normalCevap, hataliKarsilastirmalar[i]))
            {
                celiskiNedeni =
                    "Cevap, 100 hasarin 100'den yuksek oldugunu iddia ediyor.";
                return true;
            }
        }

        return false;
    }

    private string SayisalCevabiDuzelt(
        string cevap,
        NPCIslemSonucu unitySonucu
    )
    {
        if (string.IsNullOrWhiteSpace(cevap) || unitySonucu == null)
            return cevap;

        string beklenenHasar = unitySonucu.currentDamage.ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );

        string sayi = Regex.Escape(beklenenHasar);
        string duzeltilmis = cevap;

        string sonucIleBitirenHatalar =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:den|dan)\s*(?:daha\s*)?"
            + @"(?:yüksek|fazla|büyük)\s*"
            + @"(?:bir\s*)?(?:seviyeye\s*)?(?:hale\s*)?"
            + @"(?:getirildi|geldi|çıktı|yükseldi|oldu)";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            sonucIleBitirenHatalar,
            beklenenHasar + "'e yükseltildi",
            RegexOptions.IgnoreCase
        );

        string karsilastirmaIleBitenHatalar =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:den|dan)\s*(?:daha\s*)?"
            + @"(?:yüksek|fazla|büyük)\b";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            karsilastirmaIleBitenHatalar,
            "tam " + beklenenHasar,
            RegexOptions.IgnoreCase
        );

        string ustundeHatalari =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:un|ün|ın|in)\s*"
            + @"(?:üzerinde|üstünde|yukarısında)";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            ustundeHatalari,
            "tam " + beklenenHasar,
            RegexOptions.IgnoreCase
        );

        string asmaHatalari =
            @"(?i)(?<!\d)" + sayi
            + @"\s*['’]?\s*(?:u|ü|ı|i)\s*"
            + @"(?:aştı|aşıyor|geçti|geçiyor)";

        duzeltilmis = Regex.Replace(
            duzeltilmis,
            asmaHatalari,
            beklenenHasar + "'e ulaştı",
            RegexOptions.IgnoreCase
        );

        return Regex.Replace(duzeltilmis, @"\s+", " ").Trim();
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

        sonuc = Regex.Replace(sonuc, @"\s+", " ").Trim();

        return sonuc;
    }

    private List<MesajVerisi> MesajlariHazirla(
        string oyuncuMesaji
    )
    {
        List<MesajVerisi> mesajlar =
            new List<MesajVerisi>();

        string sonSystemPrompt =
            NPCSystemPrompt.Prompt
            + "\n\n"
            + DiyalogTutarlilikKurallari;

        mesajlar.Add(
            new MesajVerisi(
                "system",
                sonSystemPrompt
            )
        );

        for (int i = 0;
             i < konusmaGecmisi.Count;
             i++)
        {
            mesajlar.Add(
                konusmaGecmisi[i]
            );
        }

        mesajlar.Add(
            new MesajVerisi(
                "user",
                oyuncuMesaji
            )
        );

        return mesajlar;
    }

    private bool BaglantiAyarlariGecerli(
        out string hataMesaji
    )
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
                }
            },

            required = new[]
            {
                "action",
                "response"
            },

            additionalProperties = false
        };
    }

    private NPCIslemSonucu EylemiGuvenliUygula(
        string eylem
    )
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

    private bool YanitAyrıstır(
        string json,
        out NPCYanit npcYanit
    )
    {
        npcYanit = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        string temizJson = json.Trim();

        if (!temizJson.StartsWith(
                "{",
                StringComparison.Ordinal
            ) ||
            !temizJson.EndsWith(
                "}",
                StringComparison.Ordinal
            ))
        {
            return false;
        }

        try
        {
            npcYanit =
                JsonUtility.FromJson<NPCYanit>(
                    temizJson
                );
        }
        catch (Exception hata)
        {
            Debug.LogWarning(
                "NPC JSON ayristirma hatasi: "
                + hata.Message
            );

            npcYanit = null;

            return false;
        }

        if (npcYanit == null ||
            string.IsNullOrWhiteSpace(npcYanit.action) ||
            string.IsNullOrWhiteSpace(npcYanit.response))
        {
            npcYanit = null;
            return false;
        }

        npcYanit.action =
            npcYanit.action.Trim().ToUpperInvariant();

        npcYanit.response =
            npcYanit.response.Trim();

        int cevapSiniri = Mathf.Max(
            50,
            maksimumCevapKarakterSayisi
        );

        if (npcYanit.response.Length > cevapSiniri)
        {
            npcYanit.response =
                npcYanit.response.Substring(
                    0,
                    cevapSiniri
                ).Trim();
        }

        switch (npcYanit.action)
        {
            case "HEAL":
            case "MAX_HEAL":
            case "BOOST_DAMAGE":
            case "BOSS_HINT":
            case "UNLOCK_DOOR":
            case "NONE":
                return true;

            default:
                npcYanit = null;
                return false;
        }
    }

    private void CevabiGosterVeKaydet(
        string oyuncuMesaji,
        NPCYanit npcYanit
    )
    {
        if (npcCevapMetni != null)
        {
            npcCevapMetni.text =
                npcYanit.response;
        }

        string kaydedilecekJson =
            JsonUtility.ToJson(npcYanit);

        konusmaGecmisi.Add(
            new MesajVerisi(
                "user",
                oyuncuMesaji
            )
        );

        konusmaGecmisi.Add(
            new MesajVerisi(
                "assistant",
                kaydedilecekJson
            )
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

    private void KullaniciyaHataGoster(
        string mesaj
    )
    {
        if (npcCevapMetni != null)
        {
            npcCevapMetni.text = mesaj;
        }
    }

    private string KisaMetin(
        string metin,
        int sinir
    )
    {
        if (string.IsNullOrEmpty(metin) ||
            metin.Length <= sinir)
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

        public MesajVerisi(
            string role,
            string content
        )
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
