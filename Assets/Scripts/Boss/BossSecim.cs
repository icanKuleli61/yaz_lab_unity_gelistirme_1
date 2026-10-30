using UnityEngine;

public class BossSecim : MonoBehaviour
{
    [SerializeField] private GameObject hedefTahtasi;

    private void Start()
    {
        hedefTahtasi.SetActive(false);
    }

    public void SecimiGoster()
    {
        hedefTahtasi.SetActive(true);
    }

    private void Update()
    {
        if (hedefTahtasi.activeSelf)
        {
            hedefTahtasi.transform.Rotate(
                0,
                0,
                10 * Time.deltaTime
            );
        }
    }
    public void SecimKaldir()
    {
        hedefTahtasi.SetActive(false);
    }
}