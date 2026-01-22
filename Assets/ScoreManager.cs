using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ScoreManager : MonoBehaviour
{
    [Header("Pendant le Jeu")]
    public Transform player;
    public TextMeshProUGUI scoreText;

    [Header("Étoiles (UI en jeu)")]
    public int[] starThresholds = new int[3] { 50, 100, 200 }; // paliers (hauteur) pour 1/2/3 étoiles
    private bool[] starGiven;
    public Image[] starImages; // assigner les UI images (en jeu) dans l'inspector
    public Sprite starOnSprite;
    public Sprite starOffSprite;

    // Données
    private float maxHeight = 0f;
    private int collectedItems = 0;
    private bool gameEnded = false;

    // quantité de population donnée par étoile (modifiable)
    public float humanPerStar = 1f;

    [Header("Start / UI")]
    public GameObject startScreenUI; // associer un panneau Start dans l'inspector (optionnel)

    void Awake()
    {
        if (starThresholds == null || starThresholds.Length == 0)
            starThresholds = new int[3] { 50, 100, 200 };

        starGiven = new bool[starThresholds.Length];

        // initialiser l'UI des étoiles en jeu si assignée
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null && starOffSprite != null)
                {
                    starImages[i].sprite = starOffSprite;
                    starImages[i].color = new Color(1f, 1f, 1f, 0.45f);
                    starImages[i].transform.localScale = Vector3.one;
                }
            }
        }

        // Pause au chargement : on attend que le joueur appuie pour lancer le jeu
        Time.timeScale = 0f;

        if (startScreenUI != null)
            startScreenUI.SetActive(true);
    }

    void Update()
    {
        // Si le jeu est en pause initiale, démarrer sur entrée bouton/touche
        if (!gameEnded && Time.timeScale == 0f)
        {
            if (IsStartPressed())
            {
                StartGame();
            }
            return; // ne pas exécuter le reste tant que le jeu est en pause
        }

        if (!gameEnded && player != null)
        {
            bool heightChanged = false;
            if (player.position.y > maxHeight)
            {
                maxHeight = player.position.y;
                scoreText.text = "Score: " + Mathf.Round(maxHeight).ToString();
                heightChanged = true;
            }

            // Vérifier paliers d'étoiles si la hauteur a augmenté
            if (heightChanged)
            {
                CheckStarThresholds();
                UpdateStarsUI();
            }

            if (player.position.y >= 200f)
            {
                WinGame();
            }
        }
    }

    // Méthode publique pour être liée à un bouton UI (OnClick)
    public void StartGame()
    {
        Time.timeScale = 1f;
        if (startScreenUI != null)
            startScreenUI.SetActive(false);
    }

    // Détecte si le joueur a pressé la touche/bouton pour démarrer
    private bool IsStartPressed()
    {
        // Nouveau Input System : manette / clavier
        try
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
        }
        catch { /* si Input System non présent, on continue */ }

        // Vieux Input Manager : fallback
        try
        {
            if (Input.GetButtonDown("Submit"))
                return true;
        }
        catch { }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            return true;

        return false;
    }

    public void AddStar()
    {
        collectedItems++;
        // Donner de la population comme les autres mini-jeux
        if (GameManager.Instance != null)
        {
            GameManager.Instance.changeStat(StatType.Human, humanPerStar);
            Debug.Log($"[ScoreManager] Étoile collectée -> Human +{humanPerStar} (total étoiles ramassées: {collectedItems})");
        }
        else
        {
            Debug.LogWarning("[ScoreManager] AddStar appelé mais GameManager introuvable.");
        }

        // Si tu veux que la collecte d'une étoile active aussi une étoile UI/palier,
        // tu peux appeler CheckStarThresholds() ici si les paliers doivent inclure collectedItems.
    }

    // Vérifie les paliers configurés et attribue les étoiles une fois
    private void CheckStarThresholds()
    {
        if (starThresholds == null || starThresholds.Length == 0) return;

        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (i >= starGiven.Length) break;
            if (!starGiven[i] && maxHeight >= starThresholds[i])
            {
                starGiven[i] = true;

                // Récompense: population (comme demandé)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.changeStat(StatType.Human, humanPerStar);
                    Debug.Log($"[ScoreManager] Palier étoile {i + 1} atteint -> Human +{humanPerStar}");
                }

                // Optionnel : petite "pop" visuelle sans DOTween
                if (starImages != null && i < starImages.Length && starImages[i] != null)
                {
                    starImages[i].sprite = starOnSprite != null ? starOnSprite : starImages[i].sprite;
                    starImages[i].color = Color.white;
                    starImages[i].transform.localScale = Vector3.one * 1.2f;
                    // remettre à l'échelle normale progressivement si nécessaire (DOTween ailleurs dans le projet)
                    starImages[i].transform.localScale = Vector3.one;
                }
            }
        }
    }

    // Met à jour l'affichage des étoiles en jeu
    public void UpdateStarsUI()
    {
        if (starImages == null || starGiven == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            bool on = (i < starGiven.Length && starGiven[i]);
            if (starOnSprite != null && starOffSprite != null)
                starImages[i].sprite = on ? starOnSprite : starOffSprite;
            starImages[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            starImages[i].transform.localScale = Vector3.one;
        }
    }

    public void WinGame()
    {
        gameEnded = true;
        if (player.GetComponent<Rigidbody2D>() != null)
            player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        EndGame();
    }



    public void EndGame()
    {
        SceneManager.LoadScene("sampleScene");
    }
}