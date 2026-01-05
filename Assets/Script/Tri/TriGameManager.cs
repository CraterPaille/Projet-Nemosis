using UnityEngine;
using UnityEngine.SceneManagement;

public class TriGameManager : MonoBehaviour
{
    public static TriGameManager Instance;

    [Header("Game Settings")]
    public float gameDuration = 60f;
    private float remainingTime;
    private int score = 0;

    public bool IsPlaying { get; private set; } = false;

    [Header("References")]
    public Spawner spawner;
    public UIManagerTri uiManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        remainingTime -= Time.deltaTime;
        uiManager.UpdateTimer(remainingTime);

        if (remainingTime <= 0)
            EndGame();
    }

    public void StartGame()
    {
        score = 0;
        remainingTime = gameDuration;
        IsPlaying = true;

        uiManager.HideEndScreen();
        uiManager.UpdateScore(score);
        spawner.StartSpawning();
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    public void EndGame()
    {
        IsPlaying = false;
        spawner.StopSpawning();

        GameObject[] souls = GameObject.FindGameObjectsWithTag("Soul");
        foreach (GameObject soul in souls)
        {
            Destroy(soul);
        }

        uiManager.ShowEndScreen(score);

        // Conversion score -> Or + Foi
        if (GameManager.Instance != null)
        {
            float orGain  = score / 400f;  // à ajuster
            float foiGain = score / 800f;  // à ajuster

            if (orGain != 0f)
                GameManager.Instance.changeStat(StatType.Or, orGain);
            if (foiGain != 0f)
                GameManager.Instance.changeStat(StatType.Foi, foiGain);

            Debug.Log($"[Tri] Score={score} -> Or +{orGain}, Foi +{foiGain}");
        }
    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
