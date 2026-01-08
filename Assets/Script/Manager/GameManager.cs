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

    // Effet pour le jeu global
    [Header("Jour actuel")]
    public int currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation,  }
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
        // initialise le dictionnaire : chaque clé est un élément de StatType, valeur initiale 
        foreach (StatType stat in StatType.GetValues(typeof(StatType)))
        {
            
            Multiplicateur[stat] = 1f;
            Debug.Log($"[GameManager] Initializing stat {stat} with multiplier {Multiplicateur[stat]}");
            // valeur de départ
            Valeurs[stat] = 50f;
            changeStat(stat, 0f);
        }
        changeStat(StatType.Nemosis, -50f);
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
        if (campaignFinished) return;

        // Exemple : si c’est samedi après-midi, on affiche directement les cartes de mini-jeu
        if (currentWeekDay == "Samedi" && currentTime == DayTime.Aprem)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetUIActive(true);
                UIManager.Instance.ShowMiniJeuCardPanel();
            }
            return;
        }

        // Dimanche matin -> mini-jeu auto
        if (currentWeekDay == "Dimanche" && currentTime == DayTime.Matin)
        {
            LaunchSundayMiniGame();
            return;
        }

        UIManager.Instance.GameModeChoice();
    }

    private void LaunchSundayMiniGame()
    {
        Debug.Log("[GameManager] Dimanche matin : lancement automatique d'un mini-jeu aléatoire !");

        if (miniGameLauncher != null)
        {
            miniGameLauncher.LaunchRandomSundayMiniGame();
        }
        else
        {
            // Fallback si le launcher n'est pas assigné
            if (UIManager.Instance != null)
                UIManager.Instance.SetUIActive(false);
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("RhythmScene");
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

        //verifier si la campagne est finie
        if (campaignFinished) return;

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
            currentWeekDay = weekDays[(currentDay - 1) % 7]; // C'est une responsibiliter que le Schen
            EndDay();
        }

        // Calcul du jour de la semaine
        
        Debug.Log($"Ending half day: {currentTime} of day {currentDay} currentWeekDay = {currentWeekDay}");

        //Fin de la campagne après le dernier jour
        // Fin de campagne après la dernière journée
        if (currentDay > totalDays)
        {
            EndCampaign();
            return;
        }

        // Vérifier si un événement doit se déclencher ou est actif
        if (eventScheduler != null)
        {
            bool eventActive = eventScheduler.CheckAndTriggerEvent(currentDay, currentTime);
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
        if (campaignFinished) return;
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
    /// <summary>
    /// [DEBUG] Avance directement au dimanche matin et lance le mini-jeu.
    /// Peut être appelé depuis un bouton UI.
    /// </summary>
    public void DebugSkipToSunday()
    {
        if (campaignFinished) return;
        // Calculer combien de jours jusqu'au prochain dimanche
        int currentDayIndex = (currentDay - 1) % 7; // 0 = Lundi, 6 = Dimanche
        int daysUntilSunday = (6 - currentDayIndex + 7) % 7;
        
        // Si on est déjà dimanche après-midi, aller au dimanche suivant
        if (daysUntilSunday == 0 && currentTime == DayTime.Aprem)
            daysUntilSunday = 7;

        currentDay += daysUntilSunday;
        currentTime = DayTime.Matin;
        currentWeekDay = "Dimanche";

        Debug.Log($"[DEBUG] Saut au dimanche matin ! Jour {currentDay}");
        
        // Mettre à jour l'UI de la date
        UIManager.Instance?.changeDateUI();
        
        // Lancer le choix de mode (qui détectera dimanche matin)
        ChooseGameMode();
    }
    #endregion
}
