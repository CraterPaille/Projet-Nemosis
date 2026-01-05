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

    // Effet pour le jeu global
    [Header("Jour actuel")]
    public int currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation,  }
    public GameMode currentGameMode;


    //public EventScheduler eventScheduler;

    [Header("Cartes disponibles (Set Village)")]
    public VillageCardCollectionSO villageCardCollection;


    private readonly string[] weekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
    public EffectSO effet;

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
        UIManager.Instance.DayModeChoice(true);
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

        // Calcul du jour de la semaine
        
        Debug.Log($"Ending half day: {currentTime} of day {currentDay} currentWeekDay = {currentWeekDay}");
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
    


}
