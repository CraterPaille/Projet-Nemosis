using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Effects/DrawwExtraCard")]
public class DrawExtraCardEffectSO : EffectSO
{
    public int extraCards;

    public override Effect CreateInstance()
    {
        return new Effect_DrawExtraCard(this);
    }
}
public class Effect_DrawExtraCard : Effect
{
    private DrawExtraCardEffectSO soData;

    public Effect_DrawExtraCard(DrawExtraCardEffectSO soData) : base(soData)
    {
        this.soData = soData;
        PassiveManager.Instance.AddEffect(this);
        CheckConditions();
    }
    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        GameManager.Instance.cardsToDraw += soData.extraCards;
    }
    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        GameManager.Instance.cardsToDraw -= soData.extraCards;
    }

}


