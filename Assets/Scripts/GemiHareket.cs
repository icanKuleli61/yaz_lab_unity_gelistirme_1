using UnityEngine;


public class GemiHareket : MonoBehaviour
{
    private GemiData gemiData;

    [SerializeField]
    private Transform gemiUcu;

    private void Awake()
    {
        gemiData = new GemiData(
            "Solace",
            15f,  
            210f, 
            100f, 
            100f, 
            20f   
        );

        if (gemiUcu == null)
        {
            Transform bulunanUcu = transform.Find("GemiUcu");
            if (bulunanUcu != null)
            {
                gemiUcu = bulunanUcu;
            }
            else
            {
                Debug.LogError(
                    "GemiUcu bulunamadı! Inspector'dan referansını bağla.",
                    this
                );
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void HareketEt(Vector3 hedefKonum)
    {
        Vector3 gidilecekKonumMesafe = hedefKonum - transform.position;

        gidilecekKonumMesafe.y = 0f;

        if (gidilecekKonumMesafe.sqrMagnitude < 0.01f)
            return;

        Vector3 mevcutBurunYonu = gemiUcu.forward;

        mevcutBurunYonu.y = 0f;

        Quaternion hedefRotasyon = Quaternion.FromToRotation(
            mevcutBurunYonu,
            gidilecekKonumMesafe
        ) * transform.rotation;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            hedefRotasyon,
            gemiData.GetDonusHizi() * Time.deltaTime
        );

        transform.position += gemiUcu.forward
                    * gemiData.GetGemiHizi()
                    * Time.deltaTime;
    }
}
