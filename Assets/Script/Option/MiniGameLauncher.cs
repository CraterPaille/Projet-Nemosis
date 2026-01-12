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
        "Tri"
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

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu_principal");
    }

    /// <summary>
    /// Lance un mini-jeu aléatoire parmi la liste sundayMiniGameScenes.
    /// </summary>
    public void LaunchRandomSundayMiniGame()
    {
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
