using UnityEngine;
using TMPro;

public class StartOverlayUI : MonoBehaviour
{
    // Singleton instance so other scripts can easily access this UI
    public static StartOverlayUI Instance;

    // =========================
    // UI References
    // =========================
    public GameObject titleTextObject;   // Title screen object (e.g., game name)
    public TMP_Text bestStrokesText;    // Displays best score

    // Key used for saving best score in PlayerPrefs
    private const string BestStrokesKey = "BestStrokes";

    // =========================
    // Sound Effects
    // =========================
    [Header("SFX")]
    public AudioClip buttonSFX; // Sound played when reset button is pressed

    private void Awake()
    {
        // Set up singleton instance
        Instance = this;
    }

    private void Start()
    {
        // Ensure title is visible at start
        if (titleTextObject != null)
            titleTextObject.SetActive(true);

        // Load and display saved best score
        if (bestStrokesText != null)
        {
            if (PlayerPrefs.HasKey(BestStrokesKey))
                bestStrokesText.text = "Personal Best: " + PlayerPrefs.GetInt(BestStrokesKey);
            else
                bestStrokesText.text = "Personal Best: --"; // Default if no score yet
        }
    }

    // =========================
    // Hide start overlay (called when game begins)
    // =========================
    public void HideOverlay()
    {
        // Hide title
        if (titleTextObject != null)
            titleTextObject.SetActive(false);

        // Hide best score text
        if (bestStrokesText != null)
            bestStrokesText.gameObject.SetActive(false);
    }

    // =========================
    // Save best score if it's better than previous
    // =========================
    public void SaveBestStrokes(int strokes)
    {
        // Save only if:
        // 1. No previous score exists OR
        // 2. New score is better (lower strokes)
        if (!PlayerPrefs.HasKey(BestStrokesKey) || strokes < PlayerPrefs.GetInt(BestStrokesKey))
        {
            PlayerPrefs.SetInt(BestStrokesKey, strokes);
            PlayerPrefs.Save(); // Force save to disk
        }
    }

    // =========================
    // Reset saved best score
    // =========================
    public void ResetBestStrokes()
    {
        // Play button click sound at camera position
        if (buttonSFX != null)
            AudioSource.PlayClipAtPoint(buttonSFX, Camera.main.transform.position);

        // Remove saved score
        PlayerPrefs.DeleteKey(BestStrokesKey);
        PlayerPrefs.Save();

        // Update UI to reflect reset
        if (bestStrokesText != null)
            bestStrokesText.text = "Personal Best: --";
    }
}