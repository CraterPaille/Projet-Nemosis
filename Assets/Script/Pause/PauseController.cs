using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    [Header("Référence UI Pause")]
    [SerializeField] private PauseManager pauseManager; // assigne ton PauseManager dans l’Inspector

    private void Awake()
    {
        if (pauseManager == null)
            pauseManager = PauseManager.Instance;
    }

    private void Update()
    {
        // Centralise l’input de pause ici (Escape / Start)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();

        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsGamePaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsGamePaused) return;

        IsGamePaused = true;
        Time.timeScale = 0f;
        UIManager.Instance.HideAllUI();
        if (pauseManager != null)
            pauseManager.PauseGame();
        else
            Debug.LogWarning("[PauseController] Pas de PauseManager assigné, canvas pause non activé.");

    }

    public void Resume()
    {
        if (!IsGamePaused) return;

        IsGamePaused = false;
        Time.timeScale = 1f;

        if (pauseManager != null)
            pauseManager.ResumeGame();
    }
}