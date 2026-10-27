
using UnityEngine;
using UnityEngine.AI;

public class CompanionFollow : MonoBehaviour
{
    [Header("Takip Ayarlari")]
    public Transform targetTransform;
    public float stoppingDistance = 1.5f;
    public float targetSearchRadius = 10f;

    [Header("Gemiye Uzaklik")]
    public float takipYanMesafesi = 6f;
    public float takipArkaMesafesi = 2f;

    private NavMeshAgent agent;
    private float nextUpdateTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.enabled = false;
    }

    void Start()
    {
        if (agent == null)
        {
            Debug.LogError("Aibot: NavMeshAgent bulunamadi!");
            enabled = false;
            return;
        }

        if (targetTransform == null)
        {
            GameObject gemiObj =
                GameObject.FindGameObjectWithTag("Gemi");

            if (gemiObj != null)
                targetTransform = gemiObj.transform;
        }

        if (targetTransform == null)
        {
            Debug.LogError("Aibot: Gemi bulunamadi!");
            enabled = false;
            return;
        }

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            Debug.LogError("Aibot: Yakininda NavMesh yok!");
            enabled = false;
            return;
        }

        transform.position = hit.position;
        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        Debug.Log("Aibot: Takip sistemi hazir!");
    }

    void Update()
    {
        if (agent == null || !agent.enabled ||
            !agent.isOnNavMesh || targetTransform == null)
            return;

        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + 0.2f;

        Vector3 yan = targetTransform.right * takipYanMesafesi;
        Vector3 arka = -targetTransform.forward * takipArkaMesafesi;

        Vector3 hedef = targetTransform.position + yan + arka;
        hedef.y = transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            hedef, out hit, targetSearchRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
