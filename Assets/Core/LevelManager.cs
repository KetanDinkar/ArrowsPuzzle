using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LifeSystem life;
    public GameObject winPopup, losePopup;
    public UnityEngine.UI.Button hintButton;

    int remaining;
    bool ended;

    void Start()
    {
        var loader = GetComponent<LevelLoader>();
        remaining = loader.level.lines.Count;
        ArrowLine.OnCompleted += OnLineCompleted;
        life.OnLivesEmpty += Lose;
    }

    void OnDestroy()
    {
        ArrowLine.OnCompleted -= OnLineCompleted;
        if (life != null) life.OnLivesEmpty -= Lose;
    }

    void OnLineCompleted(ArrowLine line)
    {
        if (ended) return;
        remaining--;
        if (remaining <= 0)
        {
            ended = true;
            winPopup.SetActive(true);
            if (hintButton) hintButton.interactable = false;
        }
    }

    void Lose()
    {
        if (ended) return;
        ended = true;
        losePopup.SetActive(true);
        if (hintButton) hintButton.interactable = false;
        foreach (var l in FindObjectsByType<ArrowLine>(FindObjectsSortMode.None))
        {
            l.ClearHighlight();
            l.enabled = false;
        }
    }

    public void Retry()
    {
        ArrowLine.OnCompleted = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}