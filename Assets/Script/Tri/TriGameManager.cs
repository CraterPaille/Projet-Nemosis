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

    // Bases pour les modifs de carte
    private float _baseGameDuration;
    private float _baseSpawnInterval;
    private int _baseScorePerSoul;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Mémoriser les valeurs de base
        _baseGameDuration = gameDuration;

        if (spawner != null)
        {
            _baseSpawnInterval = spawner.spawnInterval;
            _baseScorePerSoul = spawner.scorePerSoul; // adapte si ton Spawner stocke le score ailleurs
        }

        ApplyMiniGameCardIfAny();
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
            float orGain  = score / 2;   // à ajuster
            float foiGain = score / 10;  // à ajuster

            if (orGain != 0)
                GameManager.Instance.changeStat(StatType.Or, orGain);
            if (foiGain != 0)
                GameManager.Instance.changeStat(StatType.Foi, foiGain);

            Debug.Log($"[Tri] Score={score} -> Or +{orGain}, Foi +{foiGain}");
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // --- Cartes mini-jeu ---
    private void ApplyMiniGameCardIfAny()
    {
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.Tri)
            return;

        float speedMult = Mathf.Max(0.1f, card.speedMultiplier);
        float diffMult  = Mathf.Max(0.5f, card.difficultyMultiplier);

        // Exemple : plus de vitesse => âmes plus fréquentes, partie plus courte
        gameDuration = _baseGameDuration / speedMult;

        if (spawner != null)
        {
            spawner.spawnInterval = _baseSpawnInterval / speedMult;
            spawner.scorePerSoul  = Mathf.RoundToInt(_baseScorePerSoul * diffMult);
        }

        Debug.Log($"[Tri] Carte appliquée : {card.cardName}, duration={gameDuration}, spawnInterval={spawner.spawnInterval}, scorePerSoul={spawner.scorePerSoul}");

        runtime.Clear();
    }
}
