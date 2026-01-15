using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Conditions/Stat Level Check")]
public class StatLevelCheckConditionSO : ConditionSO
{
    public StatType statType;
    public float requiredLevel;
    public bool plusQue;        // true = vérifier "supérieur à"
    public bool autoDestruct;   // true = supprimer si la condition échoue

    public override Condition CreateInstance(Effect parent)
    {
        return new Condition_StatLevelCheck(this, parent);
    }

    // Standalone evaluation used by DialogueRunner (no Effect parent)
    public override bool EvaluateStandalone()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning($"[Condition Standalone] GameManager not available for {conditionName ?? name}. Defaulting to true.");
            return true;
        }
        if (!GameManager.Instance.Valeurs.ContainsKey(statType))
        {
            Debug.LogWarning($"[Condition Standalone] Stat {statType} not initialized. Defaulting to true.");
            return true;
        }
        float current = GameManager.Instance.Valeurs[statType];
        bool conditionMet = plusQue ? current >= requiredLevel : current < requiredLevel;
        Debug.Log($"[Condition Standalone] {conditionName ?? name}: required {requiredLevel} => met: {conditionMet} (current: {current})");
        return conditionMet;
    }
}
public class Condition_StatLevelCheck : Condition
{
    private StatLevelCheckConditionSO donnee;
    public Condition_StatLevelCheck(StatLevelCheckConditionSO data, Effect parent) : base(data, parent)
    {
        this.donnee = data;
        Subscribe();
        Evaluate(); // Première évaluation directe
    }

    public override void Subscribe()
    {
        GameEvents.OnStatChanged += OnStatChanged;
    }
    public override void Unsubscribe()
    {
        GameEvents.OnStatChanged -= OnStatChanged;
    }

    public void OnStatChanged(StatType type, float amount)
    {
        if (type == donnee.statType)
        {
            Evaluate();
        }
    }
    public override void Evaluate()
    {
        
        float current = GameManager.Instance.Valeurs[donnee.statType];
        bool conditionMet = donnee.plusQue ? current >= donnee.requiredLevel : current < donnee.requiredLevel;
        Debug.Log($"[Condition] Évaluation de la condition pour {donnee.statType} (niveau requis : {(donnee.plusQue ? "≥" : "<")} {donnee.requiredLevel}) = conditionMet : {conditionMet}, niveau actuel : {current}");

        if (donnee.autoDestruct && !conditionMet)
        {
            Unsubscribe();
            linkedEffect.DestroySelf(); // méthode à ajouter dans Effect pour détruire proprement
            Debug.Log("[Condition] Auto-destruct exécuté.");
            return;
        }
        UpdateCondition(conditionMet);
    
    }
}
