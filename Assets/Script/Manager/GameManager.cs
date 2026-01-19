using UnityEngine;
using System.Collections.Generic;
using UnityEditor.EditorTools;

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
    public int RerollMax = 1;
    public int RerollsRemaining = 1;

    // Effet pour le jeu global
    [Header("Jour actuel")]
    public float currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation,  }
    public GameMode currentGameMode;

    [Header("Event System")]
    public EventScheduler eventScheduler;

    [Header("Cartes disponibles (Set Village)")]
    public VillageCardCollectionSO villageCardCollection;

<<<<<<< Updated upstream
=======
    [Header("Mini-jeu du dimanche")]
    public MiniGameLauncher miniGameLauncher;

    [Header("Durée de la campagne")]
    [Tooltip("Nombre total de jours dans une partie (1 mois = 28 jours)")]
    public int totalDays = 28;
    [Tooltip("Scène chargée quand la campagne est terminée")]
    public string endSceneName = "Menu_principal";

   
    private bool campaignFinished = false;
>>>>>>> Stashed changes

    
    public EffectSO effet;
    public readonly string[] weekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        foreach (var kvp in Valeurs)
        {
            Debug.Log($"[DEBUG] Dictionnaire contient {kvp.Key} = {kvp.Value}");
        }

<<<<<<< Updated upstream
        Debug.Log($"StatType values: {string.Join(", ", System.Enum.GetNames(typeof(StatType)))}");
        // initialise le dictionnaire : chaque clé est un élément de StatType, valeur initiale 
=======
        if (SceneManager.GetActiveScene().name == "SampleScene" && UIManager.Instance != null)
        {
            ChooseGameMode();
        }
    }

    /// <summary>
    /// Initialise totalement la partie à l'état "nouvelle partie".
    /// Appelé quand il n'y a PAS de sauvegarde (ou après avoir tout reset).
    /// </summary>
    /// 
    #region Initialisation
    public void InitDefaultState()
    {
        Multiplicateur.Clear();
        Valeurs.Clear();
        MaxValeurs.Clear();

        Debug.Log("[GameManager] Initialisation de la nouvelle partie (état par défaut).");

>>>>>>> Stashed changes
        foreach (StatType stat in StatType.GetValues(typeof(StatType)))
        {
            
            Multiplicateur[stat] = 1f;
<<<<<<< Updated upstream
            Debug.Log($"[GameManager] Initializing stat {stat} with multiplier {Multiplicateur[stat]}");
            // initial value
            Valeurs[stat] = 50f;
            changeStat(stat, 0f);
        }
        changeStat(StatType.Nemosis, -50f);
        ChooseGameMode();
=======
            Valeurs[stat] = stat == StatType.Nemosis ? 0f : 50f;
            MaxValeurs[stat] = 100f; // valeur par défaut max
            changeStat(stat, 0f);
        }

        //changeStat(StatType.Nemosis, -50f);
>>>>>>> Stashed changes

    }

<<<<<<< Updated upstream
    public void addHumain()
    {
        changeStat(StatType.Human, 10);
    }

    public void lessHumain()
    {
        changeStat(StatType.Human, -10);
    }

    public void addOr()
    {
        changeStat(StatType.Or, 100);
    }

    public void addFood()
    {
        changeStat(StatType.Food, 100);
    }

    public void changeStat(StatType type, float amount)
    {
        Debug.Log($"Changing stat {type} by {amount} with multiplier {Multiplicateur[type]}");
        float delta = (amount > 0) ? amount * Multiplicateur[type] : amount;
        Valeurs[type] += delta;
        GameEvents.TriggerStatChanged(type, Valeurs[type]);
        Debug.Log($"Stat {type} changed by {delta}, new value: {Valeurs[type]}");
    }


    // Gere le choix du mode de jeu 
