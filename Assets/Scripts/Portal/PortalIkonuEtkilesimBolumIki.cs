
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalIkonuEtkilesimBolumIki : MonoBehaviour
{
    [SerializeField]
    private GameObject cerceve;

    [SerializeField]
    private GameObject yildiz;

    [SerializeField]
    private PortalAnimasyonKontrol portalAnimasyonKontrol;

    [SerializeField]
    private Boss boss;

    [SerializeField]
    private GameObject portalIkonu;

    [SerializeField]
    private GameObject oyunBittiPaneli;

    private float buyumeKatsayisi = 1.15f;
    private float donusHizi = 45f;

    private Vector3 normalOlcekCerceve;
    private Vector3 normalOlcekYildiz;

    private Coroutine animasyonCoroutine;

    private bool basiliTutuluyor;
    private float baslamaZamani;
    private bool bolumDegisti;
    private bool portalAcildi;

    private void Awake()
    {
        normalOlcekCerceve = cerceve.transform.localScale;
        normalOlcekYildiz = yildiz.transform.localScale;

        portalIkonu.SetActive(false);
        oyunBittiPaneli.SetActive(false);

        if (boss != null)
            boss.CanSifirOldu += PortalAc;
    }

    private void PortalAc()
    {
        portalAcildi = true;
        portalIkonu.SetActive(true);
        
        boss.CanSifirOldu -= PortalAc;

        Debug.Log("Boss öldü, portal açıldı!");
    }

    private void Update()
    {
        if (!portalAcildi || !basiliTutuluyor || bolumDegisti)
            return;

        if (Time.time - baslamaZamani >= 3f)
        {
            bolumDegisti = true;
            basiliTutuluyor = false;

            portalAnimasyonKontrol.animasyonEtkinDeğil();
            oyunBittiPaneli.SetActive(true);

        }
    }

    private void OnMouseDown()
    {
        if (!portalAcildi)
            return;

        basiliTutuluyor = true;
        baslamaZamani = Time.time;

        portalAnimasyonKontrol.animasyonEtkin();
    }

    private void OnMouseUp()
    {
        basiliTutuluyor = false;
        portalAnimasyonKontrol.animasyonEtkinDeğil();
    }

    private void OnMouseEnter()
    {
        if (!portalAcildi)
            return;

        MouseCerceveyeGeldi();
    }

    private void OnMouseExit()
    {
        MouseCercevedenGitti();
    }

    private void MouseCerceveyeGeldi()
    {
        if (!portalAcildi)
            return;

        cerceve.transform.localScale =
            normalOlcekCerceve * buyumeKatsayisi;

        yildiz.transform.localScale =
            normalOlcekYildiz * buyumeKatsayisi;

        if (animasyonCoroutine == null)
            animasyonCoroutine = StartCoroutine(Animasyon());
    }

    private void MouseCercevedenGitti()
    {
        cerceve.transform.localScale = normalOlcekCerceve;
        yildiz.transform.localScale = normalOlcekYildiz;

        if (animasyonCoroutine != null)
        {
            StopCoroutine(animasyonCoroutine);
            animasyonCoroutine = null;
        }
    }

    private IEnumerator Animasyon()
    {
        while (true)
        {
            yildizIkonuAnimasyon();
            yield return null;
        }
    }

    private void yildizIkonuAnimasyon()
    {
        yildiz.transform.Rotate(
            0f, 0f, -donusHizi * Time.deltaTime
        );
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.CanSifirOldu -= PortalAc;
    }

    public void TamamButonunaBasildi()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
