
using UnityEngine;

public class SalterKontrol : MonoBehaviour
{
    [Header("Salter Durumu")]
    public bool aktifMi = false;

    [Header("Gorsel Ayarlar")]
    public Renderer salterRenderer;
    public Color aktifRengi = new Color(0.5f, 1f, 0.5f);

    [Header("Portal Ayarlari")]
    public Collider portalCollider;

    public void SalteriAktifEt()
    {
        if (aktifMi)
        {
            Debug.Log("Salter zaten aktif!");
            return;
        }

        aktifMi = true;

        // Salteri acik yesile boya.
        if (salterRenderer != null)
        {
            salterRenderer.material.color = aktifRengi;
        }
        else
        {
            Debug.LogWarning("Salter Renderer atanmamis!");
        }

        
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
            Debug.Log("Portal Collider kapatildi!");
        }
        else
        {
            Debug.LogWarning("Portal Collider atanmamis!");
        }

        Debug.Log("SALTER AKTIF EDILDI!");
    }
}
