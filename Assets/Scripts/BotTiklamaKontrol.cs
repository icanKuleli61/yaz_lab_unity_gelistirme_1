
using UnityEngine;
using UnityEngine.InputSystem;

public class BotTiklamaKontrol : MonoBehaviour
{
    public BotYardimPanelKontrol yardimPanelKontrol;
    public Camera oyunKamerasi;
    public SalterKontrol salterKontrol;

    private void Start()
    {
        if (oyunKamerasi == null)
            oyunKamerasi = Camera.main;
    }

    private void OnMouseDown()
    {
        // Salter aktifse tiklama islemi yapma.
        if (salterKontrol != null && salterKontrol.aktifMi)
            return;

        if (oyunKamerasi == null || yardimPanelKontrol == null)
            return;

        if (Mouse.current == null)
            return;

        Ray ray = oyunKamerasi.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) &&
            hit.transform == transform)
        {
            yardimPanelKontrol.PaneliAc();
        }
    }
}
