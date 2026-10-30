using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OyuncuKontrol : MonoBehaviour
{

    [SerializeField]
    private GemiHareket gemiHareket ;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private BossBilgiPaneli bossBilgiPaneli;

    [SerializeField]
    private OyuncuSaldiri oyuncuSaldiri;
    private BossSecim seciliBoss;

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current != null &&
        EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 ekranTiklamaKonumu = Mouse.current.position.ReadValue();
            Ray isin = camera.ScreenPointToRay(ekranTiklamaKonumu);


            if (Physics.Raycast(isin, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("PortalIkonu"))
                {
                    return; 
                }
            }

            Plane hareketDuzlemi = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (hareketDuzlemi.Raycast(isin, out float uzaklik))
            {
                Vector3 hedefKonum = isin.GetPoint(uzaklik);
                gemiHareket.HareketEt(hedefKonum);
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 tiklamaKonumu = Mouse.current.position.ReadValue();

            Ray isin = camera.ScreenPointToRay(tiklamaKonumu);

            if (Physics.Raycast(isin, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Boss"))
                {
                    BossSecim boss = hit.collider.GetComponentInParent<BossSecim>();

                    if (boss != null)
                    {
                        HedefSec(boss);
                    }
                }
            }
        }

    }

    private void HedefSec(BossSecim yeniBoss)
    {
        if (seciliBoss != null)
            seciliBoss.SecimKaldir();

        seciliBoss = yeniBoss;

        seciliBoss.SecimiGoster();
        bossBilgiPaneli.PaneliGoster();
        oyuncuSaldiri.HedefAta(seciliBoss);
        Debug.Log(seciliBoss.name + " seçildi!");
    }
}
