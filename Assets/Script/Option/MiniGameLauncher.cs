using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLauncher : MonoBehaviour
{
    [Header("Scènes de mini-jeux disponibles pour le dimanche")]
    [Tooltip("Noms des scènes de mini-jeux enregistrées dans Build Settings")]
    public string[] sundayMiniGameScenes = 
    {
        "ChyronTraverse",
        "RhythmScene",
        "NuitGlaciale",
        "Zeus_gameScene",
        "Tri",
        "JUMP"
    };

    // Méthode pour lancer le mini-jeu Chyron
    public void LaunchChyronGame()
    {
        DisableUIAndLoadScene("ChyronTraverse");
    }

    // Méthode pour lancer le mini-jeu Rhythm
    public void LaunchRhythmGame()
    {
        DisableUIAndLoadScene("RhythmScene");
    }

    public void LaunchNuitGlacialeGame()
    {
        DisableUIAndLoadScene("NuitGlaciale");
    }   

    public void LaunchZeusGame()
    {
        DisableUIAndLoadScene("Zeus_gameScene");
    }

    public void LaunchTriGame()
    {
        DisableUIAndLoadScene("Tri");
    }
    public void LaunchJUMPGame()
    {
        DisableUIAndLoadScene("JUMP");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu_principal");
    }

    /// <summary>
    /// Lance un mini-jeu aléatoire parmi la liste sundayMiniGameScenes.
    /// Si on est le dernier dimanche matin (dernier jour de la campagne),
    /// lance la scène du boss final "boss-chronos".
    /// </summary>
    public void LaunchRandomSundayMiniGame()
    {
        // Vérification du cas Boss final : dernier dimanche matin
        if (GameManager.Instance != null)
        {
            bool isSunday = GameManager.Instance.currentWeekDay == "Dimanche";
            bool isMorning = GameManager.Instance.currentTime == DayTime.Matin;
            bool isLastDay = GameManager.Instance.currentDay >= GameManager.Instance.totalDays;

            if (isSunday && isMorning && isLastDay)
            {
                Debug.Log("[MiniGameLauncher] Dernier dimanche matin détecté -> lancement du Boss final 'boss-chronos'.");
                DisableUIAndLoadScene("Boss_Final_Chronos");
                return;
            }
        }

        if (sundayMiniGameScenes == null || sundayMiniGameScenes.Length == 0)
        {
            Debug.LogWarning("[MiniGameLauncher] Aucune scène de mini-jeu configurée pour le dimanche.");
            return;
        }

        int index = Random.Range(0, sundayMiniGameScenes.Length);
        string sceneName = sundayMiniGameScenes[index];

        Debug.Log($"[MiniGameLauncher] Lancement aléatoire du mini-jeu : {sceneName}");
        DisableUIAndLoadScene(sceneName);
    }

    private void DisableUIAndLoadScene(string sceneName)
    {
        if (UIManager.Instance != null)
        {
            // Marquer qu'on lance un mini-jeu (pour avancer le temps au retour)
            UIManager.Instance.MarkMiniGameLaunch();
            UIManager.Instance.SetUIActive(false);
        }

        SceneManager.LoadScene(sceneName);
    }
}
