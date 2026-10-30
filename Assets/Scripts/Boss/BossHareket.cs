
using UnityEngine;
using UnityEngine.InputSystem;

public class BossHareket : MonoBehaviour
{

    [SerializeField] private Transform oyuncu;


    [SerializeField] private Transform bosPoint;


    [SerializeField] 
    private float hareketHizi = 9f;

    private float durmaMesafesi = 15f;

    private float takipBirakmaMesafesi = 40f;
    private bool eveDonuyor;
    private Vector3 baslangicKonumu;
    private Quaternion baslangicRotasyonu;

    [Header("Test")]
    [SerializeField] private float testHasari = 50f;

    private Boss boss;

    private bool saldiriyaUgradi;

    private void Awake()
    {
        boss = new Devolarium();
        boss.HasarAlindi += HasarAlindiginda;
    }

    private void Start()
    {
        if (oyuncu == null || bosPoint == null)
        {
            enabled = false;
            return;
        }

        baslangicKonumu = transform.position;
        baslangicRotasyonu = transform.rotation;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.hKey.wasPressedThisFrame)
        {
            HasarAl(testHasari);
        }

        if (eveDonuyor)
        {
            BaslangicKonumunaDon();
            return;
        }

        if (saldiriyaUgradi)
        {
            float mesafe = Vector3.Distance(
                transform.position,
                oyuncu.position
            );

            if (mesafe >= takipBirakmaMesafesi)
            {
                eveDonuyor = true;
                saldiriyaUgradi = false;
                return;
            }

            HareketEt();
        }
    }

    private void BaslangicKonumunaDon()
    {
        Vector3 hedefYon = baslangicKonumu - transform.position;
        hedefYon.y = 0f;

        float mesafe = hedefYon.magnitude;

        if (mesafe > 0.1f)
        {
            Vector3 mevcutBurunYonu = bosPoint.forward;
            mevcutBurunYonu.y = 0f;

            Quaternion hedefRotasyon = Quaternion.FromToRotation(
                mevcutBurunYonu,
                hedefYon
            ) * transform.rotation;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                hedefRotasyon,
                240f * Time.deltaTime
            );

            transform.position = Vector3.MoveTowards(
                transform.position,
                baslangicKonumu,
                hareketHizi * Time.deltaTime
            );

            return;
        }

        transform.position = baslangicKonumu;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            baslangicRotasyonu,
            240f * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, baslangicRotasyonu) < 0.5f)
        {
            transform.rotation = baslangicRotasyonu;
            eveDonuyor = false;
            saldiriyaUgradi = false;
        }
    }


    private void HareketEt()
    {
        if (oyuncu == null || bosPoint == null)
            return;

        Vector3 hedefYon = oyuncu.position - transform.position;
        hedefYon.y = 0f;

        float mesafe = hedefYon.magnitude;

        if (mesafe < 0.001f)
            return;

        hedefYon.Normalize();

        Vector3 mevcutBurunYonu = bosPoint.forward;
        mevcutBurunYonu.y = 0f;

        if (mevcutBurunYonu.sqrMagnitude < 0.001f)
            return;

        mevcutBurunYonu.Normalize();

        Quaternion hedefRotasyon = Quaternion.FromToRotation(
            mevcutBurunYonu,
            hedefYon
        ) * transform.rotation;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            hedefRotasyon,
            240f * Time.deltaTime
        );

        if (mesafe <= durmaMesafesi)
            return;

        Vector3 hareketYonu = bosPoint.forward;
        hareketYonu.y = 0f;

        hareketYonu.Normalize();

        transform.position += hareketYonu *
                              hareketHizi *
                              Time.deltaTime;
    }

    private void HasarAlindiginda()
    {
        saldiriyaUgradi = true;

        Debug.Log("Devolarium aktif oldu!");
    }

    public void HasarAl(float hasar)
    {
        boss.hasarAl(hasar);
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.HasarAlindi -= HasarAlindiginda;
    }
}
