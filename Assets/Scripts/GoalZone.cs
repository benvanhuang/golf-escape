using UnityEngine;

public class GoalZone : MonoBehaviour
{
    // Prevents goal from triggering multiple times
    private bool goalReached = false;

    // =========================
    // Sound tiers based on performance
    // =========================
    [Header("Win SFX Tiers")]
    public AudioClip perfectSFX;   // Played for best performance (low strokes)
    public AudioClip goodSFX;      // Played for average performance
    public AudioClip badSFX;       // Played for poor performance

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If goal already reached, ignore further triggers
        if (goalReached)
            return;

        // Check if the object entering is the ball
        BallController ball = other.GetComponent<BallController>();
        if (ball == null)
            return;

        // Mark goal as completed
        goalReached = true;

        // Get final stroke count from ball
        int finalStrokes = ball.GetStrokeCount();

        // =========================
        // Select sound based on performance
        // =========================
        AudioClip chosenSFX = null;

        if (finalStrokes <= 12)
            chosenSFX = perfectSFX;   // Best outcome
        else if (finalStrokes <= 16)
            chosenSFX = goodSFX;      // Mid-tier outcome
        else
            chosenSFX = badSFX;       // Worst outcome

        // =========================
        // Play selected sound
        // =========================
        if (chosenSFX != null)
            AudioSource.PlayClipAtPoint(chosenSFX, transform.position);

        // Stop ball movement and prevent further input
        ball.EndLevel();

        // Save best score if applicable
        if (StartOverlayUI.Instance != null)
            StartOverlayUI.Instance.SaveBestStrokes(finalStrokes);

        // Show end screen UI with results
        if (EndScreenUI.Instance != null)
            EndScreenUI.Instance.ShowEndScreen(finalStrokes);
    }
}