using UnityEngine;
using TMPro;

public class StrokeUI : MonoBehaviour
{
    public BallController ball;
    public TMP_Text strokeText;

    private void Update()
    {
        if (ball != null && strokeText != null)
        {
            strokeText.text = "Strokes: " + ball.GetStrokeCount();
        }
    }
}