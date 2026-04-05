using UnityEngine;

public class GoalZone : MonoBehaviour
{
    private bool goalReached = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalReached)
            return;

        BallController ball = other.GetComponent<BallController>();
        if (ball == null)
            return;

        goalReached = true;

        int finalStrokes = ball.GetStrokeCount();
        ball.EndLevel();

        if (StartOverlayUI.Instance != null)
            StartOverlayUI.Instance.SaveBestStrokes(finalStrokes);

        if (EndScreenUI.Instance != null)
            EndScreenUI.Instance.ShowEndScreen(finalStrokes);
    }
}