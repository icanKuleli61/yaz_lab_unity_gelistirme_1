using UnityEngine;

public class PortalAnimasyonKontrol : MonoBehaviour
{
    [SerializeField] private GameObject portal_animasyon;
    [SerializeField] private Transform efektNoktasi;

    private GameObject olusanEfekt;

    public void animasyonEtkin()
    {
        if (olusanEfekt != null) return;

        olusanEfekt = Instantiate(
            portal_animasyon,
            efektNoktasi.position,
            efektNoktasi.rotation
        );

        Debug.Log("Efekt oluşturuldu: " + olusanEfekt.name);
    }

    public void animasyonEtkinDeğil()
    {
        if (olusanEfekt == null) return;

        Destroy(olusanEfekt);
        olusanEfekt = null;
    }
}