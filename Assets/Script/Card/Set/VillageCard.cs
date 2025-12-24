using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card/VillageCard")]
public class VillageCard : Card
{
    public EffectSO effectSO;

    public void PlayCard()
    {
        effectSO.CreateInstance(); 
    }

}
