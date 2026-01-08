using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameCardCollection", menuName = "Game/MiniGame Card Collection")]
public class MiniGameCardCollectionSO : ScriptableObject
{
    [Tooltip("Toutes les cartes d'effets utilisables pour les mini-jeux")]
    public List<MiniGameCardEffectSO> allMiniGameCards = new List<MiniGameCardEffectSO>();
}