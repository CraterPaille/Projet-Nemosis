using System.Collections.Generic;
using UnityEngine;
public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Instance;
    private List<Effect> activeEffects = new();

    void Awake() => Instance = this;

    public void AddEffect(Effect effect)
    {
        activeEffects.Add(effect);
    }

    public void RemoveEffect(Effect effect)
    {
        activeEffects.Remove(effect);
    }

}
