
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class CompanionMission : MonoBehaviour
{
    [Header("Gorev Hedefleri")]
    public Transform shipTransform;
    public Transform switchTransform;

    [Header("Hareket Ayarlari")]
    public float stoppingDistance = 1.5f;
    public float targetSearchRadius = 100f;
    public float destinationUpdateInterval = 0.5f;

    [Header("Gemi Takip Ayarlari")]
    public float takipYanMesafesi = 6f;
    public float takipArkaMesafesi = 2f;

    private NavMeshAgent agent;
    private CompanionFollow oldFollow;
    private SalterKontrol switchControl;

    private enum MissionState
    {
        FollowShip,
        GoingToSwitch,
        ReturningToShip
    }

    private MissionState state = MissionState.FollowShip;
    private float nextUpdateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        oldFollow = GetComponent<CompanionFollow>();

        if (agent == null)
        {
            Debug.LogError("Companion: NavMeshAgent bulunamadi!");
            enabled = false;
            return;
        }

        agent.enabled = true;

        NavMeshHit botHit;

        if (!NavMesh.SamplePosition(
            transform.position, out botHit, 10f, NavMesh.AllAreas))
        {
            Debug.LogError("Companion: Botun yakininda NavMesh yok!");
            enabled = false;
            return;
        }

        if (!agent.Warp(botHit.position) || !agent.isOnNavMesh)
        {
            Debug.LogError("Companion: Bot NavMesh'e yerlestirilemedi!");
            enabled = false;
            return;
        }

        if (shipTransform == null || switchTransform == null)
        {
            Debug.LogError("Companion: Gemi veya Salter atanmamis!");
            enabled = false;
            return;
        }

        switchControl = switchTransform.GetComponent<SalterKontrol>();

        if (switchControl == null)
            switchControl = switchTransform.GetComponentInChildren<SalterKontrol>();

        if (switchControl == null)
            switchControl = switchTransform.GetComponentInParent<SalterKontrol>();

        if (switchControl == null)
        {
            Debug.LogError("Companion: SalterKontrol bulunamadi!");
            enabled = false;
            return;
        }

        if (oldFollow != null)
            oldFollow.enabled = false;

        agent.stoppingDistance = stoppingDistance;

        Debug.Log("Companion hazir! T tusuyla veya panelden gorevi baslat.");
    }

    public void BaslatSalterGorevi()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (state != MissionState.FollowShip)
        {
            Debug.Log("Companion: Zaten bir gorev yapiyorum!");
            return;
        }

        if (switchControl == null || switchControl.aktifMi)
        {
            Debug.Log("Companion: Salter zaten aktif veya kontrol bulunamadi!");
            return;
        }

        if (SetDestination(switchTransform.position))
        {
            state = MissionState.GoingToSwitch;
            Debug.Log("Companion: Saltere gidiyorum!");
        }
    }

    void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            BaslatSalterGorevi();
        }

        if (state == MissionState.FollowShip &&
            Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + destinationUpdateInterval;
            SetDestination(GemiYanindakiHedef());
        }

        if (state == MissionState.GoingToSwitch && HasArrived())
        {
            Debug.Log("Companion: Saltere ulastim!");

            switchControl.SalteriAktifEt();

            if (SetDestination(GemiYanindakiHedef()))
            {
                state = MissionState.ReturningToShip;
                Debug.Log("Companion: Gemiye geri donuyorum!");
            }
        }

        if (state == MissionState.ReturningToShip && HasArrived())
        {
            state = MissionState.FollowShip;
            Debug.Log("Companion: Gemiye dondum, takibe devam!");
        }
    }

    Vector3 GemiYanindakiHedef()
    {
        Vector3 hedef =
            shipTransform.position
            + shipTransform.right * takipYanMesafesi
            - shipTransform.forward * takipArkaMesafesi;

        hedef.y = transform.position.y;

        return hedef;
    }

    bool SetDestination(Vector3 targetPosition)
    {
        NavMeshHit targetHit;

        if (!NavMesh.SamplePosition(
            targetPosition,
            out targetHit,
            targetSearchRadius,
            NavMesh.AllAreas))
        {
            Debug.LogWarning(
                "Companion: NavMesh bulunamadi. Hedef: " + targetPosition);
            return false;
        }

        NavMeshPath path = new NavMeshPath();

        bool pathFound = agent.CalculatePath(targetHit.position, path);

        if (!pathFound || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning(
                "Companion: Hedefe tam rota yok. Hedef: " + targetHit.position);
            return false;
        }

        if (!agent.SetDestination(targetHit.position))
        {
            Debug.LogWarning("Companion: Rota ayarlanamadi!");
            return false;
        }

        Debug.Log("Companion: Rota ayarlandi: " + targetHit.position);
        return true;
    }

    bool HasArrived()
    {
        if (agent.pathPending || !agent.hasPath)
            return false;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.3f;
    }
}
