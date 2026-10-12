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

    private bool _oyunBittiMi = false;






    public void GemiPatladi()
    {
        if (_oyunBittiMi)
            return;

        _oyunBittiMi = true;

        Instantiate(
            _patlamaEfekti,
            aktifGemiTransformu.position,
            Quaternion.identity
        );

        Destroy(aktifGemiTransformu.gameObject);


        StartCoroutine(GorevBasarisizPaneliniAc());
    }

    private IEnumerator GorevBasarisizPaneliniAc()
    {
        yield return new WaitForSeconds(3f);

        _gorevBasarisizPanel.SetActive(true);
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
