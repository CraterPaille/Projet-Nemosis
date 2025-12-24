using UnityEngine;
public abstract class Condition
{
    protected ConditionSO data;
    protected Effect linkedEffect;
    public bool IsTrue { get; protected set; }
    public Condition(ConditionSO data, Effect linkedEffect)
    {
        this.data = data;
        this.linkedEffect = linkedEffect;
    }


    public abstract void Evaluate();

    public abstract void Subscribe();
    public abstract void Unsubscribe();
    protected void UpdateCondition(bool newValue)
    {
        if (IsTrue == newValue) return;

        IsTrue = newValue;
        linkedEffect.OnConditionChanged();
    }
}

public abstract class ConditionSO : ScriptableObject
{
    public string conditionName;
    public virtual Condition CreateInstance(Effect effect)
    {
        Debug.LogWarning("Base ConditionSO.CreateInstance() called — should be overridden.");
        return null;
    }
    // Evaluate the condition outside of an Effect context (used by DialogueRunner to filter responses)
    // By default returns true (non-restrictive). Override in concrete ConditionSO when you need standalone evaluation.
    public virtual bool EvaluateStandalone()
    {
        Debug.LogWarning($"ConditionSO.EvaluateStandalone not implemented for {name}. Defaulting to true.");
        return true;
    }
}
