using UnityEngine;

public class MiniHaritaKontrol : MonoBehaviour
{
    [SerializeField] private Transform oyuncuGemi;
    [SerializeField] private Transform zemin;
    [SerializeField] private Transform portal;

    [SerializeField] private RectTransform haritaArkaPlan;
    [SerializeField] private RectTransform oyuncuIkonu;
    [SerializeField] private RectTransform portalIkonu;

    private void LateUpdate()
    {
        if (zemin == null || haritaArkaPlan == null)
            return;

        Vector3 merkez = zemin.position;
        Vector3 boyut = zemin.lossyScale;

        float yariX = 5f * boyut.x;
        float yariZ = 5f * boyut.z;

        float minX = merkez.x - yariX;
        float maxX = merkez.x + yariX;
        float minZ = merkez.z - yariZ;
        float maxZ = merkez.z + yariZ;

        OyuncuIkonunuGuncelle(minX, maxX, minZ, maxZ);
        PortalIkonunuGuncelle(minX, maxX, minZ, maxZ);
    }

    private void OyuncuIkonunuGuncelle(
    float minX, float maxX, float minZ, float maxZ)
    {
        if (oyuncuGemi == null || oyuncuIkonu == null)
            return;

        Vector3 konum = oyuncuGemi.position;

        bool iceride =
            konum.x >= minX && konum.x <= maxX &&
            konum.z >= minZ && konum.z <= maxZ;

        oyuncuIkonu.gameObject.SetActive(iceride);

        if (!iceride)
            return;

        oyuncuIkonu.anchoredPosition =  KonumuHaritayaCevir(konum, minX, maxX, minZ, maxZ);

        float aci = -oyuncuGemi.eulerAngles.y + 180f;

        oyuncuIkonu.localRotation = Quaternion.Euler(0f, 0f, aci);
    }

    private void PortalIkonunuGuncelle(
        float minX, float maxX, float minZ, float maxZ)
    {
        if (portal == null || portalIkonu == null)
            return;

        Vector3 konum = portal.position;

        bool iceride =
            konum.x >= minX && konum.x <= maxX &&
            konum.z >= minZ && konum.z <= maxZ;

        portalIkonu.gameObject.SetActive(iceride);

        if (!iceride)
            return;

        portalIkonu.anchoredPosition =  KonumuHaritayaCevir(konum, minX, maxX, minZ, maxZ);
    }

    private Vector2 KonumuHaritayaCevir(
        Vector3 konum,
        float minX, float maxX,
        float minZ, float maxZ)
    {
        float xOran = Mathf.InverseLerp(minX, maxX, konum.x);
        float zOran = Mathf.InverseLerp(minZ, maxZ, konum.z);

        float x = (xOran - 0.5f) * haritaArkaPlan.rect.width;
        float y = (zOran - 0.5f) * haritaArkaPlan.rect.height;

        return new Vector2(x, y);
    }
}