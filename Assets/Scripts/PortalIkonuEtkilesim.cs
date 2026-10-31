using System.Collections;
using UnityEngine;

public class PortalIkonuEtkilesim : MonoBehaviour
{
    [SerializeField]
    private GameObject cerceve;

    [SerializeField]
    private GameObject yildiz;

    [SerializeField]
    private PortalAnimasyonKontrol portalAnimasyonKontrol;

    private float buyumeKatsayisi = 1.15f;

    private float donusHizi = 45f;

    private Vector3 normalOlcekCerceve;

    private Vector3 normalOlcekYildiz;

    private Coroutine animasyonCoroutine;

    private bool basiliTutuluyor;
    private float baslamaZamani;
    private bool bolumDegisti;

    private bool efektCalisiyor = false;
    private void Awake()
    {
        normalOlcekCerceve = cerceve.transform.localScale;
        normalOlcekYildiz = yildiz.transform.localScale;
    }

    private void Update()
    {
        if (!basiliTutuluyor || bolumDegisti)
            return;

        if (Time.time - baslamaZamani >= 3f)
        {
            bolumDegisti = true;

            UnityEngine.SceneManagement.SceneManager.LoadScene("Bolum2");
        }
    }
    private void OnMouseDown()
    {
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
        MouseCerceveyeGeldi();
        
    }

    private void OnMouseExit()
    {

        MouseCercevedenGitti();


    }

    private void MouseCerceveyeGeldi()
    {
        cerceve.transform.localScale =
            normalOlcekCerceve * buyumeKatsayisi;

        yildiz.transform.localScale =
            normalOlcekYildiz * buyumeKatsayisi;

        if (animasyonCoroutine == null)
        {
            animasyonCoroutine = StartCoroutine(Animasyon());

        }
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
        yildiz.transform.Rotate(0, 0, -donusHizi * Time.deltaTime);
    }
}
