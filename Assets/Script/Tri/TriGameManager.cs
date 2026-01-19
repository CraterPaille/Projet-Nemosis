using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

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

    [Header("Tutorial")]
    public MiniGameTutorialPanel tutorialPanel; // à assigner dans l'inspector
    public VideoClip tutorialClip; // à assigner dans l'inspector
    private bool tutorialValidated = false;

    [Header("Paliers étoiles")]
    public int[] starThresholds = new int[3] { 30, 60, 100 };
    private bool[] starGiven = new bool[3];
    [Header("UI Étoiles")]
    public UnityEngine.UI.Image[] starImages;
    public Sprite starOnSprite;
    public Sprite starOffSprite;

    private void Awake()
    {
        // on vérifie qu'il n'y a qu'une instance de ce GameManager si il y en a plusieurs on détruit le nouveau
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    private void Start()
    {
        _baseGameDuration = gameDuration;

        // récup les valeurs de base du Spawner
        if (spawner != null)
        {
            _baseSpawnInterval = spawner.spawnInterval;
            // hypothèse : 1 point de score par âme de base
            _baseScorePerSoul = 1;
        }

        ApplyMiniGameCardIfAny();

        ShowTutorialAndStart();

        starGiven = new bool[3];
        UpdateStarsUI();
    }

    private void Update()
    {
        // si tuto pas validé, on ne lance pas le timer
        if (!tutorialValidated) { return; }

        if (!IsPlaying) return;
        // Maj le timer
        remainingTime -= Time.deltaTime;
        uiManager.UpdateTimer(remainingTime);

        if (remainingTime <= 0)
            EndGame();
    }

    public void StartGame()
    {
        // Réinitialisation des variables
        score = 0;
        remainingTime = gameDuration;
        IsPlaying = true;

        uiManager.HideEndScreen();
        uiManager.UpdateScore(score);
        spawner.StartSpawning();
    }


    public void AddScore(int soulsCount)
    {
        // Calcul score avec modifs de carte
        score += soulsCount * _baseScorePerSoul;
        uiManager.UpdateScore(score);

        // Paliers étoiles
        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (!starGiven[i] && score >= starThresholds[i])
            {
                starGiven[i] = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.changeStat(StatType.Foi, 5f); // ou autre stat
            }
        }
        UpdateStarsUI();
    }

    public void UpdateStarsUI()
    {
        // Met à jour l'affichage des étoiles
        for (int i = 0; i < starImages.Length; i++)
            starImages[i].sprite = starGiven[i] ? starOnSprite : starOffSprite;
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


        SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }


    private void ApplyMiniGameCardIfAny()
    {
        // vérifie si une carte est sélectionnée et applique ses effets
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.Tri)
            return;

        // applique les modifs de la carte
        float speedMult = Mathf.Max(0.1f, card.speedMultiplier);
        float diffMult  = Mathf.Max(0.5f, card.difficultyMultiplier);
        float spawnMult = Mathf.Max(0.1f, card.spawnRateMultiplier);

        // plus de vitesse = partie plus courte
        gameDuration = _baseGameDuration / speedMult;

        if (spawner != null)
        {
            // vitesse + densité
            float interval = _baseSpawnInterval / speedMult;
            interval /= spawnMult;

            // applique du chaos(plein d'effets différents) au spawn des âmes
            _spawnChaos = Mathf.Clamp01(card.chaosLevel);

            spawner.spawnInterval = interval;
        }

        // score par âme augmenter avec la difficulté
        _baseScorePerSoul = Mathf.Max(1, Mathf.RoundToInt(_baseScorePerSoul * diffMult));

        // gains de stats globaux
        _rewardMult = Mathf.Max(0.1f, card.rewardMultiplier);
        _rewardFlat = card.rewardFlatBonus;
        _oneMistakeFail = card.oneMistakeFail;

        Debug.Log($"[Tri] Carte appliquée : {card.cardName}, duration={gameDuration}, spawnInterval={spawner.spawnInterval}, scorePerSoul={_baseScorePerSoul}, chaos={_spawnChaos}, rewardMult={_rewardMult}, rewardFlat={_rewardFlat}, oneMistakeFail={_oneMistakeFail}");

        runtime.Clear();
    }

    //Getters pour les modifs de carte
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


    // Affiche le tuto et lance la partie après validation
    public void ShowTutorialAndStart()
    {
        tutorialPanel.ShowClick(
            "Tri",
            tutorialClip
        );
        tutorialPanel.continueButton.onClick.RemoveAllListeners();
        tutorialPanel.continueButton.onClick.AddListener(() => {
            tutorialPanel.Hide();
            tutorialValidated = true;
            StartGame();

        });
    }
}
