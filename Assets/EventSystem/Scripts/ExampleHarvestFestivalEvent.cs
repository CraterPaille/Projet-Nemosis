using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EXEMPLE d'événement : Festival de la Moisson
/// Créez des fichiers similaires pour vos propres événements.
/// </summary>
[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/Game Event", order = 1)]
public class ExampleHarvestFestivalEvent : BaseGameEvent
{
    [Header("Récompenses selon le score")]
    [Tooltip("Score minimum pour obtenir les récompenses")]
    public int bronzeThreshold = 50;
    public int silverThreshold = 75;
    public int goldThreshold = 90;

    [Header("Récompenses (exemple)")]
    public int bronzeFood = 20;
    public int silverFood = 50;
    public int goldFood = 100;

    public override void StartEvent()
    {
        // Charger la scène du mini-jeu
        if (!string.IsNullOrEmpty(eventInfo.sceneName))
        {
            Debug.Log($"Chargement de la scène : {eventInfo.sceneName}");
            SceneManager.LoadScene(eventInfo.sceneName);
        }
        else
        {
            Debug.LogError("ExampleHarvestFestivalEvent: sceneName n'est pas défini!");
        }
    }

    public override void ApplyRewards(int score)
    {
        Debug.Log($"Application des récompenses pour score : {score}");

        // Déterminer les récompenses selon le score
        int foodReward = 0;

        if (score >= goldThreshold)
        {
            foodReward = goldFood;
            Debug.Log("🥇 Médaille d'OR!");
        }
        else if (score >= silverThreshold)
        {
            foodReward = silverFood;
            Debug.Log("🥈 Médaille d'ARGENT!");
        }
        else if (score >= bronzeThreshold)
        {
            foodReward = bronzeFood;
            Debug.Log("🥉 Médaille de BRONZE!");
        }
        else
        {
            Debug.Log("Pas de récompense cette fois...");
        }

        // Appliquer les récompenses au GameManager
        if (GameManager.Instance != null && foodReward > 0)
        {
            GameManager.Instance.changeStat(StatType.Food, foodReward);
            Debug.Log($"✅ +{foodReward} nourriture ajoutée!");
        }
    }
}
