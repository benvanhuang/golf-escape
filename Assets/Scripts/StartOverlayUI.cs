using UnityEngine;
using TMPro;

public class StartOverlayUI : MonoBehaviour
{
    public static StartOverlayUI Instance;

    public GameObject titleTextObject;
    public TMP_Text bestStrokesText;

    private const string BestStrokesKey = "BestStrokes";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (titleTextObject != null)
            titleTextObject.SetActive(true);

        if (bestStrokesText != null)
        {
            if (PlayerPrefs.HasKey(BestStrokesKey))
                bestStrokesText.text = "Personal Best: " + PlayerPrefs.GetInt(BestStrokesKey);
            else
                bestStrokesText.text = "Personal Best: --";
        }
    }

    public void HideOverlay()
    {
        if (titleTextObject != null)
            titleTextObject.SetActive(false);

        if (bestStrokesText != null)
            bestStrokesText.gameObject.SetActive(false);
    }

    public void SaveBestStrokes(int strokes)
    {
        if (!PlayerPrefs.HasKey(BestStrokesKey) || strokes < PlayerPrefs.GetInt(BestStrokesKey))
        {
            PlayerPrefs.SetInt(BestStrokesKey, strokes);
            PlayerPrefs.Save();
        }
    }
}