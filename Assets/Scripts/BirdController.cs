using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BirdController : MonoBehaviour
{
    [Header("Físicas y Movimiento")]
    public float walkSpeed = 3f;
    public float flySpeed = 8f;
    public float turnSpeed = 120f;
    public float flapForce = 15f;
    public float glideFallSpeed = 1.5f;

    [Header("Estamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRecoveryRate = 25f;

    [Header("Detección de Suelo")]
    public LayerMask groundLayer;
    public float groundCheckOffset = 0.1f;
    public float raycastLength = 0.3f;
    public bool isGrounded;

    [Header("Referencias")]
    public Animator animator;
    public bool isCinematic = false;

    private Rigidbody rb;
    private int flyBoolHash, hopIntHash;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Gravedad controlada por script
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        currentStamina = maxStamina;

        if (animator != null)
        {
            flyBoolHash = Animator.StringToHash("flying");
            hopIntHash = Animator.StringToHash("hop");
        }
    }

    void Update()
    {
        if (isCinematic) return;
        CheckGrounded();
        HandleRotation();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isCinematic)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        HandleMovement();
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * groundCheckOffset, Vector3.down, raycastLength, groundLayer);
    }

    void HandleRotation()
    {
        float turn = Input.GetAxis("Horizontal");
        transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
    }

    void HandleMovement()
    {
        float forwardInput = Input.GetAxis("Vertical");
        // Evitar volar en reversa
        if (!isGrounded && forwardInput < 0) forwardInput = 0;

        float currentSpeed = isGrounded ? walkSpeed : flySpeed;
        Vector3 movement = transform.forward * (forwardInput * currentSpeed);
        float targetVelocityY = rb.linearVelocity.y;

        if (isGrounded)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRecoveryRate * Time.fixedDeltaTime);
            if (Input.GetKey(KeyCode.Space)) targetVelocityY = flapForce; // Salto desde el suelo
        }
        else
        {
            if (Input.GetKey(KeyCode.Space) && currentStamina > 0)
            {
                targetVelocityY += flapForce * Time.fixedDeltaTime; // Aleteo
                currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
            }
            else if (Input.GetKey(KeyCode.Space) && currentStamina <= 0)
            {
                targetVelocityY = -glideFallSpeed; // Planear suavemente sin estamina
            }
            else
            {
                targetVelocityY -= 9.81f * 2f * Time.fixedDeltaTime; // Caída por gravedad
            }
        }

        rb.linearVelocity = new Vector3(movement.x, targetVelocityY, movement.z);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetBool(flyBoolHash, !isGrounded);
        if (isGrounded) animator.SetInteger(hopIntHash, Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ? 1 : 0);
    }
}
