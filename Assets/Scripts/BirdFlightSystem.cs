using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BirdFlightSystem : MonoBehaviour
{
    [Header("Referencias")]
    public Animator currentAnimator;
    public Transform cameraTransform;

    [Header("Estamina")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float flapCost = 10f;
    public float staminaRegenRate = 30f;

    [Header("Movimiento Terrestre (Snappy)")]
    public float walkSpeed = 7f;             // Velocidad ágil en tierra
    public float turnSpeed = 20f;            // Rotación rápida hacia el input
    public float groundJumpForce = 12f;      // Salto inicial limpio

    [Header("Planeo Estilo 'A Short Hike'")]
    public float glideSpeed = 11f;           // Velocidad constante de avance al planear
    public float glideDescentRate = -2.5f;   // Caída suave constante (JAMÁS asciende)
    public float airFlapForce = 9f;          // Impulso de aleteo aéreo corto
    public float extraGravity = 30f;         // Caída rápida en caída libre (sin mantener espacio)

    [Header("Escalada en Muros (BotW)")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.45f;
    public float climbSpeed = 3f;
    public float wallJumpForce = 8f;
    public bool isClimbing;

    [Header("Detección de Suelo")]
    public LayerMask groundLayer = -1;
    public float groundCheckDistance = 0.3f;
    public Vector3 groundCheckOffset = new Vector3(0, 0.05f, 0);

    private Rigidbody rb;
    private bool isGrounded;
    private bool isGliding;
    private RaycastHit wallHit;

    private int flyBoolHash, hopIntHash;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        flyBoolHash = Animator.StringToHash("flying");
        hopIntHash = Animator.StringToHash("hop");
    }

    void Update()
    {
        CheckEnvironment();
        HandleInputs();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            HandleClimbingPhysics();
        }
        else
        {
            HandleFlightAndGroundPhysics();
        }
    }

    void CheckEnvironment()
    {
        // 1. Detección de Suelo
        Vector3 rayOrigin = transform.position + groundCheckOffset;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);

        // 2. Detección de Muros
        Vector3 wallRayOrigin = transform.position + Vector3.up * 0.1f;
        bool touchingWall = Physics.Raycast(wallRayOrigin, transform.forward, out wallHit, wallCheckDistance, wallLayer);

        if (touchingWall && !isGrounded)
        {
            isClimbing = true;
        }
        else if (!touchingWall)
        {
            isClimbing = false;
        }

        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void HandleInputs()
    {
        // Regenerar estamina en suelo o muro
        if (isGrounded || isClimbing)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            isGliding = false;
        }

        if (isClimbing)
        {
            // Salto para soltarse del muro
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isClimbing = false;
                Vector3 jumpOffDir = (wallHit.normal + Vector3.up).normalized;
                rb.linearVelocity = jumpOffDir * wallJumpForce;
            }
        }
        else if (isGrounded)
        {
            // Salto en Tierra (Tap Espacio)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 moveInput = GetCameraRelativeInput();
                Vector3 jumpVel = Vector3.up * groundJumpForce;
                if (moveInput.magnitude > 0.1f)
                {
                    jumpVel += moveInput * (walkSpeed * 0.8f);
                }
                rb.linearVelocity = jumpVel;
            }
        }
        else
        {
            // Moverse/Planear en el aire (Mantener Espacio)
            isGliding = Input.GetKey(KeyCode.Space);

            // Aleteo aéreo con estamina (Tap Espacio)
            if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= flapCost)
            {
                currentStamina -= flapCost;
                Vector3 moveInput = GetCameraRelativeInput();
                Vector3 flapDirection = moveInput.magnitude > 0.1f ? moveInput : transform.forward;
                
                rb.linearVelocity = new Vector3(flapDirection.x * glideSpeed, airFlapForce, flapDirection.z * glideSpeed);
            }
        }
    }

    void HandleClimbingPhysics()
    {
        rb.useGravity = false;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Quaternion wallRot = Quaternion.LookRotation(-wallHit.normal);
        transform.rotation = Quaternion.Slerp(transform.rotation, wallRot, turnSpeed * Time.fixedDeltaTime);

        Vector3 climbDir = (transform.up * v + transform.right * h).normalized;
        rb.linearVelocity = climbDir * climbSpeed;
    }

    void HandleFlightAndGroundPhysics()
    {
        Vector3 moveInput = GetCameraRelativeInput();

        if (isGrounded)
        {
            rb.useGravity = true;

            Vector3 targetVel = moveInput * walkSpeed;
            rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

            if (moveInput.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveInput);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (isGliding)
            {
                // --- MODO PLANEO 'A SHORT HIKE' ---
                rb.useGravity = false;

                // Si el jugador presiona WASD, gira de inmediato hacia esa dirección. Si no presiona nada, mantiene el frente.
                Vector3 glideDir = moveInput.magnitude > 0.1f ? moveInput : transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(glideDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);

                // Forzar el vector horizontal exacto hacia donde apunta el ave
                Vector3 horizontalGlide = transform.forward * glideSpeed;

                // CORTAR INERCIA ASCENDENTE: Si el ave venía subiendo del salto, reduce drásticamente esa velocidad hacia el descenso controlado
                float targetY = Mathf.MoveTowards(rb.linearVelocity.y, glideDescentRate, 35f * Time.fixedDeltaTime);

                rb.linearVelocity = new Vector3(horizontalGlide.x, targetY, horizontalGlide.z);
            }
            else
            {
                // --- CAÍDA LIBRE (RÁPIDA) ---
                rb.useGravity = true;
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

                if (moveInput.magnitude > 0.1f)
                {
                    Vector3 airMove = moveInput * walkSpeed;
                    rb.linearVelocity = new Vector3(
                        Mathf.Lerp(rb.linearVelocity.x, airMove.x, 5f * Time.fixedDeltaTime),
                        rb.linearVelocity.y,
                        Mathf.Lerp(rb.linearVelocity.z, airMove.z, 5f * Time.fixedDeltaTime)
                    );

                    Quaternion targetRot = Quaternion.LookRotation(moveInput);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }

    Vector3 GetCameraRelativeInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

        return (camForward * v + camRight * h).normalized;
    }

    void UpdateAnimations()
    {
        if (currentAnimator == null) return;

        if (isGrounded || isClimbing)
        {
            currentAnimator.SetBool(flyBoolHash, false);
            bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
            currentAnimator.SetInteger(hopIntHash, isMoving ? 1 : 0);
        }
        else
        {
            currentAnimator.SetBool(flyBoolHash, isGliding || rb.linearVelocity.y > 0.1f);
            currentAnimator.SetInteger(hopIntHash, 0);
        }
    }
}