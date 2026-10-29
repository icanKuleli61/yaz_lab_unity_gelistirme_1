using System.Collections;
using UnityEngine;

public class PortalIkonuEtkilesim : MonoBehaviour
{
    [SerializeField]
    private GameObject cerceve;

    [SerializeField]
    private GameObject yildiz;

    private float buyumeKatsayisi = 1.15f;

    private float donusHizi = 45f;

    private Vector3 normalOlcekCerceve;

    private Vector3 normalOlcekYildiz;

    private Coroutine animasyonCoroutine;

    private void Awake()
    {
        normalOlcekCerceve = cerceve.transform.localScale;
        normalOlcekYildiz = yildiz.transform.localScale;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnMouseDown()
    {
        
    }

    private void OnMouseUp()
    {
        
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
