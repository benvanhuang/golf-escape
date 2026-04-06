using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenUI : MonoBehaviour
{
    // Singleton instance for easy access from other scripts
    public static EndScreenUI Instance;

    // =========================
    // UI References
    // =========================
    public GameObject endScreenRoot; // Root object for end screen UI
    public TMP_Text strokesText;     // Displays number of strokes
    public TMP_Text starText;        // Displays star rating

    // =========================
    // Sound Effects
    // =========================
    public AudioClip buttonSFX; // Played when restarting level

    private void Awake()
    {
        // Set singleton instance
        Instance = this;
    }

    private void Start()
    {
        // Hide end screen at start of level
        if (endScreenRoot != null)
            endScreenRoot.SetActive(false);
    }

    // =========================
    // Show end screen with results
    // =========================
    public void ShowEndScreen(int strokes)
    {
        // Enable end screen UI
        if (endScreenRoot != null)
            endScreenRoot.SetActive(true);

        // Display stroke count
        if (strokesText != null)
            strokesText.text = "Strokes: " + strokes;

        // Display star rating based on performance
        if (starText != null)
            starText.text = GetStarRating(strokes);
    }

    // =========================
    // Convert stroke count into star rating
    // =========================
    private string GetStarRating(int strokes)
    {
        if (strokes <= 12)
            return "Score: 3/3"; // Best performance

        if (strokes <= 16)
            return "Score: 2/3"; // Medium performance

        return "Score: 1/3";     // Lowest performance
    }

    // =========================
    // Restart button handler
    // =========================
    public void RestartLevel()
    {
        // Start coroutine so we can delay reload (for SFX)
        StartCoroutine(RestartWithDelay());
    }

    // =========================
    // Restart with optional sound delay
    // =========================
    private System.Collections.IEnumerator RestartWithDelay()
    {
        float delay = 0.5f; // Default delay if no sound

        // Play button sound if assigned
        if (buttonSFX != null)
        {
            AudioSource.PlayClipAtPoint(buttonSFX, Camera.main.transform.position);

            // Use actual clip length for better timing
            delay = buttonSFX.length;
        }

        // Wait before restarting scene
        yield return new WaitForSeconds(delay);

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}