using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OyuncuPanelUi : MonoBehaviour
{

    [SerializeField]
    private Image canBari;

    [SerializeField]
    private TMP_Text canDegeriText;

    public void CanGuncelle(float mevcutCan, float maksimumCan)
    {
        float canOrani = maksimumCan > 0
            ? Mathf.Clamp01(mevcutCan / maksimumCan)
            : 0f;

        canBari.fillAmount = canOrani;

        canDegeriText.text =
            $"{mevcutCan:0} / {maksimumCan:0} HP";
    }
}
