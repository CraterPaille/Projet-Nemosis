using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Stats performance")]
    public int normalHits;
    public int goodHits;
    public int perfectHits;
    public int missedHits;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private Coroutine vibrationCoroutine;

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
        currentScore = 0;
        normalHits = 0;
        goodHits = 0;
        perfectHits = 0;
        missedHits = 0;

        ScoreText.text = "Score : 0";

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
                theBS.beatTempo = distance / spawnLeadTime;
            }
        }
    }

    private void Update()
    {
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

    void PressLane(int lane)
    {
        NoteObject note = NoteObject.GetClosestNoteInLane(lane);
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
        NoteObject note = NoteObject.GetClosestNoteInLane(lane);
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
        currentScore += baseScore * currentMultiplier;
        ScoreText.text = "Score : " + currentScore;
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
    }

    public void NoteMissed()
    {
        missedHits++;
        currentMultiplier = 1;
        multiplierTracker = 0;
        MultiText.text = "Multiplier X1";

        Vibrate(0.6f, 0.6f, 0.12f);
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
            // Exemple : 500 points de score -> +1 Foi
            float foiGain = currentScore / 500;

            if (foiGain != 0f)
            {
                GameManager.Instance.changeStat(StatType.Foi, foiGain);
                Debug.Log($"[Rhytme] Score={currentScore} -> Foi +{foiGain}");
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