
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
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

    private bool cevapBekleniyor = false;

    private void OnMouseDown()
    {
        PaneliAc();
    }

    public void PaneliAc()
    {
        if (konusmaPaneli == null || npcCevapMetni == null)
            return;

        konusmaPaneli.SetActive(true);
        npcCevapMetni.text = "Merhaba yolcu! Benimle konusabilirsin.";
    }

   
    public void PaneliKapat()
    {
        if (konusmaPaneli != null)
            konusmaPaneli.SetActive(false);
    }

    
    public void MesajGonder()
    {
        if (cevapBekleniyor)
            return;

        if (oyuncuMesajAlani == null || npcCevapMetni == null)
            return;

        string mesaj = oyuncuMesajAlani.text.Trim();

        if (string.IsNullOrEmpty(mesaj))
            return;

        oyuncuMesajAlani.text = "";

        StartCoroutine(OllamaCevapAl(mesaj));
    }

  
    private IEnumerator OllamaCevapAl(string oyuncuMesaji)
    {
        cevapBekleniyor = true;
        npcCevapMetni.text = "NPC dusunuyor...";

        MesajVerisi mesajVerisi = new MesajVerisi(oyuncuMesaji);

        string json = "{\"model\":\"" + model +
                      "\",\"messages\":[" +
                      JsonUtility.ToJson(mesajVerisi) +
                      "],\"stream\":false}";

        byte[] veri = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest istek =
               new UnityWebRequest(ollamaURL, "POST"))
        {
            istek.uploadHandler = new UploadHandlerRaw(veri);
            istek.downloadHandler = new DownloadHandlerBuffer();
            istek.SetRequestHeader("Content-Type", "application/json");

            yield return istek.SendWebRequest();

            if (istek.result == UnityWebRequest.Result.Success)
            {
                OllamaCevap cevap =
                    JsonUtility.FromJson<OllamaCevap>(
                        istek.downloadHandler.text);

                if (cevap != null &&
                    cevap.message != null &&
                    !string.IsNullOrEmpty(cevap.message.content))
                {
                    npcCevapMetni.text = cevap.message.content;
                }
                else
                {
                    npcCevapMetni.text = "Cevabini anlayamadim.";
                }
            }
            else
            {
                npcCevapMetni.text =
                    "Su anda cevap veremiyorum. Lutfen tekrar dene.";

                Debug.LogError("Ollama hatasi: " + istek.error);
            }
        }

        cevapBekleniyor = false;
    }

   
    [System.Serializable]
    public class MesajVerisi
    {
        public string role = "user";
        public string content;

        public MesajVerisi(string metin)
        {
            content = metin;
        }
    }

   
    [System.Serializable]
    public class OllamaCevap
    {
        public Mesaj message;
    }

    [System.Serializable]
    public class Mesaj
    {
        public string role;
        public string content;
    }
}
