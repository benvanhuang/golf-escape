using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // =========================
    // Target the camera follows
    // =========================
    [Header("Target")]
    public Transform target; // Usually the golf ball

    // =========================
    // Follow settings
    // =========================
    [Header("Follow")]
    public float followSpeed = 8f; // How quickly camera follows target
    public Vector3 baseOffset = new Vector3(0f, 10f, -10f); // Default offset from target

    // =========================
    // Camera panning (look ahead)
    // =========================
    [Header("Pan")]
    public float holdTimeBeforePan = 1f; // Time player must hold key before panning
    public float panDistance = 6f;       // How far camera pans
    public float panSpeed = 12f;         // Speed of pan movement

    // Internal tracking variables
    private float holdTimer = 0f;            // How long a direction key is held
    private Vector3 heldDirection = Vector3.zero; // Current direction being held
    private Vector3 currentPanOffset = Vector3.zero; // Current pan offset applied

    private void LateUpdate()
    {
        // If no target assigned, do nothing
        if (target == null)
            return;

        // If UI is open, disable panning and reset camera smoothly
        if (IsUIScreenOpen())
        {
            holdTimer = 0f;
            heldDirection = Vector3.zero;

            // Smoothly return camera to default position (no pan)
            currentPanOffset = Vector3.MoveTowards(
                currentPanOffset,
                Vector3.zero,
                panSpeed * Time.deltaTime
            );
        }
        else
        {
            // Update input direction and pan behavior
            UpdateHeldDirection();
            UpdatePanOffset();
        }

        // Calculate desired camera position (target + offset + pan)
        Vector3 desiredPosition = target.position + baseOffset + currentPanOffset;

        // Smoothly move camera toward desired position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
    }

    // =========================
    // Check if UI screens are open
    // =========================
    private bool IsUIScreenOpen()
    {
        // Check if start screen is active
        bool startScreenOpen =
            StartOverlayUI.Instance != null &&
            StartOverlayUI.Instance.titleTextObject != null &&
            StartOverlayUI.Instance.titleTextObject.activeSelf;

        // Check if end screen is active
        bool endScreenOpen =
            EndScreenUI.Instance != null &&
            EndScreenUI.Instance.endScreenRoot != null &&
            EndScreenUI.Instance.endScreenRoot.activeSelf;

        // Return true if ANY UI screen is open
        return startScreenOpen || endScreenOpen;
    }

    // =========================
    // Track WASD input direction
    // =========================
    private void UpdateHeldDirection()
    {
        // Ensure keyboard input exists
        if (Keyboard.current == null)
            return;

        Vector3 newDirection = Vector3.zero;

        // Detect WASD input
        if (Keyboard.current.wKey.isPressed)
            newDirection += Vector3.up;
        if (Keyboard.current.sKey.isPressed)
            newDirection += Vector3.down;
        if (Keyboard.current.aKey.isPressed)
            newDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed)
            newDirection += Vector3.right;

        // Normalize so diagonal movement isn't faster
        newDirection = newDirection.normalized;

        // If no key is pressed, reset
        if (newDirection == Vector3.zero)
        {
            holdTimer = 0f;
            heldDirection = Vector3.zero;
            return;
        }

        // If direction changed, reset timer
        if (newDirection != heldDirection)
        {
            heldDirection = newDirection;
            holdTimer = 0f;
        }

        // Increment how long the direction has been held
        holdTimer += Time.deltaTime;
    }

    // =========================
    // Handle camera panning logic
    // =========================
    private void UpdatePanOffset()
    {
        Vector3 targetPanOffset = Vector3.zero;

        // Only start panning after holding input long enough
        if (holdTimer >= holdTimeBeforePan && heldDirection != Vector3.zero)
        {
            targetPanOffset = heldDirection * panDistance;
        }

        // Smoothly move toward target pan offset
        currentPanOffset = Vector3.MoveTowards(
            currentPanOffset,
            targetPanOffset,
            panSpeed * Time.deltaTime
        );
    }
}