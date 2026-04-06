using UnityEngine;

public class WaterZone : MonoBehaviour
{
    // =========================
    // Movement slowing (water/sand effect)
    // =========================
    public float sandLinearDamping = 3.5f;   // Slows horizontal movement
    public float sandAngularDamping = 4.5f;  // Slows rotation/spin

    // =========================
    // Splash sound
    // =========================
    public AudioClip splashSFX;

    [Header("SFX Settings")]
    [Range(0f, 3f)] public float splashVolume = 1.5f; // Overall volume multiplier
    public float splashSpeedDivisor = 6f;             // Controls how speed affects volume
    public float minPitch = 0.9f;                     // Random pitch variation (low)
    public float maxPitch = 1.1f;                     // Random pitch variation (high)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if object entering is the ball
        BallController ball = other.GetComponent<BallController>();
        if (ball == null) return;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();

        // =========================
        // Calculate splash volume based on ball speed
        // =========================
        float speed = rb.linearVelocity.magnitude;

        // Normalize speed into 0–1 range
        float dynamicVolume = Mathf.Clamp01(speed / splashSpeedDivisor);

        // Apply multiplier
        float finalVolume = dynamicVolume * splashVolume;

        // =========================
        // Play splash sound with pitch variation
        // =========================
        if (splashSFX != null)
        {
            // Create temporary object to hold AudioSource
            GameObject temp = new GameObject("TempSplashAudio");
            temp.transform.position = ball.transform.position;

            AudioSource a = temp.AddComponent<AudioSource>();
            a.clip = splashSFX;
            a.volume = finalVolume;

            // Random pitch for more natural sound variation
            a.pitch = Random.Range(minPitch, maxPitch);

            a.Play();

            // Destroy temp object after sound finishes
            Destroy(temp, splashSFX.length);
        }

        // =========================
        // Apply damping (slow ball movement in water/sand)
        // =========================
        rb.linearDamping = sandLinearDamping;
        rb.angularDamping = sandAngularDamping;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if object exiting is the ball
        BallController ball = other.GetComponent<BallController>();
        if (ball == null) return;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();

        // Restore original damping values from BallController
        rb.linearDamping = ball.linearDamping;
        rb.angularDamping = ball.angularDamping;
    }
}