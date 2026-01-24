using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class NuitGlacialeGameManager : MonoBehaviour
{
    public static NuitGlacialeGameManager Instance;

    [Header("Références")]
    public Transform housesParent;
    public TextMeshProUGUI timerText;

    [Header("Paramètres de jeu")]
    public float duration = 60f;
    public float interval = 3f;
    public float intervalDecrease = 0.9f; // Accélération progressive

    [Header("Génération de maisons")]
    public GameObject housePrefab;
    public int minHouses = 3;
    public int maxHouses = 7;

    public enum WeatherPhase { Normal, Blizzard, Calm }
    public WeatherPhase currentPhase = WeatherPhase.Normal;

    private House[] houses;
    private float timeLeft;
    public bool isRunning = false;

    private float _baseDuration;
    private float _baseIntervalDecrease;

    // --- paramètres carte ---
    private bool _oneMistakeFail = false;

    [Header("Tutoriel")]
    public MiniGameTutorialPanel tutorialPanel; // à assigner dans l'inspector
    public VideoClip tutorialClip; // à assigner dans l'inspector
    private bool tutorialValidated = false; // Ajouté
    public bool StartPlaying;


    [Header("Paliers étoiles")]
    public int[] starThresholds = new int[3] { 10, 20, 30 };
    private bool[] starGiven = new bool[3];
    [Header("UI Étoiles")]
    public UnityEngine.UI.Image[] starImages;
    public Sprite starOnSprite;
    public Sprite starOffSprite;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        starGiven = new bool[3];
        UpdateStarsUI();
    }

    void Start()
    {
        ShowTutorialAndStart();

        _baseDuration = duration;
        _baseIntervalDecrease = intervalDecrease;
        ApplyMiniGameCardIfAny();
    }

    private void ApplyMiniGameCardIfAny()
    {
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.NuitGlaciale)
            return;

        float diffMult = Mathf.Max(0.5f, card.difficultyMultiplier);

        duration = _baseDuration / diffMult;         // plus dur => moins de temps
        intervalDecrease = Mathf.Lerp(1f, _baseIntervalDecrease, diffMult); // accélération plus forte

        // --- nouveau : une erreur = défaite ---
        _oneMistakeFail = card.oneMistakeFail;

        Debug.Log($"[NuitGlaciale] Carte appliquée : {card.cardName}, duration={duration}, intervalDecrease={intervalDecrease}, oneMistakeFail={_oneMistakeFail}");

        runtime.Clear();
    }

    public void StartMiniGame()
    {
        // Sécurité : vérifier les références essentielles
        if (housesParent == null)
        {
            Debug.LogError("[NuitGlaciale] StartMiniGame annulé : housesParent n'est pas assigné dans l'Inspector.");
            return;
        }

        if (housePrefab == null)
        {
            Debug.LogWarning("[NuitGlaciale] housePrefab non assigné : génération des maisons impossible. Abandon du démarrage.");
            return;
        }

        // S'assurer que les maisons existent ; si non, générer
        houses = housesParent.GetComponentsInChildren<House>();
        if (houses == null || houses.Length == 0)
        {
            Debug.Log("[NuitGlaciale] Aucune house trouvée, génération automatique avant démarrage.");
            GenerateRandomHouses();
            houses = housesParent.GetComponentsInChildren<House>();
        }

        if (houses == null || houses.Length == 0)
        {
            Debug.LogError("[NuitGlaciale] Aucun House trouvé après tentative de génération. StartMiniGame annulé.");
            return;
        }

        // Si le jeu est démarré manuellement, on considère le tutoriel validé
        tutorialValidated = true;

        timeLeft = duration;
        isRunning = true;

        foreach (var h in houses)
        {
            if (h != null)
                h.SetState(true);
        }

        StartCoroutine(HouseFailures());
    }

    public void ShowTutorialAndStart()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.ShowSimple(
                "Nuit Glaciale",
                tutorialClip,
                " Conseil : Cliquez  ou appuyé sur A ou X (selon la manette) pour rallumer les maison éteinte. " +
                "Si plus de la moitié des maisons s'éteignent vous pedrez. " +
                "De plus plusieurs maisons peuvent s'éteindre en même temps!"
            );

            tutorialPanel.continueButton.onClick.RemoveAllListeners();
            tutorialPanel.continueButton.onClick.AddListener(() =>
            {
                tutorialPanel.Hide();
                tutorialValidated = true;
                GenerateRandomHouses();
                houses = housesParent.GetComponentsInChildren<House>();
                StartMiniGame();
            });
        }
        else
        {
            tutorialValidated = true;
            GenerateRandomHouses();
            StartMiniGame();
        }
    }

    void Update()
    {
        if (!tutorialValidated)
            return;

        if (!isRunning) return;
        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (!starGiven[i] && timeLeft >= starThresholds[i])
            {
                starGiven[i] = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.changeStat(StatType.Foi, 5f);
            }
        }
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            Win();
            return;
        }

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        int offCount = 0;
        if (houses != null)
        {
            foreach (var h in houses)
                if (h != null && !h.isOn) offCount++;
        }

        // Nouvelle logique pour le max de maisons éteintes
        int maxAllowedOff;
        if (houses != null && houses.Length % 2 == 0)
            maxAllowedOff = (houses.Length / 2) - 1;
        else
            maxAllowedOff = Mathf.CeilToInt((houses != null && houses.Length > 0 ? houses.Length : minHouses) / 2f);

        if (offCount >= maxAllowedOff)
            Lose();
        UpdateStarsUI();

    }
    public void UpdateStarsUI()
    {
        for (int i = 0; i < starImages.Length; i++)
            starImages[i].sprite = starGiven[i] ? starOnSprite : starOffSprite;
    }
    IEnumerator HouseFailures()
    {
        float currentInterval = interval;
        float lastExtinguishTime = -999f; // temps de la dernière extinction

        while (isRunning)
        {
            float wait = Random.Range(currentInterval * 0.5f, currentInterval * 1.5f);

            // On attend le plus long entre le wait normal et le temps restant pour atteindre 2s mini
            float timeSinceLast = Time.time - lastExtinguishTime;
            float minWait = Mathf.Max(0f, 2f - timeSinceLast);
            float finalWait = Mathf.Max(wait, minWait);

            yield return new WaitForSeconds(finalWait);

            // combien de maisons sont allumées
            int onCount = 0;
            if (houses != null)
            {
                foreach (var h in houses)
                    if (h != null && h.isOn) onCount++;
            }

            int maxExtinguishable = Mathf.FloorToInt(onCount / 2f);
            if (maxExtinguishable < 1)
                maxExtinguishable = 1;

            int housesToExtinguish = Random.Range(1, maxExtinguishable + 1);

            for (int i = 0; i < housesToExtinguish; i++)
            {
                var house = GetRandomOnHouse();
                if (house != null)
                    house.SetState(false);
            }

            lastExtinguishTime = Time.time; // on note le moment de l’extinction

            currentInterval *= intervalDecrease;
            currentInterval = Mathf.Max(0.5f, currentInterval);
        }
    }

    // --- appelé par House quand elle passe de ON à OFF ---
    public void OnHouseTurnedOff(House house)
    {
        if (!isRunning) return;

        if (_oneMistakeFail)
        {
            Debug.Log("[NuitGlaciale] Mode oneMistakeFail : une maison s’est éteinte -> défaite immédiate.");
            Lose();
        }
    }

    IEnumerator SpawnAnimation(GameObject obj)
    {
        // Protection : si l'objet a déjà été détruit ou non assigné, on sort.
        if (obj == null)
            yield break;

        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        // Sauvegarder la référence Transform (non nécessaire mais évite plusieurs .transform calls)
        Transform t = obj.transform;
        if (t == null)
            yield break;

        // Initialisation sécurisée
        t.localScale = initialScale;

        while (elapsed < duration)
        {
            // L'objet peut être détruit pendant la coroutine ; sortir proprement si c'est le cas.
            if (obj == null)
                yield break;

            // Re-obtenir le Transform au cas où l'objet ait été renommé/reparenté (sécurisé)
            t = obj.transform;
            t.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
        {
            obj.transform.localScale = targetScale;
        }
    }

    House GetRandomOnHouse()
    {
        var onHouses = new List<House>();
        if (houses != null)
        {
            foreach (var h in houses)
                if (h != null && h.isOn) onHouses.Add(h);
        }

        if (onHouses.Count == 0)
        {
            if (houses != null && houses.Length > 0)
                return houses[Random.Range(0, houses.Length)];
            return null;
        }

        return onHouses[Random.Range(0, onHouses.Count)];
    }

    void GenerateRandomHouses()
    {
        float minX, maxX, minY, maxY;
        GetCameraBounds(out minX, out maxX, out minY, out maxY);

        int houseCount = Random.Range(minHouses, maxHouses + 1);

        // Supprimer anciennes maisons
        foreach (Transform child in housesParent)
            Destroy(child.gameObject);

        List<Collider2D> existingColliders = new List<Collider2D>();

        for (int i = 0; i < houseCount; i++)
        {
            GameObject h = Instantiate(housePrefab, housesParent);

            BoxCollider2D bc = h.GetComponent<BoxCollider2D>();
            if (bc == null) bc = h.AddComponent<BoxCollider2D>();

            int tries = 0;
            bool validPos = false;
            Vector3 pos = Vector3.zero;

            while (!validPos && tries < 10)
            {
                float x = Random.Range(minX + 0.5f, maxX - 0.5f);
                float y = Random.Range(minY + 0.5f, maxY - 0.5f);
                pos = new Vector3(x, y, 0f);

                // Vérifier collisions
                validPos = true;
                foreach (var col in existingColliders)
                {
                    if (col.bounds.Intersects(bc.bounds))
                    {
                        validPos = false;
                        break;
                    }
                }

                tries++;
            }

            h.transform.position = pos;

            // Ajouter le collider à la liste pour les futures vérifications
            existingColliders.Add(bc);

            // Activer le script si jamais désactivé
            var houseCmp = h.GetComponent<House>();
            if (houseCmp != null) houseCmp.enabled = true;

            // Démarrer l’animation de spawn (sécurisé : la coroutine vérifie si l'objet est détruit)
            StartCoroutine(SpawnAnimation(h));
        }
    }

    void GetCameraBounds(out float minX, out float maxX, out float minY, out float maxY)
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        minX = cam.transform.position.x - width / 2f;
        maxX = cam.transform.position.x + width / 2f;

        minY = cam.transform.position.y - height / 2f;
        maxY = cam.transform.position.y + height / 2f;
    }

    void Win()
    {
        Debug.Log("Victoire : tu as tenu la nuit !");
        isRunning = false;
        StopAllCoroutines();
        UIManagerNuit.Instance.ShowWin();
        SceneManager.LoadScene("SampleScene");
    }

    void Lose()
    {
        Debug.Log("Défaite : trop de maisons glacées !");
        isRunning = false;
        StopAllCoroutines();
        UIManagerNuit.Instance.ShowLose();
        SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}