=======
    #endregion
    #region Set stats/Max
    public void changeStat(StatType type, float amount)
    {
        if (!Multiplicateur.ContainsKey(type))
        {
            Multiplicateur[type] = 1f;
        }
        if (!Valeurs.ContainsKey(type))
        {
            Valeurs[type] = 0f;
        }
        if (!MaxValeurs.ContainsKey(type))
        {
            MaxValeurs[type] = 100f;
        }
        float ancienvaleur = Valeurs[type];
        // Si amount positif, applique le multiplicateur ; si négatif, pas de multiplier
        float proposedDelta = (amount > 0) ? amount * Multiplicateur[type] : amount;
        // Clamp pour garder la valeur entre -MaxValeurs[type] et MaxValeurs[type]
        float clampedDelta = (ancienvaleur + proposedDelta) > MaxValeurs[type]
            ? MaxValeurs[type] - ancienvaleur
            : proposedDelta;
        Valeurs[type] += clampedDelta;
        Debug.Log($"Stat {type} etais a {ancienvaleur}. + {clampedDelta} = {Valeurs[type]}");
        GameEvents.TriggerStatChanged(type, Valeurs[type]);
    }

    /// <summary>
    /// Modifie la valeur maximale d'une stat (utilisé par certains effets).
    /// Retourne la nouvelle valeur maximale.
    /// </summary>
    public float changeStatMax(StatType type, float amount)
    {
        if (!MaxValeurs.ContainsKey(type))
            MaxValeurs[type] = 100f;
        MaxValeurs[type] += amount;
        // si on réduit max en dessous de la valeur courante, on clamp la valeur courante
        if (Valeurs.ContainsKey(type) && Valeurs[type] > MaxValeurs[type])
        {
            Valeurs[type] = MaxValeurs[type];
            GameEvents.TriggerStatChanged(type, Valeurs[type]);
        }
        // Debug.Log($"Max value for {type} changed by {amount}, new max: {MaxValeurs[type]}");
        return MaxValeurs[type];
    }
    #endregion
    /// <summary>
    /// Choix du mode de jeu "normal".
    /// - Si dimanche matin -> mini-jeu aléatoire automatiquement
    /// - Sinon -> affiche le ModeChoiceUI (panel jour/mode)
    /// </summary>
    #region Game Mode Choice
>>>>>>> Stashed changes
    public void ChooseGameMode()
    {
        UIManager.Instance.GameModeChoice();
    }
<<<<<<< Updated upstream
=======

    /// <summary>
    /// Appelé depuis un bouton du ModeChoiceUI pour ouvrir le panel des cartes mini-jeu.
    /// (Ne dépend plus de ChooseGameMode)
    /// </summary>


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

    public void MiniJeuCardPanelAnimation()
    {
        StartCoroutine(DOTweenManager.Instance.transitionChoixJeu(OpenMiniJeuCardPanel));
    }
    public void OpenMiniJeuCardPanel()
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.SetUIActive(true);
        UIManager.Instance.ShowMiniJeuCardPanel();
    }

    public void ChooseVillageAnimation()
    {
        StartCoroutine(DOTweenManager.Instance.transitionChoixJeu(ChooseVillage));
    }

