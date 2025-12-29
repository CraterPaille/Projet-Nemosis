using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    public GameObject pauseMenuRoot;      // Panel racine du menu pause
    public GameObject firstSelected;      // Bouton sélectionné par défaut (Continue)

    [Header("Scenes")]
    public string mainMenuSceneName = "Menu_principal";
    public string optionsSceneName = "Menu_option";

    private bool _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Optionnel : si tu veux un PauseManager différent par mini-jeu, enlève cette ligne
        DontDestroyOnLoad(gameObject);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    private void Update()
    {
        // Touche d'exemple : Escape ou Start manette via InputSystem
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (_isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);

            if (EventSystem.current != null && firstSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }
        }
    }

    public void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    // Bouton "Continuer"
    public void OnContinueButton()
    {
        ResumeGame();
    }

    // Bouton "Options"
    public void OnOptionsButton()
    {
        // On remet le temps à 1 avant de changer de scène
        Time.timeScale = 1f;
        _isPaused = false;

        if (!string.IsNullOrEmpty(optionsSceneName))
        {
            SceneManager.LoadScene(optionsSceneName);
        }
        else
        {
            Debug.LogWarning("PauseManager : optionsSceneName non défini.");
        }
    }

    // Bouton "Quitter"
    public void OnQuitToMainMenuButton()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("PauseManager : mainMenuSceneName non défini.");
        }
    }
}