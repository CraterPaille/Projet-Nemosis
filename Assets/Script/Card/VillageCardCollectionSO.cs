using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "VillageCardCollection", menuName = "Game/Village Card Collection")]
public class VillageCardCollectionSO : ScriptableObject
{
    public List<VillageCard> allVillageCards;
}
