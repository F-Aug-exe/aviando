using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OwnerAI : MonoBehaviour
{
    public enum AIState { Patrolling, Chasing, Searching }
    public AIState currentState;

    [Header("Configuración de Visión")]
    public Transform player;
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Patrullaje")]
    public Transform[] waypoints;
    private int currentWaypointIndex;

    [Header("Búsqueda (Sigilo)")]
    public float searchTime = 4f;
    private float searchTimer;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = AIState.Patrolling;
        GoToNextWaypoint();
    }

    void Update()
    {
        DetectPlayer(); // Siempre evalúa si ve al jugador

        switch (currentState)
        {
            case AIState.Patrolling:
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextWaypoint();
                break;

            case AIState.Chasing:
                agent.destination = player.position;
                break;

            case AIState.Searching:
                searchTimer += Time.deltaTime;
                if (searchTimer >= searchTime)
                {
                    currentState = AIState.Patrolling;
                    GoToNextWaypoint();
                }
                break;
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void DetectPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = false;

        if (distToPlayer < viewRadius && Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
        {
            // Tira un rayo, si no golpea un obstáculo antes que al jugador, lo ve.
            if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
            {
                canSeePlayer = true;
            }
        }

        if (canSeePlayer)
        {
            currentState = AIState.Chasing;
            searchTimer = 0f;
        }
        else if (currentState == AIState.Chasing)
        {
            // Lo perdió de vista, empieza a buscar
            currentState = AIState.Searching;
            agent.destination = transform.position; // Se detiene
        }
    }
}
