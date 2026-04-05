using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxDragDistance = 2.5f;
    public float shotForceMultiplier = 8f;
    public float minSpeedToBeMoving = 0.2f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    [Header("Aim Line")]
    public LineRenderer aimLine;
    public float lineZ = -0.1f;
    public float minLineWidth = 0.05f;
    public float maxLineWidth = 0.25f;

    [Header("Stop Tuning")]
    public float linearDamping = 0.8f;
    public float angularDamping = 1.2f;
    public float hardStopSpeed = 0.12f;
    public float hardStopAngularSpeed = 5f;
    public float groundedStopDelay = 0.15f;

    private Rigidbody2D rb;
    private Camera mainCamera;

    private bool isDragging = false;
    private Vector2 dragStartMouseWorld;
    private Vector2 dragCurrentMouseWorld;

    private float groundedSlowTimer = 0f;

    private bool hasHiddenStartOverlay = false;
    private int strokeCount = 0;
    private bool levelEnded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
    }

    private void Start()
    {
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!IsGrounded() || IsMoving())
        {
            if (aimLine != null)
                aimLine.enabled = false;

            return;
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;
                dragStartMouseWorld = mouseWorld;
                dragCurrentMouseWorld = mouseWorld;
            }
        }

        if (isDragging)
        {
            dragCurrentMouseWorld = mouseWorld;
            UpdateAimLine();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            if (aimLine != null)
                aimLine.enabled = false;

            ShootBall();
        }
    }

    private void FixedUpdate()
    {
        TryHardStop();
    }

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

    private void ShootBall()
    {
        if (levelEnded)
            return;

        Vector2 dragVector = dragCurrentMouseWorld - dragStartMouseWorld;
        dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        if (dragVector.sqrMagnitude <= 0.0001f)
            return;

        Vector2 force = -dragVector * shotForceMultiplier;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        groundedSlowTimer = 0f;

        rb.AddForce(force, ForceMode2D.Impulse);

        strokeCount++;

        if (!hasHiddenStartOverlay)
        {
            hasHiddenStartOverlay = true;

            if (StartOverlayUI.Instance != null)
                StartOverlayUI.Instance.HideOverlay();
        }
    }

    private void TryHardStop()
    {
        bool grounded = IsGrounded();
        float speed = rb.linearVelocity.magnitude;
        float spin = Mathf.Abs(rb.angularVelocity);

        // Only stop the ball when it is on the ground and already very slow.
        // This prevents weird mid-bounce corrections.
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

    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > minSpeedToBeMoving;
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

    public int GetStrokeCount()
    {
        return strokeCount;
    }

    public void EndLevel()
    {
        levelEnded = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}