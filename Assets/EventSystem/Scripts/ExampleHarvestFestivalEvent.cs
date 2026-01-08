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
    public EffectSO bronzeEffect;
    public EffectSO silverEffect;
    public EffectSO goldEffect;

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
        EffectSO foodReward = null;

        if (score >= goldThreshold)
        {
            foodReward = goldEffect;
            Debug.Log("🥇 Médaille d'OR!");
        }
        else if (score >= silverThreshold)
        {
            foodReward = silverEffect;
            Debug.Log("🥈 Médaille d'ARGENT!");
        }
        else if (score >= bronzeThreshold)
        {
            foodReward = bronzeEffect;
            Debug.Log("🥉 Médaille de BRONZE!");
        }
        else
        {
            Debug.Log("Pas de récompense cette fois...");
        }

        // Appliquer les récompenses au GameManager
        if (GameManager.Instance != null && foodReward != null)
        {
            foodReward.CreateInstance();
            Debug.Log($"✅ +{foodReward} nourriture ajoutée!");
        }
    }
}