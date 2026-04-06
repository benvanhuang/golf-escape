using UnityEngine;
using UnityEngine.InputSystem;

// Ensure the GameObject has required components
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    // =========================
    // Charge (dragging) sound settings
    // =========================
    [Header("Charge SFX")]
    public AudioSource chargeAudioSource;   // Audio source for charging sound
    public AudioClip chargeLoopSFX;         // Looping sound while dragging

    public float minPitch = 0.8f;           // Pitch at minimum drag
    public float maxPitch = 1.5f;           // Pitch at max drag

    [Range(0f, 1f)] public float chargeVolume = 0.4f;         // Max volume while charging
    [Range(0f, 1f)] public float minChargeDragVolume = 0.2f;  // Volume at low drag

    private bool isChargePlaying = false;   // Tracks if charge SFX is active

    // =========================
    // General SFX
    // =========================
    [Header("SFX")]
    public AudioSource audioSource;         // Main audio source
    public AudioClip hitSFX;                // Sound when ball is shot
    public AudioClip bounceSFX;             // Default bounce sound

    public float bounceThreshold = 0.3f;    // Minimum impact required to play bounce
    public float bounceVolumeDivisor = 6f;  // Controls bounce volume scaling
    public float minBouncePitch = 0.95f;    // Slight pitch variation for realism
    public float maxBouncePitch = 1.05f;

    // =========================
    // Shooting / drag mechanics
    // =========================
    [Header("Shot Settings")]
    public float maxDragDistance = 2.5f;        // Max distance player can drag
    public float shotForceMultiplier = 8f;      // Strength of shot
    public float minSpeedToBeMoving = 0.2f;     // Threshold to consider ball moving

    // =========================
    // Ground detection
    // =========================
    [Header("Ground Check")]
    public LayerMask groundLayer;               // What counts as "ground"
    public float groundCheckDistance = 0.6f;    // Distance to raycast downward

    // =========================
    // Aim line (visual feedback)
    // =========================
    [Header("Aim Line")]
    public LineRenderer aimLine;
    public float lineZ = -0.1f;                 // Keeps line rendered above ground
    public float minLineWidth = 0.05f;
    public float maxLineWidth = 0.25f;

    // =========================
    // Stopping behavior (prevents endless micro bouncing)
    // =========================
    [Header("Stop Tuning")]
    public float linearDamping = 0.8f;
    public float angularDamping = 1.2f;
    public float hardStopSpeed = 0.12f;         // Speed below which we snap to stop
    public float hardStopAngularSpeed = 5f;
    public float groundedStopDelay = 0.15f;     // Delay before forcing stop

    // =========================
    // Internal references
    // =========================
    private Rigidbody2D rb;
    private Collider2D ballCollider;
    private Camera mainCamera;

    // Drag state tracking
    private bool isDragging = false;
    private Vector2 dragStartMouseWorld;
    private Vector2 dragCurrentMouseWorld;

    // Timer for stopping logic
    private float groundedSlowTimer = 0f;

    // Gameplay state
    private bool hasHiddenStartOverlay = false;
    private int strokeCount = 0;
    private bool levelEnded = false;

    private void Awake()
    {
        // Cache components
        rb = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        // Fallback in case audio source not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Apply damping settings
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
    }

    private void Start()
    {
        // Initialize aim line
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
        // Ensure mouse exists (Input System)
        if (Mouse.current == null)
            return;

        // Prevent dragging/shooting while airborne and moving
        if (!IsGrounded() && rb.linearVelocity.magnitude > 0.05f)
        {
            if (aimLine != null)
                aimLine.enabled = false;

            if (isDragging)
            {
                isDragging = false;
                StopChargeSound();
            }

            return;
        }

        // Convert mouse position to world coordinates
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        // Start dragging if clicking on ball
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (ballCollider != null && ballCollider.OverlapPoint(mouseWorld))
            {
                isDragging = true;
                dragStartMouseWorld = mouseWorld;
                dragCurrentMouseWorld = mouseWorld;

                StartChargeSound();
            }
        }

        // While dragging, update aim and sound
        if (isDragging)
        {
            dragCurrentMouseWorld = mouseWorld;
            UpdateAimLine();

            // Adjust pitch/volume based on drag strength
            if (chargeAudioSource != null && isChargePlaying)
            {
                Vector2 dragVector = dragCurrentMouseWorld - dragStartMouseWorld;
                float dragAmount = Mathf.Clamp01(dragVector.magnitude / maxDragDistance);

                chargeAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, dragAmount);
                chargeAudioSource.volume = Mathf.Lerp(minChargeDragVolume, chargeVolume, dragAmount);
            }
        }

        // Release shot
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            if (aimLine != null)
                aimLine.enabled = false;

            StopChargeSound();
            ShootBall();
        }
    }

    private void FixedUpdate()
    {
        // Apply stop logic each physics step
        TryHardStop();
    }

    // =========================
    // Charge Sound Logic
    // =========================
    private void StartChargeSound()
    {
        if (chargeAudioSource == null || chargeLoopSFX == null)
            return;

        chargeAudioSource.clip = chargeLoopSFX;
        chargeAudioSource.loop = true;
        chargeAudioSource.pitch = minPitch;
        chargeAudioSource.volume = minChargeDragVolume;

        if (!chargeAudioSource.isPlaying)
            chargeAudioSource.Play();

        isChargePlaying = true;
    }

    private void StopChargeSound()
    {
        if (chargeAudioSource != null)
            chargeAudioSource.Stop();

        isChargePlaying = false;
    }

    // =========================
    // Aim Line Visualization
    // =========================
    private void UpdateAimLine()
    {
        if (aimLine == null)
            return;

        Vector2 dragVector = dragCurrentMouseWorld - dragStartMouseWorld;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        float dragAmount = dragVector.magnitude;
        float normalizedPower = dragAmount / maxDragDistance;

        Vector3 ballPos = transform.position;
        Vector3 previewEnd = ballPos - (Vector3)dragVector;

        aimLine.enabled = true;
        aimLine.SetPosition(0, new Vector3(ballPos.x, ballPos.y, lineZ));
        aimLine.SetPosition(1, new Vector3(previewEnd.x, previewEnd.y, lineZ));

        float width = Mathf.Lerp(minLineWidth, maxLineWidth, normalizedPower);
        aimLine.startWidth = width;
        aimLine.endWidth = width;
    }

    // =========================
    // Shooting Logic
    // =========================
    private void ShootBall()
    {
        if (levelEnded)
            return;

        Vector2 dragVector = dragCurrentMouseWorld - dragStartMouseWorld;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        // Ignore tiny drags
        if (dragVector.sqrMagnitude <= 0.0001f)
            return;

        Vector2 force = -dragVector * shotForceMultiplier;

        // Reset motion before applying new force
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        groundedSlowTimer = 0f;

        rb.AddForce(force, ForceMode2D.Impulse);

        // Play hit sound
        if (audioSource != null && hitSFX != null)
            audioSource.PlayOneShot(hitSFX);

        strokeCount++;

        // Hide start overlay after first shot
        if (!hasHiddenStartOverlay)
        {
            hasHiddenStartOverlay = true;

            if (StartOverlayUI.Instance != null)
                StartOverlayUI.Instance.HideOverlay();
        }
    }

    // =========================
    // Stop jittering / micro-bounce fix
    // =========================
    private void TryHardStop()
    {
        bool grounded = IsGrounded();
        float speed = rb.linearVelocity.magnitude;
        float spin = Mathf.Abs(rb.angularVelocity);

        if (grounded && speed < hardStopSpeed && spin < hardStopAngularSpeed)
        {
            groundedSlowTimer += Time.fixedDeltaTime;

            if (groundedSlowTimer >= groundedStopDelay)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                groundedSlowTimer = 0f;
            }
        }
        else
        {
            groundedSlowTimer = 0f;
        }
    }

    // Check if ball is still moving
    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > minSpeedToBeMoving;
    }

    // Raycast downward to detect ground
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    // Debug ground ray
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

    public int GetStrokeCount()
    {
        return strokeCount;
    }

    // Stop all movement when level ends
    public void EndLevel()
    {
        levelEnded = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        StopChargeSound();
    }

    // =========================
    // Collision / bounce sound
    // =========================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impact = collision.relativeVelocity.magnitude;

        // Ignore very small impacts
        if (impact < bounceThreshold)
            return;

        AudioClip clipToPlay = bounceSFX;

        // Use custom bounce sound if surface has one
        BounceSurface surface = collision.gameObject.GetComponent<BounceSurface>();
        if (surface != null && surface.bounceSFX != null)
        {
            clipToPlay = surface.bounceSFX;
        }

        if (audioSource != null && clipToPlay != null)
        {
            float volume = Mathf.Clamp01(impact / bounceVolumeDivisor);

            audioSource.pitch = Random.Range(minBouncePitch, maxBouncePitch);
            audioSource.PlayOneShot(clipToPlay, volume);
            audioSource.pitch = 1f;
        }
    }
}