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
    private AudioClip _uyariSesi;

    [SerializeField]
    private AudioSource _uyariSesKaynagi;

    private float _hasarMiktari = 200f;

    private float _hasarAraligi = 1f;

    private float _uyariSesAraligi = 10f;

    private float _sonrakiUyariSesZamani;

    private float _sonrakiHasarZamani;

    private void Awake()
    {
        
    }
    void Start()
    {
        
    }


    private void Update()
    {
        if (!SerizableInitilazeEdildiMi())
            return;


        if (!GemiGuvenliAlandaMi() && Time.time >= _sonrakiUyariSesZamani)
        {
            _uyariSesKaynagi.PlayOneShot(_uyariSesi);

            _sonrakiUyariSesZamani =
                Time.time + _uyariSesAraligi;
        }

        if (!GemiGuvenliAlandaMi() &&
            Time.time >= _sonrakiHasarZamani)
        {
            GuvenliAlanDegilseHasarVer();
        }
    }



    private bool SerizableInitilazeEdildiMi()
    {
        if(_zeminAlanBilgileri == null || _gemiAnlıkKonumu == null || _aktifGemi == null )
        {
            return false;
        }
        return true;
    }
    private bool GemiGuvenliAlandaMi()
    {
        Bounds sinirlar = _zeminAlanBilgileri.bounds;
        Vector3 konum = _gemiAnlıkKonumu.position;

        if (konum.x >= sinirlar.min.x && konum.x <= sinirlar.max.x &&
            konum.z >= sinirlar.min.z && konum.z <= sinirlar.max.z)
        {
            return true;
        }
        return false;
    }

    private void GuvenliAlanDegilseHasarVer()
    {
        Gemi aktifGemi = _aktifGemi.GetAktifGemiyi();
        aktifGemi.HasarAl(_hasarMiktari);
        _aktifGemi.GemiDurumunuKontrolEt();
        _sonrakiHasarZamani = Time.time + _hasarAraligi;
    }
}
