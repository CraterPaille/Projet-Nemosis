using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManagerTri : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject endScreen;
    public TMP_Text endScoreText;
    public TMP_Text endMessageText;

    [Header("Formatting")]
    public string scorePrefix = "pièces récupéré : ";
    public string timerPrefix = "Temps : ";

    // Appelé par TriGameManager pour mettre à jour le score
    public void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = scorePrefix + newScore;
    }

    // Appelé à chaque frame pour mettre à jour le timer
    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            time = Mathf.Max(0, time);
            int seconds = Mathf.CeilToInt(time);
            timerText.text = timerPrefix + seconds + "s";
        }
    }


    // Appelé à la fin de la partie
    public void ShowEndScreen(int finalScore)
    {
        if (endScreen != null)
            endScreen.SetActive(true);

        if (endScoreText != null)
            endScoreText.text = "Score final : " + finalScore;

        if (endMessageText != null)
        {
            if (finalScore >= 100)
                endMessageText.text = "Les dieux te sourient";
            else if (finalScore >= 50)
                endMessageText.text = "Ton tri n’est pas parfait, mais respectable.";
            else
                endMessageText.text = "Hadès est déçu...";
        }
    }

    // Pour cacher l’écran de fin (utile pour relancer une partie)
    public void HideEndScreen()
    {
        if (endScreen != null)
            endScreen.SetActive(false);
    }
}
