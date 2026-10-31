
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OyuncuSaldiri : MonoBehaviour
{
    [Header("Lazer Ayarlari")]
    [SerializeField] private GameObject lazerPrefab;
    [SerializeField] private Transform solLazerNoktasi;
    [SerializeField] private Transform sagLazerNoktasi;



    [Header("Saldiri Ayarlari")]
    [SerializeField] private float saldiriMesafesi = 20f;
    [SerializeField] private float atisAraligi = 2f;
    [SerializeField] private float lazerHizi = 40f;

    [SerializeField] private GemiHareket gemiHareket;

    [SerializeField] private GemiYonetici gemiYonetici;

    private GemiData gemiData;

    private BossSecim seciliBoss;
    private BossHareket bossHareket;

    private bool saldiriAktif;
    private float sonrakiAtisZamani;
    private float sonrakiMesafeLogZamani;
    private void Start()
    {
        gemiData = gemiYonetici.GetAktifGemiyi().getGemiVerileri();
    }
    public void HedefAta(BossSecim yeniBoss)
    {
        seciliBoss = yeniBoss;

        bossHareket = seciliBoss != null
            ? seciliBoss.GetComponentInParent<BossHareket>()
            : null;

        saldiriAktif = false;
        gemiHareket.SaldiriDurumuAyarla(false);

        Debug.Log("Hedef: " +
            (seciliBoss != null ? seciliBoss.name : "Yok"));

        Debug.Log("BossHareket bulundu mu: " +
            (bossHareket != null));
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // CTRL ile saldiriyi ac / kapat
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            if (!saldiriAktif)
            {
                if (seciliBoss == null || bossHareket == null)
                {
                    Debug.LogWarning("Once bir boss secmelisin!");
                    return;
                }

                if (lazerPrefab == null ||
                    solLazerNoktasi == null ||
                    sagLazerNoktasi == null)
                {
                    Debug.LogError("Lazer Inspector baglantilari eksik!");
                    return;
                }

                saldiriAktif = true;
                sonrakiAtisZamani = 0f;

                gemiHareket.SaldiriDurumuAyarla(
                    true,
                    seciliBoss.transform
                );

                Debug.Log("Saldiri baslatildi.");
            }
            else
            {
                saldiriAktif = false;
                gemiHareket.SaldiriDurumuAyarla(false);

                Debug.Log("Saldiri durduruldu.");
            }
        }

        if (!saldiriAktif)
            return;

        if (seciliBoss == null || bossHareket == null)
        {
            saldiriAktif = false;
            gemiHareket.SaldiriDurumuAyarla(false);
            return;
        }

        // Saldiri boyunca geminin burnunu boss'a cevir.


        // Oyuncu ile boss arasindaki mesafe.
        float mesafe = Vector3.Distance(
            transform.position,
            seciliBoss.transform.position
        );

        // Menzil disindaysa ates etme.
        if (mesafe > saldiriMesafesi)
        {
            if (Time.time >= sonrakiMesafeLogZamani)
            {
                Debug.Log("Menzil disinda! Mesafe: " +
                    mesafe + " / Menzil: " + saldiriMesafesi);

                sonrakiMesafeLogZamani = Time.time + 1f;
            }

            return;
        }

        // Atis zamani geldiyse iki lazeri olustur.
        if (Time.time >= sonrakiAtisZamani)
        {
            LazerAt(solLazerNoktasi);
            LazerAt(sagLazerNoktasi);

            sonrakiAtisZamani = Time.time + atisAraligi;
        }
    }
    private void LazerAt(Transform cikisNoktasi)
    {
        if (seciliBoss == null || lazerPrefab == null)
            return;

        Transform hedef = seciliBoss.transform;
        BossHareket hedefBossHareket = bossHareket;

        Vector3 yon = hedef.position - cikisNoktasi.position;

        Quaternion rotasyon = yon.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(yon)
            : cikisNoktasi.rotation;

        GameObject lazer = Instantiate(
            lazerPrefab,
            cikisNoktasi.position,
            rotasyon
        );

        lazer.SetActive(true);

        Debug.Log("LAZER OLUSTURULDU: " + cikisNoktasi.name);

        StartCoroutine(
            LazerHareketEt(lazer, hedef, hedefBossHareket)
        );
    }

    private IEnumerator LazerHareketEt(
        GameObject lazer,
        Transform hedef,
        BossHareket hedefBossHareket)
    {
        while (lazer != null && hedef != null)
        {
            Vector3 hedefKonum = hedef.position;

            Vector3 yon = hedefKonum - lazer.transform.position;

            if (yon.sqrMagnitude > 0.001f)
            {
                lazer.transform.rotation = Quaternion.LookRotation(yon);
            }

            lazer.transform.position = Vector3.MoveTowards(
                lazer.transform.position,
                hedefKonum,
                lazerHizi * Time.deltaTime
            );

            if (Vector3.Distance(
                lazer.transform.position,
                hedefKonum) <= 0.3f)
            {
                if (hedefBossHareket != null)
                {
                    float oyuncuHasari = gemiData.GetHasar();

                    hedefBossHareket.HasarAl(oyuncuHasari);

                    Debug.Log("Boss hasar aldi: " + oyuncuHasari);
                }

                Destroy(lazer);
                yield break;
            }

            yield return null;
        }

        if (lazer != null)
            Destroy(lazer);
    }
}
