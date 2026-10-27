
using UnityEngine;
using UnityEngine.UI;

public class BotYardimPanelKontrol : MonoBehaviour
{
    [Header("Panel")]
    public GameObject yardimPaneli;

    [Header("Butonlar")]
    public Button evetButonu;
    public Button hayirButonu;

    [Header("Gorev")]
    public CompanionMission companionMission;

    private void Start()
    {
        if (yardimPaneli != null)
            yardimPaneli.SetActive(false);

        if (evetButonu != null)
            evetButonu.onClick.AddListener(EvetButonunaBasildi);

        if (hayirButonu != null)
            hayirButonu.onClick.AddListener(HayirButonunaBasildi);
    }

    public void PaneliAc()
    {
        if (yardimPaneli != null)
            yardimPaneli.SetActive(true);
    }

    public void EvetButonunaBasildi()
    {
        if (yardimPaneli != null)
            yardimPaneli.SetActive(false);

        if (companionMission != null)
            companionMission.BaslatSalterGorevi();
        else
            Debug.LogWarning("BotYardimPanelKontrol: CompanionMission atanmamis!");
    }

    public void HayirButonunaBasildi()
    {
        if (yardimPaneli != null)
            yardimPaneli.SetActive(false);
    }
}
