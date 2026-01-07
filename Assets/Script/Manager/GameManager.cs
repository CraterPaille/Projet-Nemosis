using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // <-- nécessaire pour charger les scènes

public enum DayTime { Matin, Aprem }

// Représente un mini-jeu dans l'inspector
[System.Serializable]
public class MiniGameInfo
{
    public string displayName; // pour l'UI
    public string sceneName;   // nom exact de la scène du mini-jeu
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float statLossMultiplier = 1.0f;

    [Tooltip("Multiplicateur appliqué aux stats")]
    public Dictionary<StatType, float> Multiplicateur = new Dictionary<StatType, float>();
    public Dictionary<StatType, float> Valeurs = new Dictionary<StatType, float>();

    [Tooltip("Nombre de cartes à piocher par set")]
    public int cardsToDraw = 3; // separer en set apres les tests ?

    // Effet pour le jeu global
    [Header("Jour actuel")]
    public int currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation, MiniJeu }
    public GameMode currentGameMode;

    //public EventScheduler eventScheduler;

    [Header("Cartes disponibles (Set Village)")]
    public VillageCardCollectionSO villageCardCollection;

    private readonly string[] weekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
    public EffectSO effet;

    // ---------- MINI-JEUX ----------
    [Header("Mini-jeux disponibles (Dimanche matin uniquement)")]
    public List<MiniGameInfo> availableMiniGames = new List<MiniGameInfo>(); // AJOUT

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
        // initialise le dictionnaire : chaque clé est un élément de StatType, valeur initiale 
        foreach (StatType stat in StatType.GetValues(typeof(StatType)))
        {
            Multiplicateur[stat] = 1f;
            Debug.Log($"[GameManager] Initializing stat {stat} with multiplier {Multiplicateur[stat]}");
            // initial value
            Valeurs[stat] = 50f;
            changeStat(stat, 0f);
        }
        changeStat(StatType.Nemosis, -50f);

        // On initialise le nom du jour correctement
        currentWeekDay = weekDays[(currentDay - 1) % 7];

        ChooseGameMode();
    }

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
    public void ChooseGameMode()
    {
        // Dimanche matin uniquement : mini-jeu automatique
        if (currentWeekDay == "Dimanche" && currentTime == DayTime.Matin)
        {
            LaunchSundayMorningMiniGame();
            return;
        }

        // Tous les autres moments (y compris dimanche après-midi) : choix normal
        UIManager.Instance.GameModeChoice();
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
        // Passage à la demi-journée suivante
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

        // On redemande le "mode de jeu" adapté (y compris la logique Dimanche/mini-jeu)
        ChooseGameMode();
        //eventScheduler.CheckAndTriggerEvents(currentDay, currentTime);
    }

    public void EndDay()
    {
        // Actions à effectuer à la fin de la journée
        currentWeekDay = weekDays[(currentDay - 1) % 7];
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
    }

    // ---------- LOGIQUE MINI-JEUX ----------

    void LaunchSundayMorningMiniGame()
    {
        if (availableMiniGames == null || availableMiniGames.Count == 0)
        {
            Debug.LogWarning("[GameManager] Aucun mini-jeu configuré (availableMiniGames vide).");
            UIManager.Instance.GameModeChoice(); // fallback
            return;
        }

        // Choix aléatoire
        int index = Random.Range(0, availableMiniGames.Count);
        MiniGameInfo selected = availableMiniGames[index];

        Debug.Log($"[GameManager] Dimanche matin -> lancement auto du mini-jeu {selected.displayName} (scene {selected.sceneName})");

        LaunchMiniGame(selected);
    }

    public void LaunchMiniGame(MiniGameInfo info)
    {
        if (string.IsNullOrEmpty(info.sceneName))
        {
            Debug.LogError("[GameManager] MiniGameInfo.sceneName est vide.");
            return;
        }

        currentGameMode = GameMode.MiniJeu;
        Debug.Log($"[GameManager] Chargement de la scène de mini-jeu : {info.sceneName}");
        SceneManager.LoadScene(info.sceneName);
    }

    // Appelé par les mini-jeux quand ils veulent revenir au hub
    public void ReturnToMainScene()
    {
        currentGameMode = GameMode.village; // ou ce que tu veux
        SceneManager.LoadScene("SampleScene");

        // Attendre que la scène soit chargée, puis passer à la demi-journée suivante
        SceneManager.sceneLoaded += OnReturnedToMainScene;
    }

    private void OnReturnedToMainScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            SceneManager.sceneLoaded -= OnReturnedToMainScene;
            EndHalfDay(); // Passe à la demi-journée suivante (dimanche aprem -> lundi matin, etc.)
        }
    }

    public void SkipToNextSundayMorning()
    {
        // Cherche le prochain dimanche (en conservant le fait que le jeu est en cours)
        while (currentWeekDay != "Dimanche")
        {
            currentDay++;
            currentWeekDay = weekDays[(currentDay - 1) % 7];
        }

        currentTime = DayTime.Matin;

        Debug.Log($"[GameManager] Debug: passage forcé au dimanche matin - Jour {currentDay}, {currentWeekDay} {currentTime}");

        // Met à jour l'UI de date
        GameEvents.TriggerDayEnd();   // ou un event plus précis si tu préfères
        GameEvents.TriggerMorningEnd();

        // Laisse la logique existante gérer le dimanche matin (mini-jeu auto)
        ChooseGameMode();
    }
}

