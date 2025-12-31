using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameLauncher : MonoBehaviour
{
    // Méthode pour lancer le mini-jeu Chyron
    public void LaunchChyronGame()
    {
        SceneManager.LoadScene("ChyronTraverse");
    }

    // Méthode pour lancer le mini-jeu Rhythm
    public void LaunchRhythmGame()
    {
        SceneManager.LoadScene("RhythmScene");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu_principal");
    }   
}
