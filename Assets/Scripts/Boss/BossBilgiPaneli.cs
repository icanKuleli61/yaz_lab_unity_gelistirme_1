using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossBilgiPaneli : MonoBehaviour
{
    [SerializeField] private Image bossAvatar;
    [SerializeField] private TMP_Text bossAdi;
    [SerializeField] private Image canBari;
    [SerializeField] private TMP_Text canDegeri;

    [SerializeField] 
    private Boss boss;

    private void OnEnable()
    {
        if (boss != null)
            boss.HasarAlindi += BilgileriGuncelle;
    }

    private void Start()
    {
        BilgileriGuncelle();
    }

    private void OnDisable()
    {
        if (boss != null)
            boss.HasarAlindi -= BilgileriGuncelle;
    }
    public void PaneliGoster()
    {
        gameObject.SetActive(true);
    }
    private void BilgileriGuncelle()
    {
        if (boss == null) return;

        float mevcutCan = boss.Ozellikler.getAnlik_boss_can();
        float maksimumCan = boss.Ozellikler.getMaxsimum_can();

        bossAdi.text = "Devolarium";

        canDegeri.text = $"{mevcutCan:0} / {maksimumCan:0} HP";

        canBari.fillAmount = maksimumCan > 0
            ? mevcutCan / maksimumCan
            : 0;
    }
}