using UnityEngine;

public class BossPatlama : MonoBehaviour
{
    [SerializeField] 
    private GameObject patlamaEfekti;
    private Boss boss;

    private void Awake()
    {
        boss = GetComponent<Boss>();

        if (boss != null)
        {
            boss.CanSifirOldu += PatlamaOlustur;
        }
    }

    private void PatlamaOlustur()
    {
        Instantiate(patlamaEfekti, transform.position,  transform.rotation);
        GetComponent<BossSaldiri>()?.LazerleriTemizle();
        boss.CanSifirOldu -= PatlamaOlustur;
        Destroy(gameObject);


    }
}
