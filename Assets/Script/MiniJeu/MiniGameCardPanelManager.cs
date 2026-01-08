using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class MiniGameCardPanelManager : MonoBehaviour
{
    [Header("Source de cartes")]
    [SerializeField] private MiniGameCardCollectionSO cardCollection;
    public MiniGameCardCollectionSO CardCollection => cardCollection;

    [Header("Boutons de cartes dans le panel")]
    [SerializeField] private MiniGameCardButton[] cardButtons;

    [Header("Config")]
    [SerializeField] private int cardsToDraw = 3;

    private void OnEnable()
    {
        RandomizeCards();
    }

    public void RandomizeCards()
    {
        if (cardCollection == null || cardCollection.allMiniGameCards == null || cardCollection.allMiniGameCards.Count == 0)
        {
            Debug.LogWarning("[MiniGameCardPanelManager] Pas de collection ou de cartes configurées.");
            return;
        }

        if (cardButtons == null || cardButtons.Length == 0)
        {
            Debug.LogWarning("[MiniGameCardPanelManager] Aucun MiniGameCardButton assigné.");
            return;
        }

        int drawCount = Mathf.Min(cardsToDraw, cardButtons.Length, cardCollection.allMiniGameCards.Count);

        List<MiniGameCardEffectSO> pool = new List<MiniGameCardEffectSO>(cardCollection.allMiniGameCards);
        List<MiniGameCardEffectSO> currentChoices = new List<MiniGameCardEffectSO>();

        for (int i = 0; i < drawCount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            var card = pool[index];
            pool.RemoveAt(index);
            currentChoices.Add(card);
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                cardButtons[i].SetCard(currentChoices[i]);
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        UIManager.Instance.GameModeChoice();
        GameManager.Instance.EndHalfDay();
    }
}