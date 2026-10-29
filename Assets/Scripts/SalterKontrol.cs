
using UnityEngine;

public class SalterKontrol : MonoBehaviour
{
    [Header("Salter Durumu")]
    public bool aktifMi = false;

    [Header("Gorsel Ayarlar")]
    public Renderer salterRenderer;
    public Color aktifRengi = new Color(0.5f, 1f, 0.5f);

    [Header("Portal Kalkan Ayarlari")]
    public GameObject EnerjiKalkani;

    [Header("Portal Ayarlari")]
    public Collider portalCollider;
    public GameObject portalBilgiPaneli;

    public void SalteriAktifEt()
    {
        if (aktifMi)
        {
            Debug.Log("Salter zaten aktif!");
            return;
        }

        aktifMi = true;

        // Salterin rengini degistir.
        if (salterRenderer != null)
        {
            salterRenderer.material.color = aktifRengi;
        }
        else
        {
            Debug.LogWarning("Salter Renderer atanmamis!");
        }

        // Portal kalkanini koyu yesile cevir.
        if (EnerjiKalkani != null)
        {
            EnerjiKalkani.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Portal kalkan Renderer atanmamis!");
        }

        // Portalin colliderini kapat.
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
            Debug.Log("Portal Collider kapatildi!");
        }
        else
        {
            Debug.LogWarning("Portal Collider atanmamis!");
        }

        // Portal bilgi panelini gizle.
        if (portalBilgiPaneli != null)
        {
            portalBilgiPaneli.SetActive(false);
        }

        Debug.Log("SALTER AKTIF EDILDI!");
    }
}
