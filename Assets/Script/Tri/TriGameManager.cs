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

    // paramètres dérivés de la carte
    private float _spawnChaos = 0f;
    private float _rewardMult = 1f;
    private float _rewardFlat = 0f;
    private bool _oneMistakeFail = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _baseGameDuration = gameDuration;

        if (spawner != null)
        {
            _baseSpawnInterval = spawner.spawnInterval;
            // hypothèse : 1 point de score par âme de base
            _baseScorePerSoul = 1;
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

    public void AddScore(int soulsCount)
    {
        // on garde l’idée d’un score par âme, ici multiplié par la difficulté
        score += soulsCount * _baseScorePerSoul;
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

        // Conversion score -> Or + Foi avec rewardMultiplier / rewardFlatBonus
        if (GameManager.Instance != null)
        {
            float baseOr  = score / 2f;
            float baseFoi = score / 10f;

            float orGain  = baseOr  * _rewardMult + _rewardFlat;
            float foiGain = baseFoi * _rewardMult + _rewardFlat;

            if (orGain != 0)
                GameManager.Instance.changeStat(StatType.Or, orGain);
            if (foiGain != 0)
                GameManager.Instance.changeStat(StatType.Foi, foiGain);

            Debug.Log($"[Tri] Score={score} -> Or +{orGain}, Foi +{foiGain} (mult x{_rewardMult}, flat +{_rewardFlat})");
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
        float spawnMult = Mathf.Max(0.1f, card.spawnRateMultiplier);

        // plus de vitesse => partie plus courte
        gameDuration = _baseGameDuration / speedMult;

        if (spawner != null)
        {
            // vitesse + densité
            float interval = _baseSpawnInterval / speedMult;
            interval /= spawnMult;

            // appliquer un chaos multiplicatif autour de l’intervalle de base dans le Spawner
            _spawnChaos = Mathf.Clamp01(card.chaosLevel);

            spawner.spawnInterval = interval;
        }

        // score par âme augmente avec la difficulté
        _baseScorePerSoul = Mathf.Max(1, Mathf.RoundToInt(_baseScorePerSoul * diffMult));

        // gains de stats globaux
        _rewardMult = Mathf.Max(0.1f, card.rewardMultiplier);
        _rewardFlat = card.rewardFlatBonus;
        _oneMistakeFail = card.oneMistakeFail;

        Debug.Log($"[Tri] Carte appliquée : {card.cardName}, duration={gameDuration}, spawnInterval={spawner.spawnInterval}, scorePerSoul={_baseScorePerSoul}, chaos={_spawnChaos}, rewardMult={_rewardMult}, rewardFlat={_rewardFlat}, oneMistakeFail={_oneMistakeFail}");

        runtime.Clear();
    }

    // exposé pour que le Spawner puisse récupérer le chaos (si tu veux l’utiliser dedans)
    public float GetSpawnChaos()
    {
        return _spawnChaos;
    }

    public void OnMistake()
    {
        if (!IsPlaying) return;

        if (_oneMistakeFail)
        {
            Debug.Log("[Tri] Mode oneMistakeFail : erreur de tri -> fin immédiate de la partie.");
            EndGame();
        }
    }
}
