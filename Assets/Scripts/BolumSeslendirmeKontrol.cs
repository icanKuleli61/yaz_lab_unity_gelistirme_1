using UnityEngine;

public class BolumSeslendirmeKontrol : MonoBehaviour
{

    [SerializeField]
    private AudioSource ses_kaynagi;

    [SerializeField]
    private AudioClip calınacak_ses;

    private void Start()
    {
        if (calınacak_ses != null)
        {
            ses_kaynagi.PlayOneShot(calınacak_ses);
        }
    }
}