>>>>>>> Stashed changes
    public void ChooseVillage()
    {
        currentGameMode = GameMode.village;
        VillageManager.Instance.AfficheBuildings();
    }

    public void ChooseVillageCardsTransition()
    {
        StartCoroutine(DOTweenManager.Instance.transitionChoixJeu(ChooseVillageCards));
    }

     public void ChooseVillageCards()
    {
        currentGameMode = GameMode.VillageCard;
        UIManager.Instance.VillageCardChoice(villageCardCollection, cardsToDraw);
    }

    public void RelationTransitionAnimation()
    {
        StartCoroutine(DOTweenManager.Instance.transitionChoixJeu(ChooseRelationTransition));
    }

    public void ChooseRelationTransition()
    {
        currentGameMode = GameMode.Relation;
        ChooseRelationUI.Instance.Open();
    }
    #endregion
    #region Time Management

    public void EndHalfDay()
    {
<<<<<<< Updated upstream
        // Passage à la demi-journée suivante
        
=======
        if (campaignFinished) return;
        RerollsRemaining = RerollMax;
        var schedule = FindFirstObjectByType<ScheduleShow>();
        if (schedule != null)
        {
            schedule.UpdateWeekText();
        }

>>>>>>> Stashed changes
        if (currentTime == DayTime.Matin)
        {
            currentTime = DayTime.Aprem;
            GameEvents.TriggerMorningEnd();
        }
        else
        {
            currentTime = DayTime.Matin;
            currentDay += 1f;
            currentWeekDay = weekDays[((int)currentDay - 1) % 7]; // C'est une responsibiliter que le Schen
            EndDay();
        }

        // Calcul du jour de la semaine
        
        Debug.Log($"Ending half day: {currentTime} of day {currentDay} currentWeekDay = {currentWeekDay}");
        
        // Vérifier si un événement doit se déclencher ou est actif
        if (eventScheduler != null)
        {
            Debug.Log("Vérification des événements...");
            bool eventActive = eventScheduler.CheckAndTriggerEvent((int)currentDay, currentTime);
            if (eventActive)
            {
                // Un événement est actif, on ne lance pas ChooseGameMode
                Debug.Log("Événement actif, gameplay normal en pause.");
                return;
            }
        }
        
        // Pas d'événement ou événement terminé, on continue normalement
        ChooseGameMode();
    }


    public void EndDay()
    {
        // Actions à effectuer à la fin de la journée
        currentWeekDay = weekDays[((int)currentDay - 1) % 7];
        // arrondi à l'entier le plus proche
        float foodLoss = Mathf.Round(Valeurs[StatType.Human] * 0.1f);
        if (Valeurs[StatType.Food] >= foodLoss)
        {
            changeStat(StatType.Food, -foodLoss);
        }
        else
        {
            changeStat(StatType.Human, -10); // perte de 10 humains si pas assez de nourriture
            changeStat(StatType.Food, -Valeurs[StatType.Food]); // met la nourriture à 0
        }
        Debug.Log($"Fin de journée : Perte de nourriture de {foodLoss}, Nourriture restante : {Valeurs[StatType.Food]}");
        Valeurs[StatType.Food] -= foodLoss;
        GameEvents.TriggerDayEnd();
<<<<<<< Updated upstream
=======

        if (currentDay >= totalDays && currentTime == DayTime.Aprem)
        {
            EndCampaign();
        }
    }
    #endregion

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
    #region Sauvegarde / Chargement
    // --- SAUVEGARDE / CHARGEMENT ---
    [System.Serializable]
    private class SaveData
    {
        public int currentDay;
        public int currentTime;      // cast de l'enum DayTime
        public string currentWeekDay;

        public Dictionary<StatType, float> stats;
>>>>>>> Stashed changes
    }
    


<<<<<<< Updated upstream
}
=======
    public void SaveGame()
    {
        var data = new SaveData
        {
            currentDay = currentDay,
            currentTime = (int)currentTime,
            currentWeekDay = currentWeekDay,
            stats = new Dictionary<StatType, float>(Valeurs)
        };

        var wrapper = new SaveWrapper
        {
            currentDay = data.currentDay,
            currentTime = data.currentTime,
            currentWeekDay = data.currentWeekDay,
            statKeys = new List<string>(),
            statValues = new List<float>()
        };

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

        Valeurs.Clear();
        Multiplicateur.Clear();
        MaxValeurs.Clear();

        for (int i = 0; i < wrapper.statKeys.Count; i++)
        {
            if (System.Enum.TryParse(wrapper.statKeys[i], out StatType stat))
            {
                float value = wrapper.statValues[i];
                Valeurs[stat] = value;
                if (!Multiplicateur.ContainsKey(stat))
                    Multiplicateur[stat] = 1f;
                if (!MaxValeurs.ContainsKey(stat))
                    MaxValeurs[stat] = 100f;

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
    #endregion
}
>>>>>>> Stashed changes
