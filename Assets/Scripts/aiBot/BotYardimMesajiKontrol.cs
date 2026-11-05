
using UnityEngine;

public class BotYardimMesajiKontrol : MonoBehaviour
{
    [Header("Baglantilar")]
    public Transform bot;
    public Transform gemi;
    public GameObject mesaj;
    public Camera oyunKamerasi;
    public SalterKontrol salterKontrol;

    [Header("Gorunme Ayarlari")]
    public float gorunmeMesafesi = 8f;
    public Vector3 botUstuOfset = new Vector3(0f, 2f, 0f);

    private RectTransform mesajRect;

    private void Start()
    {
        mesajRect = mesaj != null
            ? mesaj.GetComponent<RectTransform>()
            : null;

        if (mesaj != null)
            mesaj.SetActive(false);

        if (oyunKamerasi == null)
            oyunKamerasi = Camera.main;
    }

    private void LateUpdate()
    {
        if (bot == null || gemi == null ||
            mesaj == null || mesajRect == null ||
            oyunKamerasi == null)
            return;

        if (salterKontrol != null && salterKontrol.aktifMi)
        {
            if (mesaj.activeSelf)
                mesaj.SetActive(false);

            return;
        }

        float mesafe = Vector3.Distance(gemi.position, bot.position);

        Vector3 ekranKonumu = oyunKamerasi.WorldToScreenPoint(
            bot.position + botUstuOfset
        );

        bool yakin = mesafe <= gorunmeMesafesi &&
                     ekranKonumu.z > 0f;

        if (mesaj.activeSelf != yakin)
            mesaj.SetActive(yakin);

        if (yakin)
            mesajRect.position = ekranKonumu;
    }
}
