using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TurtleCompanion : MonoBehaviour
{
    public enum TurtleState { Inactive, Activating, Idle, MovingToTarget }

    [Header("Estado")]
    public TurtleState currentState = TurtleState.Inactive;

    [Header("Activación")]
    [Tooltip("Modelo visual de la tortuga volteada (patas arriba).")]
    public GameObject flippedModel;
    [Tooltip("Modelo visual de la tortuga activa (patas abajo).")]
    public GameObject activeModel;
    public float activationFlipDuration = 1.2f;

    [Header("Movimiento")]
    public float moveSpeed = 1.5f;
    public float stoppingDistance = 0.3f;

    [Header("Impacto")]
    [Tooltip("Fuerza extra al llegar al objetivo (empujón contra la puerta).")]
    public float impactForce = 8f;

    private NavMeshAgent agent;
    private Vector3 targetPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.enabled = false; // Inactivo hasta que el ave la libere

        // Estado visual inicial: volteada
        if (flippedModel) flippedModel.SetActive(true);
        if (activeModel) activeModel.SetActive(false);
    }

    // --- API Pública ---

    /// <summary>
    /// Llamar cuando el ave choca con el terrario o empuja a la tortuga.
    /// </summary>
    public void Activate()
    {
        if (currentState != TurtleState.Inactive) return;
        currentState = TurtleState.Activating;
        StartCoroutine(FlipSequence());
    }

    /// <summary>
    /// El ave señala un obstáculo; la tortuga camina hacia él.
    /// </summary>
    public void SetTarget(Vector3 destination)
    {
        if (currentState != TurtleState.Idle) return;
        targetPosition = destination;
        currentState = TurtleState.MovingToTarget;
        agent.SetDestination(targetPosition);
    }

    // --- Internos ---

    void Update()
    {
        if (currentState != TurtleState.MovingToTarget) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            OnReachedTarget();
        }
    }

    void OnReachedTarget()
    {
        currentState = TurtleState.Idle;

        // Empujón físico hacia el objetivo (activa ExplosiveDoor vía colisión)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (targetPosition - transform.position).normalized;
            rb.AddForce(pushDir * impactForce, ForceMode.Impulse);
        }
    }

    System.Collections.IEnumerator FlipSequence()
    {
        // Animación simple de volteo: rotar 180° en X durante activationFlipDuration
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(180f, 0f, 0f);

        while (elapsed < activationFlipDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / activationFlipDuration);
            yield return null;
        }

        transform.rotation = endRot;

        // Intercambiar modelos
        if (flippedModel) flippedModel.SetActive(false);
        if (activeModel) activeModel.SetActive(true);

        // Restaurar rotación limpia y habilitar navegación
        transform.rotation = Quaternion.identity;
        agent.enabled = true;
        currentState = TurtleState.Idle;
    }

    // Colisión con el terrario activa la tortuga automáticamente
    void OnCollisionEnter(Collision collision)
    {
        if (currentState == TurtleState.Inactive && collision.gameObject.CompareTag("Player"))
        {
            Activate();
        }
    }
}
