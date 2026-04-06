using UnityEngine;
using TMPro;

public class StrokeUI : MonoBehaviour
{
    // Reference to the ball (used to get stroke count)
    public BallController ball;

    // UI text element that displays strokes
    public TMP_Text strokeText;

    private void Update()
    {
        // Ensure both references are assigned before updating UI
        if (ball != null && strokeText != null)
        {
            // Update text every frame with current stroke count
            strokeText.text = "Strokes: " + ball.GetStrokeCount();
        }
    }
}