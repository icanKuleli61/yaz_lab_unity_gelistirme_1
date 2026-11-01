using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BossSaldiri : MonoBehaviour
{
    [SerializeField] 
    private GameObject lazerPrefab;
    [SerializeField] 
    private Transform sagLazer;
    [SerializeField] 
    private Transform solLazer;

    [SerializeField] 
    private float saldiriMesafesi = 30f;
    [SerializeField] 
    private float atisAraligi = 2f;
    [SerializeField] 
    private float lazerHizi = 40f;

    [SerializeField] 
    private Transform oyuncu;
    [SerializeField] 
    private GemiYonetici gemiYonetici;

    private List<GameObject> aktifLazerler = new List<GameObject>();

    private Boss boss;
    private BossHareket bossHareket;
    private float sonrakiAtisZamani;

    private void Awake()
    {
        boss = GetComponent<Boss>();
        bossHareket = GetComponent<BossHareket>();
    }

    private void Update()
    {
        if (bossHareket == null || oyuncu == null)
            return;

        if (!bossHareket.SaldiriyaUgradi)
            return;

        float mesafe = Vector3.Distance(
            transform.position,
            oyuncu.position
        );

        if (mesafe > saldiriMesafesi)
            return;

        if (Time.time >= sonrakiAtisZamani)
        {
            LazerAt(sagLazer);
            LazerAt(solLazer);

            sonrakiAtisZamani = Time.time + atisAraligi;
        }
    }

    private void LazerAt(Transform cikisNoktasi)
    {
        if (cikisNoktasi == null || lazerPrefab == null)
            return;

        GameObject lazer = Instantiate(
            lazerPrefab,
            cikisNoktasi.position,
            cikisNoktasi.rotation
        );

        aktifLazerler.Add(lazer);

        StartCoroutine(LazerHareketEt(lazer));
    }

    private IEnumerator LazerHareketEt(GameObject lazer)
    {
        while (lazer != null && oyuncu != null)
        {
            Vector3 hedefKonum = oyuncu.position;

            Vector3 yon = hedefKonum - lazer.transform.position;

            if (yon.sqrMagnitude > 0.001f)
                lazer.transform.rotation = Quaternion.LookRotation(yon);

            lazer.transform.position = Vector3.MoveTowards(
                lazer.transform.position,
                hedefKonum,
                lazerHizi * Time.deltaTime
            );

            if (Vector3.Distance(lazer.transform.position,oyuncu.position) <= 0.3f)
            {
                if (gemiYonetici != null)
                    gemiYonetici.HasarVer(boss.Ozellikler.getHasar());

                Destroy(lazer);
                yield break;
            }

            yield return null;
        }

        if (lazer != null)
        {
            Destroy(lazer);
            aktifLazerler.Remove(lazer);
        }
    }

    public void LazerleriTemizle()
    {
        foreach (GameObject lazer in aktifLazerler)
        {
            if (lazer != null)
                Destroy(lazer);
        }

        aktifLazerler.Clear();
    }
}