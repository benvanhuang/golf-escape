using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float followSpeed = 8f;
    public Vector3 baseOffset = new Vector3(0f, 1.5f, -10f);

    [Header("Pan")]
    public float holdTimeBeforePan = 1f;
    public float panDistance = 6f;
    public float panSpeed = 12f;

    private float holdTimer = 0f;
    private Vector3 heldDirection = Vector3.zero;
    private Vector3 currentPanOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateHeldDirection();
        UpdatePanOffset();

        Vector3 desiredPosition = target.position + baseOffset + currentPanOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    private void UpdateHeldDirection()
    {
        if (Keyboard.current == null)
            return;

        Vector3 newDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            newDirection += Vector3.up;
        if (Keyboard.current.sKey.isPressed)
            newDirection += Vector3.down;
        if (Keyboard.current.aKey.isPressed)
            newDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed)
            newDirection += Vector3.right;

        newDirection = newDirection.normalized;

        if (newDirection == Vector3.zero)
        {
            holdTimer = 0f;
            heldDirection = Vector3.zero;
            return;
        }

        // If direction changed, restart the timer
        if (newDirection != heldDirection)
        {
            heldDirection = newDirection;
            holdTimer = 0f;
        }

        holdTimer += Time.deltaTime;
        Debug.Log("Holding: " + heldDirection + " Time: " + holdTimer);
    }

    private void UpdatePanOffset()
    {
        Vector3 targetPanOffset = Vector3.zero;

        if (holdTimer >= holdTimeBeforePan && heldDirection != Vector3.zero)
        {
            targetPanOffset = heldDirection * panDistance;
        }

        currentPanOffset = Vector3.MoveTowards(
            currentPanOffset,
            targetPanOffset,
            panSpeed * Time.deltaTime
        );
    }
}