using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct EventInfo
{
    public string sceneName;
    public Sprite eventImage;
    public string eventName;
    [TextArea(3, 5)]
    public string description;
    public int durationHalfDays;
}

public abstract class BaseGameEvent : ScriptableObject
{


    public EventInfo eventInfo;

    // Appelé quand l'événement commence (charge la scène, etc.)
    public abstract void StartEvent();

    // Appelé à la fin de l'événement pour appliquer les récompenses selon le score
    public abstract void ApplyRewards(int score);

    public EventInfo GetEventInfo()
    {
        return eventInfo;
    }
}
