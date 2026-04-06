using UnityEngine;

// Ensure the object has a Rigidbody2D (required for movement)
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    // =========================
    // Movement points
    // =========================
    public Transform pointA; // Start position
    public Transform pointB; // End position

    public float speed = 2f; // Movement speed between points

    private Rigidbody2D rb;  // Rigidbody used for movement
    private Vector2 target;  // Current destination point

    private void Awake()
    {
        // Cache Rigidbody and set it to kinematic (not affected by physics forces)
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        // Initialize first movement target (move toward point B first)
        if (pointB != null)
            target = pointB.position;
    }

    private void FixedUpdate()
    {
        // Safety check: ensure both points are assigned
        if (pointA == null || pointB == null)
            return;

        // Move platform toward current target position
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        // Apply movement using Rigidbody (keeps physics interactions stable)
        rb.MovePosition(newPosition);

        // Check if platform reached target (with small tolerance)
        if (Vector2.Distance(rb.position, target) < 0.05f)
        {
            // Switch target to the opposite point
            target = target == (Vector2)pointA.position
                ? (Vector2)pointB.position
                : (Vector2)pointA.position;
        }
    }
}