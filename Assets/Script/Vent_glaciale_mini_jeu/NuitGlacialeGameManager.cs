using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NuitGlacialeGameManager : MonoBehaviour
{
    public static NuitGlacialeGameManager Instance;

    [Header("Références")]
    public Transform housesParent;       
    public TextMeshProUGUI timerText;    

    [Header("Paramètres de jeu")]
    public float duration = 60f;         
    public float interval = 3f;          
    public float intervalDecrease = 0.9f;// Accélération progressive

    [Header("Génération de maisons")]
    public GameObject housePrefab;
    public int minHouses = 3;
    public int maxHouses = 7;

    public enum WeatherPhase { Normal, Blizzard, Calm }
    public WeatherPhase currentPhase = WeatherPhase.Normal;

    private House[] houses;
    private float timeLeft;
    public bool isRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateRandomHouses();
        houses = housesParent.GetComponentsInChildren<House>();
        StartMiniGame();
    }

    public void StartMiniGame()
    {
        timeLeft = duration;
        isRunning = true;

        foreach (var h in houses)
            h.SetState(true);

        StartCoroutine(HouseFailures());
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            Win();
            return;
        }

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        int offCount = 0;
        foreach (var h in houses)
            if (!h.isOn) offCount++;

        // Vérification de la défaite arrondi à l'entier supérieur (ceil)
        int maxAllowedOff = Mathf.CeilToInt(houses.Length / 2f);
        if (offCount >= maxAllowedOff)
            Lose();

    }

    IEnumerator HouseFailures()
    {
        float currentInterval = interval;

        while (isRunning)
        {
            float wait = Random.Range(currentInterval * 0.5f, currentInterval * 1.5f);
            yield return new WaitForSeconds(wait);

            // combien de maisons sont allumées
            int onCount = 0;
            foreach (var h in houses)
                if (h.isOn) onCount++;

            //le max de maisons éteignable est la moitié inférieur(floorToInt) des maisons allumées
            int maxExtinguishable = Mathf.FloorToInt(onCount / 2f);
            if (maxExtinguishable < 1)
                maxExtinguishable = 1; // au moins 1 sinon ça ne sert à rien

            // Nombre aléatoire à éteindre inférieur ou égal au max
            int housesToExtinguish = Random.Range(1, maxExtinguishable + 1);

            // Éteindre les maisons une par une
            for (int i = 0; i < housesToExtinguish; i++)
            {
                var house = GetRandomOnHouse();
                if (house != null)
                    house.SetState(false);
            }

            // accélération progressive
            currentInterval *= intervalDecrease;
            currentInterval = Mathf.Max(0.5f, currentInterval); // ne pas descendre trop bas
        }
    }

    IEnumerator SpawnAnimation(GameObject obj)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        obj.transform.localScale = initialScale;

        while (elapsed < duration)
        {
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    House GetRandomOnHouse()
    {
        var onHouses = new List<House>();
        foreach (var h in houses)
            if (h.isOn) onHouses.Add(h);

        if (onHouses.Count == 0)
            return houses[Random.Range(0, houses.Length)];

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

            // Démarrer l’animation de spawn
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

        if (GameManager.Instance != null)
        {
            // Exemple : temps restant comme "score" (plus il reste de temps, plus tu gagnes)
            int finalScore = Mathf.CeilToInt(timeLeft);

            float foodGain  = finalScore / 2f;  // à ajuster
            float humanGain = finalScore / 5f;  // à ajuster

            if (foodGain != 0f)
                GameManager.Instance.changeStat(StatType.Food, foodGain);
            if (humanGain != 0f)
                GameManager.Instance.changeStat(StatType.Human, humanGain);

            Debug.Log($"[NuitGlaciale] Score={finalScore} -> Food +{foodGain}, Human +{humanGain}");
        }
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
