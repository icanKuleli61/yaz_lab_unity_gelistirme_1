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
            return;
        }

        aktifMi = true;

        if (salterRenderer != null)
        {
            salterRenderer.material.color = aktifRengi;
        }

        if (EnerjiKalkani != null)
        {
            EnerjiKalkani.SetActive(false);
        }

        if (portalCollider != null)
        {
            portalCollider.enabled = false;
        }

        if (portalBilgiPaneli != null)
        {
            portalBilgiPaneli.SetActive(false);
        }
    }
}