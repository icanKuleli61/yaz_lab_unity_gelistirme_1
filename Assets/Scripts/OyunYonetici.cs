
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OyunYonetici : MonoBehaviour
{
    [SerializeField]
    private GameObject _patlamaEfekti;

    [SerializeField]
    private GameObject _gorevBasarisizPanel;

    [SerializeField]
    private Transform aktifGemiTransformu;

    [SerializeField]
    private Button oyundanCik;

    [SerializeField]
    private AudioClip _gemiPatlamaSesi;

    private bool _oyunBittiMi = false;

    public void GemiPatladi()
    {
        Debug.Log("TEST: GemiPatladi metodu calisti!");

        if (_oyunBittiMi)
        {
            Debug.Log("Oyun zaten bitmis.");
            return;
        }

        _oyunBittiMi = true;

        if (aktifGemiTransformu == null)
        {
            Debug.LogError("HATA: Aktif Gemi Transformu atanmamis!");
            return;
        }

        Vector3 gemiKonumu = aktifGemiTransformu.position;

        if (_gemiPatlamaSesi != null)
        {
            AudioSource.PlayClipAtPoint(
                _gemiPatlamaSesi,
                gemiKonumu
            );
        }

        if (_patlamaEfekti != null)
        {
            Instantiate(
                _patlamaEfekti,
                gemiKonumu,
                Quaternion.identity
            );
        }

        Destroy(aktifGemiTransformu.gameObject);

        Debug.Log("TEST: Basarisizlik paneli icin 3 saniye bekleniyor.");

        StartCoroutine(GorevBasarisizPaneliniAc());
    }

    private IEnumerator GorevBasarisizPaneliniAc()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("TEST: Basarisizlik paneli acilacak!");

        if (_gorevBasarisizPanel == null)
        {
            Debug.LogError(
                "HATA: Gorev Basarisiz Panel alani bos! " +
                "Inspector'dan GorevBasarisizPaneli nesnesini bagla."
            );
            yield break;
        }

        _gorevBasarisizPanel.SetActive(true);

        Debug.Log("TEST: Basarisizlik paneli etkinlestirildi.");
    }

    public void OyundanCik()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
