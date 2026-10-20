
using UnityEngine;
using UnityEngine.AI;

public class CompanionFollow : MonoBehaviour
{
    [Header("Takip Ayarlari")]
    public Transform targetTransform;
    public float stoppingDistance = 4f;
    public float targetSearchRadius = 10f;

    private NavMeshAgent agent;
    private float nextUpdateTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    void Start()
    {
        if (agent == null)
        {
            Debug.LogError("Aibot: NavMeshAgent bileseni bulunamadi!");
            enabled = false;
            return;
        }

        
        if (targetTransform == null)
        {
            GameObject gemiObj =
                GameObject.FindGameObjectWithTag("Gemi");

            if (gemiObj != null)
            {
                targetTransform = gemiObj.transform;
            }
        }

        if (targetTransform == null)
        {
            Debug.LogError(
                "Aibot: Takip edilecek gemi bulunamadi! " +
                "Target Transform alanini kontrol et."
            );

            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        
        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            transform.position,
            out hit,
            10f,
            NavMesh.AllAreas))
        {
            Debug.LogError(
                "Aibot: Yakininda NavMesh yok! " +
                "Botun konumunu ve NavMesh Bake ayarlarini kontrol et."
            );

            enabled = false;
            return;
        }

        
        transform.position = hit.position;

        
        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                "Aibot: Agent NavMesh uzerine yerlestirilemedi!"
            );

            agent.enabled = false;
            enabled = false;
            return;
        }

        Debug.Log("Aibot: NavMesh hazir, takip baslatiliyor!");
    }

    void Update()
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh ||
            targetTransform == null)
        {
            return;
        }

        
        if (Time.time < nextUpdateTime)
        {
            return;
        }

        nextUpdateTime = Time.time + 0.2f;

        
        Vector3 targetPosition = new Vector3(
            targetTransform.position.x,
            transform.position.y,
            targetTransform.position.z
        );

       
        NavMeshHit targetHit;

        if (NavMesh.SamplePosition(
            targetPosition,
            out targetHit,
            targetSearchRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(targetHit.position);
        }
        else
        {
            Debug.LogWarning(
                "Aibot: Geminin yakininda ulasilabilir " +
                "NavMesh noktasi bulunamadi."
            );
        }
    }
}
