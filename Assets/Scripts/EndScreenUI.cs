using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance;

    public GameObject endScreenRoot;
    public TMP_Text strokesText;
    public TMP_Text starText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (endScreenRoot != null)
            endScreenRoot.SetActive(false);
    }

    public void ShowEndScreen(int strokes)
    {
        if (endScreenRoot != null)
            endScreenRoot.SetActive(true);

        if (strokesText != null)
            strokesText.text = "Strokes: " + strokes;

        if (starText != null)
            starText.text = GetStarRating(strokes);
    }

    private string GetStarRating(int strokes)
    {
        if (strokes <= 2)
            return "Score: 3/3";
        if (strokes <= 4)
            return "Score: 2/3";
        return "Score: 1/3";
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}