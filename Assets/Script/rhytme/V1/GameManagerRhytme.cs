using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManagerRhytme : MonoBehaviour
{
    public static GameManagerRhytme instance;

    public AudioSource theMusic;
    public BeatScroller theBS;

    public bool StartPlaying;

    [Header("Score")]
    public int currentScore;
    public int ScorePerNote = 100;
    public int ScorePerGoodNote = 125;
    public int ScorePerPerfectNote = 150;

    public int currentMultiplier = 1;
    public int multiplierTracker;
    public int[] multiplierThresholds;

    public TMP_Text ScoreText;
    public TMP_Text MultiText;

    private PunchScale _scorePunch;
    private PunchScale _multiPunch;

    [Header("Stats performance")]
    public int normalHits;
    public int goodHits;
    public int perfectHits;
    public int missedHits;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private Coroutine vibrationCoroutine;

    [Header("Feedback lanes")]
    [SerializeField] private LaneHighlight[] laneHighlights;

    [Header("Feedback global")]
    [SerializeField] private CameraShakeRhytm missShake;

    [Header("Layout & Infos UI")]
    [SerializeField] private RhythmLaneLayout laneLayout;
    [SerializeField] private TMP_Text invertInfoText;

    private float _speedMultiplier = 1f;
    public float SpeedMultiplier => _speedMultiplier;
    private float _difficultyMultiplier = 1f;

    private bool _invertControlsRhythm = false;

    // dérivé de la carte
    private float _chaosLevel = 0f;
    private float _rewardMult = 1f;
    private float _rewardFlat = 0f;
    private bool _oneMistakeFail = false;

    [Header("Tutoriel")]
    public MiniGameTutorialPanel tutorialPanel; // à assigner dans l'inspector
    public VideoClip tutorialClip; // à assigner dans l'inspector
    private bool tutorialValidated = false; // Ajouté


    private void Awake()
    {
        instance = this;

        if (InputManager.Instance == null)
        {
            Debug.LogError("GameManagerRhytme : InputManager non trouvé !");
            return;
        }

        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;

        keyboardControls.Rhytm.Lane0.performed += _ => PressLane(0);
        keyboardControls.Rhytm.Lane0.canceled += _ => ReleaseLane(0);

        keyboardControls.Rhytm.Lane1.performed += _ => PressLane(1);
        keyboardControls.Rhytm.Lane1.canceled += _ => ReleaseLane(1);

        keyboardControls.Rhytm.Lane2.performed += _ => PressLane(2);
        keyboardControls.Rhytm.Lane2.canceled += _ => ReleaseLane(2);

        keyboardControls.Rhytm.Lane3.performed += _ => PressLane(3);
        keyboardControls.Rhytm.Lane3.canceled += _ => ReleaseLane(3);

        gamepadControls.Rhytm.Lane0.performed += _ => PressLane(0);
        gamepadControls.Rhytm.Lane0.canceled += _ => ReleaseLane(0);
        gamepadControls.Rhytm.Lane1.performed += _ => PressLane(1);
        gamepadControls.Rhytm.Lane1.canceled += _ => ReleaseLane(1);
        gamepadControls.Rhytm.Lane2.performed += _ => PressLane(2);
        gamepadControls.Rhytm.Lane2.canceled += _ => ReleaseLane(2);
        gamepadControls.Rhytm.Lane3.performed += _ => PressLane(3);
        gamepadControls.Rhytm.Lane3.canceled += _ => ReleaseLane(3);
    }

    private void OnEnable()
    {
        keyboardControls?.Rhytm.Enable();
        gamepadControls?.Rhytm.Enable();
    }

    private void OnDisable()
    {
        keyboardControls?.Rhytm.Disable();
        gamepadControls?.Rhytm.Disable();
        StopVibration();
    }

    private void OnApplicationQuit()
    {
        StopVibration();
    }

    private void Start()
    {
        ShowTutorialAndStart();

        // Effet carte mini-jeu, si présent
        ApplyMiniGameCardIfAny();

        // Met à jour le layout visuel + texte d’info
        if (laneLayout != null)
            laneLayout.ApplyInverted(_invertControlsRhythm);
        if (invertInfoText != null)
            invertInfoText.gameObject.SetActive(_invertControlsRhythm);

        theMusic.pitch = _speedMultiplier;   // 2f => double tempo audio
        Debug.Log($"[Rhytme] Musique pitch réglé à {_speedMultiplier}x");

        currentScore = 0;
        normalHits = 0;
        goodHits = 0;
        perfectHits = 0;
        missedHits = 0;

        ScoreText.text = "Score : 0";

        _scorePunch = ScoreText != null ? ScoreText.GetComponent<PunchScale>() : null;
        _multiPunch = MultiText != null ? MultiText.GetComponent<PunchScale>() : null;

        NoteSpawner spawner = FindFirstObjectByType<NoteSpawner>();
        Transform activator = GameObject.FindGameObjectWithTag("Activator")?.transform;

        if (spawner != null && activator != null && theBS != null && spawner.chartLoader != null)
        {
            float bpm = spawner.chartLoader.loadedChart.bpm;
            float secondsPerBeat = 60f / bpm;
            float spawnLeadTime = spawner.beatsAhead * secondsPerBeat;
            float distance = spawner.transform.position.y - activator.position.y;

            if (spawnLeadTime > 0.01f)
            {
                theBS.beatTempo = distance / spawnLeadTime * _speedMultiplier;
            }
        }
    }

    private void ApplyMiniGameCardIfAny()
    {
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.Rhythm)
            return;

        _speedMultiplier = Mathf.Max(0.1f, card.speedMultiplier);
        _difficultyMultiplier = Mathf.Max(0.5f, card.difficultyMultiplier);

        float spawnMult = Mathf.Max(0.1f, card.spawnRateMultiplier);
        float scoreMult = _difficultyMultiplier * spawnMult;

        ScorePerNote = Mathf.RoundToInt(ScorePerNote * scoreMult);
        ScorePerGoodNote = Mathf.RoundToInt(ScorePerGoodNote * scoreMult);
        ScorePerPerfectNote = Mathf.RoundToInt(ScorePerPerfectNote * scoreMult);

        _invertControlsRhythm = card.invertControls;

        _chaosLevel = Mathf.Clamp01(card.chaosLevel);
        _rewardMult = Mathf.Max(0.1f, card.rewardMultiplier);
        _rewardFlat = card.rewardFlatBonus;
        _oneMistakeFail = card.oneMistakeFail;

        Debug.Log($"[Rhytme] Carte appliquée : {card.cardName}, speed x{_speedMultiplier}, diff x{_difficultyMultiplier}, spawnRateMult x{spawnMult}, chaos={_chaosLevel}, rewardMult={_rewardMult}, rewardFlat={_rewardFlat}, invert={_invertControlsRhythm}, oneMistakeFail={_oneMistakeFail}");

        runtime.Clear();
    }
    public void ShowTutorialAndStart()
    {
        InputAction[] actions = {
            InputManager.Instance.keyboardControls.Rhytm.Lane0,
            InputManager.Instance.keyboardControls.Rhytm.Lane1,
            InputManager.Instance.keyboardControls.Rhytm.Lane2,
            InputManager.Instance.keyboardControls.Rhytm.Lane3
        };
        tutorialPanel.Show(
      "Rhythm",
      new[] { keyboardControls.Rhytm.Lane0, keyboardControls.Rhytm.Lane1, keyboardControls.Rhytm.Lane2, keyboardControls.Rhytm.Lane3 },
      new[] { gamepadControls.Rhytm.Lane0, gamepadControls.Rhytm.Lane1, gamepadControls.Rhytm.Lane2, gamepadControls.Rhytm.Lane3 },
      tutorialClip
        );
        tutorialPanel.continueButton.onClick.RemoveAllListeners();
        tutorialPanel.continueButton.onClick.AddListener(() => {
            tutorialPanel.Hide();
            tutorialValidated = true;
        });
    }
    private void Update()
    {
        // Ne démarre le jeu que si le tutoriel a été validé
        if (!tutorialValidated)
            return;

        if (!StartPlaying)
        {
            StartPlaying = true;
            theBS.HasStarted = true;
            Invoke(nameof(StartMusic), 0.05f);
        }

        if (StartPlaying && theMusic != null && !theMusic.isPlaying)
        {
            StartPlaying = false;
            EndSong();
        }
    }

    void StartMusic()
    {
        theMusic.Play();
    }

    // --- mapping lane si inversion active ---
    private int MapLane(int lane)
    {
        if (!_invertControlsRhythm)
            return lane;

        // 0 <-> 3, 1 <-> 2 (pour 4 lanes)
        switch (lane)
        {
            case 0: return 3;
            case 1: return 2;
            case 2: return 1;
            case 3: return 0;
            default: return lane;
        }
    }

    // Permet aux autres scripts (NoteSpawner, etc.) de récupérer la lane logique
    public int GetLogicalLane(int chartLane)
    {
        return MapLane(chartLane);
    }

    void PressLane(int lane)
    {
        int logicLane = MapLane(lane);

        if (laneHighlights != null && logicLane >= 0 && logicLane < laneHighlights.Length)
            laneHighlights[logicLane]?.Trigger();

        NoteObject note = NoteObject.GetClosestNoteInLane(logicLane);
        if (note == null) return;

        if (note.duration > 0f)
        {
            if (!note.finished && note.IsHoldNotePressable())
                note.StartHold();
        }
        else
        {
            if (!note.finished && note.IsSimpleNotePressable())
                note.TryHit();
        }
    }

    void ReleaseLane(int lane)
    {
        int logicLane = MapLane(lane);

        NoteObject note = NoteObject.GetClosestNoteInLane(logicLane);
        if (note == null) return;

        if (note.duration > 0f && !note.finished)
        {
            note.ReleaseHold();
        }
    }

    public void NormalHit()
    {
        normalHits++;
        HandleMultiplier();
        AddScore(ScorePerNote);
        Vibrate(0.2f, 0.2f, 0.09f);
    }

    public void GoodHit()
    {
        goodHits++;
        HandleMultiplier();
        AddScore(ScorePerGoodNote);
        Vibrate(0.35f, 0.35f, 0.11f);
    }

    public void PerfectHit()
    {
        perfectHits++;
        HandleMultiplier();
        AddScore(ScorePerPerfectNote);
        Vibrate(0.1f, 0.1f, 0.04f);
    }

    void AddScore(int baseScore)
    {
        // jitter chaos sur le score (léger)
        float chaosFactor = 1f + Random.Range(-_chaosLevel, _chaosLevel);
        int finalBase = Mathf.Max(0, Mathf.RoundToInt(baseScore * chaosFactor));

        currentScore += finalBase * currentMultiplier;
        ScoreText.text = "Score : " + currentScore;

        _scorePunch?.Play();
    }

    void HandleMultiplier()
    {
        if (multiplierThresholds == null || multiplierThresholds.Length == 0)
        {
            MultiText.text = "Multiplier X" + currentMultiplier;
            return;
        }

        if (currentMultiplier - 1 < multiplierThresholds.Length)
        {
            multiplierTracker++;

            if (multiplierTracker >= multiplierThresholds[currentMultiplier - 1])
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        MultiText.text = "Multiplier X" + currentMultiplier;
        _multiPunch?.Play();
    }

    public void NoteMissed()
    {
        missedHits++;
        currentMultiplier = 1;
        multiplierTracker = 0;
        MultiText.text = "Multiplier X1";

        Vibrate(0.6f, 0.6f, 0.12f);
        missShake?.Play();

        // --- ONE MISTAKE FAIL ---
        if (_oneMistakeFail)
        {
            Debug.Log("[Rhytme] Mode oneMistakeFail : note ratée -> fin immédiate de la chanson.");
            // on arrête la musique et on termine
            if (theMusic != null && theMusic.isPlaying)
                theMusic.Stop();

            EndSong();
        }
    }

    // --- Fin du mini-jeu ---
    public void EndSong()
    {
        if (theBS != null)
            theBS.HasStarted = false;

        int totalNotes = normalHits + goodHits + perfectHits + missedHits;
        float hitCount = normalHits + goodHits + perfectHits;
        float accuracy = totalNotes > 0 ? (hitCount / totalNotes) : 0f;

        Debug.Log($"[Rhytme] Fin chanson - Score={currentScore}, Acc={accuracy:P1}");

        // Conversion score -> stats globales (Rythme -> Foi)
        if (GameManager.Instance != null)
        {
            float baseFoi = currentScore / 500f;
            float foiGain = baseFoi * _rewardMult + _rewardFlat;

            if (foiGain != 0f)
            {
                GameManager.Instance.changeStat(StatType.Foi, foiGain);
                Debug.Log($"[Rhytme] Score={currentScore} -> Foi +{foiGain} (mult x{_rewardMult}, flat +{_rewardFlat})");
            }
        }
        else
        {
            Debug.LogWarning("[Rhytme] GameManager.Instance est null, impossible d'appliquer les stats.");
        }
    }

    public void Vibrate(float left, float right, float duration)
    {
        if (Gamepad.current == null)
            return;

        if (vibrationCoroutine != null)
            StopCoroutine(vibrationCoroutine);

        vibrationCoroutine = StartCoroutine(VibrationRoutine(left, right, duration));
    }

    private System.Collections.IEnumerator VibrationRoutine(float left, float right, float duration)
    {
        Gamepad.current.SetMotorSpeeds(left, right);
        yield return new WaitForSeconds(duration);
        StopVibration();
    }

    private void StopVibration()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);

        vibrationCoroutine = null;
    }

    public void AdjustNoteVisual(NoteObject note, NoteData data)
    {
        Transform visual = note.transform.Find("visual");
        if (visual != null)
        {
            switch (data.lane)
            {
                case 0: visual.right = Vector3.left; break;
                case 1: visual.right = Vector3.down; break;
                case 2: visual.right = Vector3.up; break;
                case 3: visual.right = Vector3.right; break;
            }
        }
    }

    public void OnQuitMiniGame()
    {
        // On applique les stats avec le score actuel avant de quitter
        if (StartPlaying)
        {
            if (theMusic != null && theMusic.isPlaying)
                theMusic.Stop();

            EndSong();
        }

        SceneManager.LoadScene("SampleScene");
    }

   
}