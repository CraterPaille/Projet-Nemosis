using UnityEngine;

public class UIManagerNuit : MonoBehaviour
{
    public static UIManagerNuit Instance;

    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void ShowWin()
    {
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        losePanel.SetActive(true);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("NuitGlaciale");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
