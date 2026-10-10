using UnityEngine;

public class GüvenliAlanKontrol : MonoBehaviour
{
    [SerializeField]
    private Renderer _zeminAlanBilgileri;

    [SerializeField]
    private Transform _gemiAnlıkKonumu;

    [SerializeField]
    private GemiYonetici _aktifGemi;

    [SerializeField] 
    private float _hasarMiktari = 50f;

    [SerializeField] 
    private float _hasarAraligi = 1f;

    private void Awake()
    {
        
    }
    void Start()
    {
        
    }


    void Update()
    {

        
    }
}
