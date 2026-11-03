
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
    [SerializeField] private int zamanAsimiSaniye = 60;
    [SerializeField] private int maksimumGecmisMesajSayisi = 12;

    [Header("NPC Eylem Sistemi")]
    [SerializeField] private NPCEylemYoneticisi eylemYoneticisi;

    private bool cevapBekleniyor = false;

    private List<MesajVerisi> konusmaGecmisi =
        new List<MesajVerisi>();

    private void OnMouseDown()
    {
        PaneliAc();
    }

    public void PaneliAc()
    {
        if (konusmaPaneli == null || npcCevapMetni == null)
            return;

        konusmaPaneli.SetActive(true);

        if (konusmaGecmisi.Count == 0)
        {
            npcCevapMetni.text =
                "Merhaba yolcu! Benimle konusabilirsin.";
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
            return;

        string mesaj = oyuncuMesajAlani.text.Trim();

        if (string.IsNullOrWhiteSpace(mesaj))
            return;

        oyuncuMesajAlani.text = "";

        StartCoroutine(OllamaCevapAl(mesaj));
    }

    private IEnumerator OllamaCevapAl(string oyuncuMesaji)
    {
        cevapBekleniyor = true;

        npcCevapMetni.text = "NPC dusunuyor...";

        List<MesajVerisi> gonderilecekMesajlar =
            new List<MesajVerisi>();

        // SYSTEM PROMPT
        gonderilecekMesajlar.Add(
            new MesajVerisi(
                "system",
                NPCSystemPrompt.Prompt
            )
        );

        // ONCEKI KONUSMALAR
        foreach (MesajVerisi eskiMesaj in konusmaGecmisi)
        {
            gonderilecekMesajlar.Add(eskiMesaj);
        }

        // YENI OYUNCU MESAJI
        gonderilecekMesajlar.Add(
            new MesajVerisi("user", oyuncuMesaji)
        );

        // OLLAMA ISTEGI
        OllamaIstek istekVerisi = new OllamaIstek();

        istekVerisi.model = model;
        istekVerisi.messages = gonderilecekMesajlar;
        istekVerisi.stream = false;
        istekVerisi.format = "json";

        string json = JsonUtility.ToJson(istekVerisi);

        byte[] veri = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest istek =
               new UnityWebRequest(ollamaURL, "POST"))
        {
            istek.uploadHandler = new UploadHandlerRaw(veri);
            istek.downloadHandler = new DownloadHandlerBuffer();

            istek.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            istek.SetRequestHeader(
                "Accept",
                "application/json"
            );

            istek.timeout = Mathf.Max(1, zamanAsimiSaniye);

            yield return istek.SendWebRequest();

            if (istek.result == UnityWebRequest.Result.Success)
            {
                string sunucuCevabi =
                    istek.downloadHandler.text;

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
                    Debug.LogError(
                        "Ollama cevap JSON hatasi: " + hata.Message
                    );
                }

                if (ollamaCevap == null ||
                    ollamaCevap.message == null ||
                    string.IsNullOrWhiteSpace(
                        ollamaCevap.message.content))
                {
                    npcCevapMetni.text =
                        "Gelen cevabi anlayamadim. Tekrar deneyebilir misin?";

                    Debug.LogError(
                        "Ollama bos veya gecersiz cevap dondurdu."
                    );
                }
                else
                {
                    string modelJson =
                        ollamaCevap.message.content.Trim();

                    NPCYanit npcYanit;

                    if (YanitAyrıştır(modelJson, out npcYanit))
                    {
                        bool eylemBasarili = false;

                        if (eylemYoneticisi != null)
                        {
                            eylemBasarili =
                                eylemYoneticisi.EylemiGerceklestir(
                                    npcYanit.action
                                );
                        }
                        else
                        {
                            Debug.LogError(
                                "NPCEylemYoneticisi atanmamis!"
                            );
                        }

                        Debug.Log(
                            "NPC JSON Eylemi: " + npcYanit.action
                        );

                        if (eylemBasarili)
                        {
                            npcCevapMetni.text = npcYanit.response;

                            // BASARILI KONUSMAYI HAFIZAYA EKLE
                            konusmaGecmisi.Add(
                                new MesajVerisi(
                                    "user",
                                    oyuncuMesaji
                                )
                            );

                            konusmaGecmisi.Add(
                                new MesajVerisi(
                                    "assistant",
                                    modelJson
                                )
                            );

                            GecmisiSinirla();
                        }
                        else
                        {
                            npcCevapMetni.text =
                                "Bu islemi su anda gerceklestiremiyorum. "
                                + "Lutfen tekrar dene.";
                        }
                    }
                    else
                    {
                        npcCevapMetni.text =
                            "Cevabimi duzgun olusturamadim. "
                            + "Tekrar dener misin?";

                        Debug.LogError(
                            "Gecersiz NPC JSON: " + modelJson
                        );
                    }
                }
            }
            else
            {
                npcCevapMetni.text =
                    "Su anda istasyon baglantisina ulasamiyorum. "
                    + "Lutfen tekrar dene.";

                Debug.LogError(
                    "Ollama baglanti hatasi: "
                    + istek.error
                    + " | HTTP kodu: "
                    + istek.responseCode
                );
            }
        }

        cevapBekleniyor = false;
    }

    // OLLAMA JSON CEVABINI KONTROL ET
    private bool YanitAyrıştır(
        string json,
        out NPCYanit npcYanit)
    {
        npcYanit = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            npcYanit = JsonUtility.FromJson<NPCYanit>(json);

            if (npcYanit == null)
                return false;

            if (string.IsNullOrWhiteSpace(npcYanit.action))
                return false;

            if (string.IsNullOrWhiteSpace(npcYanit.response))
                return false;

            npcYanit.action =
                npcYanit.action.Trim().ToUpperInvariant();

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
                    return false;
            }
        }
        catch (Exception hata)
        {
            Debug.LogError(
                "NPC JSON ayristirma hatasi: " + hata.Message
            );

            return false;
        }
    }

    // KONUSMA GECMISININ SINIRSIZ BUYUMESINI ENGELLE
    private void GecmisiSinirla()
    {
        int maksimumMesaj = Mathf.Max(
            2,
            maksimumGecmisMesajSayisi
        );

        if (maksimumMesaj % 2 != 0)
            maksimumMesaj--;

        while (konusmaGecmisi.Count > maksimumMesaj)
        {
            konusmaGecmisi.RemoveRange(0, 2);
        }
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
    public class OllamaIstek
    {
        public string model;
        public List<MesajVerisi> messages;
        public bool stream;
        public string format;
    }

    [Serializable]
    public class OllamaCevap
    {
        public Mesaj message;
    }

    [Serializable]
    public class Mesaj
    {
        public string role;
        public string content;
    }
}
