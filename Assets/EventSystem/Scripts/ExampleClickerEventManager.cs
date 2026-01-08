using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EXEMPLE de Manager pour un mini-jeu simple : Clicker Game
/// Créez vos propres managers en héritant de BaseEventManager
/// </summary>
public class ExampleClickerEventManager : BaseEventManager
{
    [Header("UI du mini-jeu")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Button clickButton;
    public Button finishButton;

    [Header("Paramètres")]
    public float gameDuration = 30f;

    private int clicks = 0;
    private float timeRemaining;

    protected override void OnEventStart()
    {
        base.OnEventStart();
        timeRemaining = gameDuration;
        clicks = 0;
        UpdateUI();

        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnClick);
        }

        if (finishButton != null)
        {
            finishButton.onClick.AddListener(CompleteEvent);
            finishButton.gameObject.SetActive(true); // Caché jusqu'à la fin
        }
    }

    void Update()
    {
        timeRemaining -= Time.deltaTime;
        
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            OnTimeUp();
        }

        UpdateUI();
    }

    void OnClick()
    {
        if (timeRemaining > 0)
        {
            clicks++;
            UpdateUI();
        }
    }

    void OnTimeUp()
    {
        if (clickButton != null) clickButton.interactable = false;
        if (finishButton != null) finishButton.gameObject.SetActive(true);
        Debug.Log($"Temps écoulé! Total de clics : {clicks}");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Clics : {clicks}";
        }

        if (timerText != null)
        {
            timerText.text = $"Temps : {Mathf.CeilToInt(timeRemaining)}s";
        }
    }

    protected override int CalculateScore()
    {
        // Score = nombre de clics (vous pouvez avoir une formule plus complexe)
        // Normaliser sur 100 pour faciliter les seuils
        int normalizedScore = Mathf.Clamp(clicks * 2, 0, 100);
        return normalizedScore;
    }


}
