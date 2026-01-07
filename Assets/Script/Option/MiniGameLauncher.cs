using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLauncher : MonoBehaviour
{
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
