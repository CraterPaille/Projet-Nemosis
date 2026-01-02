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
    public float currentDay = 1;
    public DayTime currentTime = DayTime.Matin;
    public string currentWeekDay = "Lundi";
    public enum GameMode { village, VillageCard, Relation, }
    public GameMode currentGameMode;

    [Header("Event System")]
    public EventScheduler eventScheduler;

    [Header("Cartes disponibles (Set Village)")]
    public VillageCardCollectionSO villageCardCollection;

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

        Debug.Log($"StatType values: {string.Join(", ", System.Enum.GetNames(typeof(StatType)))}");

        // initialise le dictionnaire : chaque clé est un élément de StatType, valeur initiale 
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            Multiplicateur[stat] = 1f;
            Debug.Log($"[GameManager] Initializing stat {stat} with multiplier {Multiplicateur[stat]}");

            // valeur initiale
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
   
