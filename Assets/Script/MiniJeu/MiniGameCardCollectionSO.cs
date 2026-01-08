using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameCardCollection", menuName = "Game/MiniGame Card Collection")]
public class MiniGameCardCollectionSO : ScriptableObject
{
    [Tooltip("Toutes les cartes d'effets utilisables pour les mini-jeux")]
    public List<MiniGameCardEffectSO> allMiniGameCards = new List<MiniGameCardEffectSO>();

    public void RemoveCard(MiniGameCardEffectSO card)
    {
        if (card == null) return;

        if (allMiniGameCards.Remove(card))
        {
            Debug.Log($"[MiniGameCardCollection] Carte retirée : {card.cardName}");
        }
        else
        {
            Debug.LogWarning($"[MiniGameCardCollection] Impossible de retirer la carte (pas trouvée dans la collection) : {card.cardName}");
        }
    }
}