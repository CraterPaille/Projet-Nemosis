using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ResourceEntry
{
    public StatType statType;
    public int amount;
}

[CreateAssetMenu(menuName = "Effects/AddRessource")]
public class AddRessourceEffectSO : EffectSO
{
    // Remplacer le Dictionary par une liste sérialisable visible dans l'inspector
    public List<ResourceEntry> RessourceToAdd = new List<ResourceEntry>();

    public override Effect CreateInstance()
    {
        return new Effect_AddRessource(this);
    }
}
public class Effect_AddRessource : Effect
{
    private AddRessourceEffectSO soData;

    public Effect_AddRessource(AddRessourceEffectSO soData) : base(soData)
    {
        this.soData = soData;

        PassiveManager.Instance.AddEffect(this);
        if (conditions.Count == 0) Activate();
    }
    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;

        // Parcourt la liste d'entrées sérialisées
        Debug.Log("[Effect_AddRessource] Activation de l'effet : ajout de ressources.");
        foreach (var entry in soData.RessourceToAdd)
        {
            StatType stat = entry.statType;
            float amount = entry.amount;
            GameManager.Instance.changeStat(stat, amount);
            Debug.Log($"Add resource {stat}: {amount}");
        }
        Deactivate();
    }
    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        return;
    }

}


