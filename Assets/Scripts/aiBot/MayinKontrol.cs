using UnityEngine;

public class MayinKontrol : MonoBehaviour
{

    [SerializeField]
    private GameObject _patlamaEfekti;


    [SerializeField]
    private GemiYonetici _aktifGemiYonetici;

    [SerializeField]
    private AudioClip _patlamaSesi;

    [SerializeField]
    private float _hasarMiktari = 450f;

    private void LateUpdate()
    {
        transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.gameObject.CompareTag("Gemi"))
        {
            hasarVer();

            if (_patlamaEfekti != null)
            {
                Instantiate(
                    _patlamaEfekti,
                    transform.position,
                    Quaternion.identity
                );
            }

            if (_patlamaSesi != null)
            {
                AudioSource.PlayClipAtPoint(
                    _patlamaSesi,
                    transform.position,
                    1f
                );
            }

            Destroy(gameObject);
        }
    }

    private void hasarVer()
    {
        _aktifGemiYonetici.HasarVer(_hasarMiktari);
    }



}
