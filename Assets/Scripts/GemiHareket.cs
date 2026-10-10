using UnityEngine;


public class GemiHareket : MonoBehaviour
{

    private Gemi aktifGemi;
    private GemiData gemiData;

    [SerializeField]
    private Transform gemiUcu;

    public void GemiAta(Gemi aktifGemi)
    {
        if (aktifGemi == null)
        {
            throw new System.Exception("Gemi atanmadı");
        }

        this.aktifGemi = aktifGemi;
        gemiData = this.aktifGemi.getGemiVerileri();
               
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private bool IsEngelVarmi()
    {
        Vector3 hareketYonu = gemiUcu.forward;
        hareketYonu.y = 0f;
        hareketYonu.Normalize();

        float kontrolMesafesi = gemiData.GetGemiHizi() * Time.deltaTime;

        if (Physics.SphereCast(
            gemiUcu.position,
            0.5f,
            hareketYonu,
            out RaycastHit hit,
            kontrolMesafesi
        ))
        {
            Debug.Log("Tespit edilen nesne: " + hit.collider.name);
            return true;
        }

        return false;
    }
    public void HareketEt(Vector3 hedefKonum)
    {

        if (IsEngelVarmi())
        {
            return;
        }

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
