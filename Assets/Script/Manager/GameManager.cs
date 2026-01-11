using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DayTime { Matin, Aprem }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float statLossMultiplier = 1.0f;

    [Tooltip("Multiplicateur appliqué aux stats")]
    public Dictionary<StatType, float> Multiplicateur = new Dictionary<StatType, float>();
    public Dictionary<StatType, float> Valeurs = new Dictionary<StatType, float>();

    [Tooltip("Nombre de cartes à piocher par set")]
    public int cardsToDraw = 3; // separer en set apres les tests ?

    [Header("Jour actuel")]
    public int currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation, }
    public GameMode currentGameMode;

    [Header("Event System")]
    public EventScheduler eventScheduler;

    [Header("Cartes disponibles (Set Village)")]
    public VillageCardCollectionSO villageCardCollection;

    [Header("Mini-jeu du dimanche")]
    public MiniGameLauncher miniGameLauncher;

    [Header("Durée de la campagne")]
    [Tooltip("Nombre total de jours dans une partie (1 mois = 28 jours)")]
    public int totalDays = 28;
    [Tooltip("Scène chargée quand la campagne est terminée")]
    public string endSceneName = "Menu_principal";

    private bool campaignFinished = false;

    public EffectSO effet;
    public readonly string[] weekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Deuxième instance détectée, je me détruis.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[GameManager] Instance initialisée et marquée DontDestroyOnLoad.");
    }

    void Start()
    {
        foreach (var kvp in Valeurs)
        {
            Debug.Log($"[DEBUG] Dictionnaire contient {kvp.Key} = {kvp.Value}");
        }

        Debug.Log($"StatType values: {string.Join(", ", System.Enum.GetNames(typeof(StatType)))}");

        foreach (StatType stat in StatType.GetValues(typeof(StatType)))
        {
            Multiplicateur[stat] = 1f;
            Debug.Log($"[GameManager] Initializing stat {stat} with multiplier {Multiplicateur[stat]}");
            Valeurs[stat] = 50f;
            changeStat(stat, 0f);
        }

        changeStat(StatType.Nemosis, -50f);

        if (SceneManager.GetActiveScene().name == "SampleScene" && UIManager.Instance != null)
        {
            ChooseGameMode();
        }


    }

    public void addHumain()  { changeStat(StatType.Human, 10); }
    public void lessHumain() { changeStat(StatType.Human, -10); }
    public void addOr()      { changeStat(StatType.Or, 100); }
    public void addFood()    { changeStat(StatType.Food, 100); }

    public void changeStat(StatType type, float amount)
    {
        Debug.Log($"Changing stat {type} by {amount} with multiplier {Multiplicateur[type]}");
        float delta = (amount > 0) ? amount * Multiplicateur[type] : amount;
        Valeurs[type] += delta;
        GameEvents.TriggerStatChanged(type, Valeurs[type]);
        Debug.Log($"Stat {type} changed by {delta}, new value: {Valeurs[type]}");
    }

    /// <summary>
    /// Choix du mode de jeu "normal".
    /// - Si dimanche matin -> mini-jeu aléatoire automatiquement
    /// - Sinon -> affiche le ModeChoiceUI (panel jour/mode)
    /// </summary>
    public void ChooseGameMode()
    {
        if (campaignFinished) return;

        // Dimanche matin -> mini-jeu auto
        if (currentWeekDay == "Dimanche" && currentTime == DayTime.Matin)
        {
            LaunchSundayMiniGame();
            return;
        }

        // Cas standard : afficher le panel de choix de mode de jeu
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetUIActive(true);
            UIManager.Instance.GameModeChoice();
        }
    }

    /// <summary>
    /// Appelé depuis un bouton du ModeChoiceUI pour ouvrir le panel des cartes mini-jeu.
    /// (Ne dépend plus de ChooseGameMode)
    /// </summary>
    public void OpenMiniJeuCardPanel()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.SetUIActive(true);
        UIManager.Instance.ShowMiniJeuCardPanel();
    }

    private void LaunchSundayMiniGame()
    {
        Debug.Log("[GameManager] Dimanche matin : lancement automatique d'un mini-jeu aléatoire !");

        if (miniGameLauncher != null)
        {
            miniGameLauncher.LaunchRandomSundayMiniGame();
            Debug.Log("[GameManager] Mini-jeu lancé via MiniGameLauncher.");
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.SetUIActive(false);

            SceneManager.LoadScene("RhythmScene");
        }
    }

    public void ChooseVillage()
    {
        currentGameMode = GameMode.village;
        VillageManager.Instance.AfficheBuildings();
    }

    public void ChooseVillageCards()
    {
        currentGameMode = GameMode.VillageCard;
        UIManager.Instance.VillageCardChoice(villageCardCollection, cardsToDraw);
    }

    public void ChooseRelation()
    {
        currentGameMode = GameMode.Relation;
    }

    public void EndHalfDay()
    {
        if (campaignFinished) return;

        var schedule = FindFirstObjectByType<ScheduleShow>();
        if (schedule != null)
        {
            schedule.UpdateWeekText();
        }

        if (currentTime == DayTime.Matin)
        {
            currentTime = DayTime.Aprem;
            GameEvents.TriggerMorningEnd();
        }
        else
        {
            currentTime = DayTime.Matin;
            currentDay++;
            currentWeekDay = weekDays[(currentDay - 1) % 7];
            EndDay();
        }

        Debug.Log($"Ending half day: {currentTime} of day {currentDay} currentWeekDay = {currentWeekDay}");

        if (currentDay > totalDays)
        {
            EndCampaign();
            return;
        }

        if (eventScheduler != null)
        {
            bool eventActive = eventScheduler.CheckAndTriggerEvent(currentDay, currentTime);
            if (eventActive)
            {
                Debug.Log("Événement actif, gameplay normal en pause.");
                return;
            }
        }

        ChooseGameMode();
    }

    public void EndDay()
    {
        if (campaignFinished) return;

        var schedule = FindFirstObjectByType<ScheduleShow>();
        if (schedule != null)
        {
            schedule.UpdateWeekText();
        }

        currentWeekDay = weekDays[(currentDay - 1) % 7];

        float foodLoss = Mathf.Round(Valeurs[StatType.Human] * 0.1f);
        if (Valeurs[StatType.Food] >= foodLoss)
        {
            changeStat(StatType.Food, -foodLoss);
        }
        else
        {
            changeStat(StatType.Human, -10);
            changeStat(StatType.Food, -Valeurs[StatType.Food]);
        }

        Debug.Log($"Fin de journée : Perte de nourriture de {foodLoss}, Nourriture restante : {Valeurs[StatType.Food]}");
        Valeurs[StatType.Food] -= foodLoss;

        GameEvents.TriggerDayEnd();

        if (currentDay >= totalDays && currentTime == DayTime.Aprem)
        {
            EndCampaign();
        }
    }

    private void EndCampaign()
    {
        if (campaignFinished) return;
        campaignFinished = true;

        Debug.Log($"[GameManager] Fin de campagne : {totalDays} jours écoulés.");

        UIManager.Instance?.SetUIActive(false);

        if (!string.IsNullOrEmpty(endSceneName))
            SceneManager.LoadScene(endSceneName);
    }

    #region DEBUG

    public void DebugSkipToSunday()
    {
        if (campaignFinished) return;

        int currentDayIndex = (currentDay - 1) % 7; // 0 = Lundi, 6 = Dimanche
        int daysUntilSunday = (6 - currentDayIndex + 7) % 7;

        if (daysUntilSunday == 0 && currentTime == DayTime.Aprem)
            daysUntilSunday = 7;

        currentDay += daysUntilSunday;
        currentTime = DayTime.Matin;
        currentWeekDay = "Dimanche";

        Debug.Log($"[DEBUG] Saut au dimanche matin ! Jour {currentDay}");

        UIManager.Instance?.changeDateUI();
        ChooseGameMode();
    }

    #endregion

    // --- SAUVEGARDE / CHARGEMENT ---
    [System.Serializable]
    private class SaveData
    {
        public int currentDay;
        public int currentTime;      // cast de l'enum DayTime
        public string currentWeekDay;

        public Dictionary<StatType, float> stats;
    }

    private const string SAVE_KEY = "GAME_SAVE_v1";

    public void SaveGame()
    {
        var data = new SaveData
        {
            currentDay = currentDay,
            currentTime = (int)currentTime,
            currentWeekDay = currentWeekDay,
            stats = new Dictionary<StatType, float>(Valeurs)
        };

        // PlayerPrefs ne sait pas stocker un Dictionary directement, on passe par du JSON simple.
        // Comme Unity ne sérialise pas les Dictionary dans JsonUtility, on fait une petite structure intermédiaire.
        var wrapper = new SaveWrapper();
        wrapper.currentDay = data.currentDay;
        wrapper.currentTime = data.currentTime;
        wrapper.currentWeekDay = data.currentWeekDay;

        wrapper.statKeys = new List<string>();
        wrapper.statValues = new List<float>();

        foreach (var kvp in data.stats)
        {
            wrapper.statKeys.Add(kvp.Key.ToString());
            wrapper.statValues.Add(kvp.Value);
        }

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[GameManager] Partie sauvegardée : " + json);
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.LogWarning("[GameManager] Aucune sauvegarde trouvée.");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        var wrapper = JsonUtility.FromJson<SaveWrapper>(json);
        if (wrapper == null)
        {
            Debug.LogError("[GameManager] Échec du chargement de la sauvegarde.");
            return;
        }

        currentDay = wrapper.currentDay;
        currentTime = (DayTime)wrapper.currentTime;
        currentWeekDay = wrapper.currentWeekDay;

        // Reconstruire le dictionnaire de stats
        Valeurs.Clear();
        Multiplicateur.Clear();

        for (int i = 0; i < wrapper.statKeys.Count; i++)
        {
            if (System.Enum.TryParse(wrapper.statKeys[i], out StatType stat))
            {
                float value = wrapper.statValues[i];
                Valeurs[stat] = value;
                // On remet un multiplicateur par défaut si besoin
                if (!Multiplicateur.ContainsKey(stat))
                    Multiplicateur[stat] = 1f;

                GameEvents.TriggerStatChanged(stat, value);
            }
        }

        UIManager.Instance?.changeDateUI();

        Debug.Log("[GameManager] Partie chargée : " + json);
    }

    [System.Serializable]
    private class SaveWrapper
    {
        public int currentDay;
        public int currentTime;
        public string currentWeekDay;

        public List<string> statKeys;
        public List<float> statValues;
    }
}