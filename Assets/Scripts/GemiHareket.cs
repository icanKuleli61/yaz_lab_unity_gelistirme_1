using UnityEngine;


public class GemiHareket : MonoBehaviour
{

    private Gemi aktifGemi;
    private GemiData gemiData;

    [SerializeField]
    private Transform gemiUcu;

    [SerializeField]
    private SphereCollider enerjiKalkaniCollider;


    private bool kalkanaDegiyorMu = false;

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

        if (gemiUcu == null || gemiData == null)
        {
            return false;
        }

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
            if (hit.collider.CompareTag("EnerjiKalkani"))
            {
                Debug.Log("Tespit edilen kalkan: " + hit.collider.name);
                return true;
            }



        }

        if (kalkanaDegiyorMu)
        {
            return true;
        }

        return false;
    }
    public void HareketEt(Vector3 hedefKonum)
    {
        if (gemiUcu == null || gemiData == null)
            return;



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

        if (IsEngelVarmi())
        {
            return;
        }

        transform.position += gemiUcu.forward
                    * gemiData.GetGemiHizi()
                    * Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnerjiKalkani")) {

            kalkanaDegiyorMu = true;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("EnerjiKalkani"))
        {
            kalkanaDegiyorMu = false;
        }
    }
}
