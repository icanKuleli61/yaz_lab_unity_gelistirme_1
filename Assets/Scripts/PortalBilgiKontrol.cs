
using UnityEngine;

public class PortalBilgiKontrol : MonoBehaviour
{
    [SerializeField] private GameObject portalBilgiPaneli;
    [SerializeField] private SalterKontrol salterKontrol;

    private void Start()
    {
        if (portalBilgiPaneli != null)
        {
            portalBilgiPaneli.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Gemi"))
            return;

        // Salter aktifse bilgilendirme panelini tekrar acma.
        if (salterKontrol != null && salterKontrol.aktifMi)
        {
            if (portalBilgiPaneli != null)
                portalBilgiPaneli.SetActive(false);

            return;
        }

        if (portalBilgiPaneli != null)
            portalBilgiPaneli.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Gemi"))
            return;

        if (portalBilgiPaneli != null)
            portalBilgiPaneli.SetActive(false);
    }

    public void PaneliKapat()
    {
        if (portalBilgiPaneli != null)
            portalBilgiPaneli.SetActive(false);
    }
}
