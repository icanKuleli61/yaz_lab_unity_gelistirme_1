using UnityEngine;


public class GemiHareket : MonoBehaviour
{

    private Gemi aktifGemi;
    private GemiData gemiData;

    [SerializeField]
    private Transform gemiUcu;

    [SerializeField]
    private SphereCollider enerjiKalkaniCollider;

    private bool saldiriAktif;
    private Transform saldiriHedefi;


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
        if (saldiriAktif)
        {
            SaldiriRotasyonu();
        }
    }

    public void SaldiriRotasyonu()
    {
        if (saldiriHedefi == null || gemiUcu == null || gemiData == null)
            return;

        // Gemiden boss'a doğru yön vektörü.
        Vector3 hedefYon = saldiriHedefi.position - transform.position;
        hedefYon.y = 0f;

        if (hedefYon.sqrMagnitude < 0.001f)
            return;

        Vector3 mevcutBurunYonu = gemiUcu.forward;
        mevcutBurunYonu.y = 0f;

        Quaternion hedefRotasyon = Quaternion.FromToRotation(
            mevcutBurunYonu,
            hedefYon.normalized
        ) * transform.rotation;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            hedefRotasyon,
            gemiData.GetDonusHizi() * Time.deltaTime
        );
    }

    public void SaldiriDurumuAyarla(bool aktif, Transform hedef = null)
    {
        saldiriAktif = aktif;
        saldiriHedefi = aktif ? hedef : null;
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

        // Fare hedefinin yönü.
        Vector3 hareketYonu = hedefKonum - transform.position;
        hareketYonu.y = 0f;

        if (hareketYonu.sqrMagnitude < 0.01f)
            return;

        // Saldırı kapalıysa geminin burnunu fareye döndür.
        if (!saldiriAktif)
        {
            Vector3 mevcutBurunYonu = gemiUcu.forward;
            mevcutBurunYonu.y = 0f;

            Quaternion hedefRotasyon = Quaternion.FromToRotation(
                mevcutBurunYonu,
                hareketYonu
            ) * transform.rotation;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                hedefRotasyon,
                gemiData.GetDonusHizi() * Time.deltaTime
            );

            // Normal hareket: Geminin burnunun yönünde ilerle.
            hareketYonu = gemiUcu.forward;
            hareketYonu.y = 0f;
        }

        if (IsEngelVarmi())
            return;

        // Saldırı açıkken de kapalıyken de hareket yönü fare hedefidir.
        transform.position += hareketYonu.normalized *
                              gemiData.GetGemiHizi() *
                              Time.deltaTime;
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
