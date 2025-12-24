using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[CreateAssetMenu(menuName = "Effects/ModifyPercentage")]
public class ModifyPercentageEffectSO : EffectSO
{
    public StatType statType;
    public float multiplier; // exemple : 0.1f pour +10% (ou 0.1 = +10% additionnel)
    // conditions[] hérité de EffectSO

    public override Effect CreateInstance()
    {
        return new Effect_ModifyPourcentageGain(this);
    }
}


public class Effect_ModifyPourcentageGain : Effect
{
    private ModifyPercentageEffectSO donnee; 
    public Effect_ModifyPourcentageGain(ModifyPercentageEffectSO soData) : base(soData)
    {
        donnee = soData;
        // NE PAS appeler PassiveManager.Instance.AddEffect(this) ici.
        // L'ajout se fera explicitement après CreateInstance() par le code "joue la carte".
        CheckConditions();
        Debug.Log($"Effet de modification de pourcentage créé pour {donnee.statType} avec un multiplicateur actuelle {GameManager.Instance.Multiplicateur[donnee.statType]} multi actif : {IsActive}");
    }
    
    public override void Activate()
    {   
        if (IsActive) return;
        IsActive = true;
        Debug.Log($"Activation de l'effet de modification de pourcentage pour {donnee.statType}. Multiplicateur avant : {GameManager.Instance.Multiplicateur[donnee.statType]}");
        GameManager.Instance.Multiplicateur[donnee.statType] += donnee.multiplier;
        Debug.Log($"Effet activé. Nouveau multiplicateur pour {donnee.statType} : {GameManager.Instance.Multiplicateur[donnee.statType]}");
        if (IsInstant)
        {
            DestroySelf();
        }
    }



    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;

        GameManager.Instance.Multiplicateur[donnee.statType] -= donnee.multiplier;
    }

}
