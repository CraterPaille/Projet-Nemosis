using System.Collections.Generic;
using TMPro; // Ajoutez ceci en haut du fichier
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

    [Header("UI")]
    [SerializeField] private TMP_Text rerollText; // Ajoutez ce champ

    // Cartes déjà utilisées (pour la journée / session en cours)
    private readonly HashSet<MiniGameCardEffectSO> _usedCards = new HashSet<MiniGameCardEffectSO>();

    private void OnEnable()
    {
        RandomizeCards();
        UpdateRerollText(); // Ajoutez cet appel pour l'init
    }

    public void RandomizeCards()
    {
        if (cardCollection == null || cardButtons == null || cardButtons.Length == 0)
        {
            Debug.LogWarning("[MiniGameCardPanelManager] cardCollection ou cardButtons non assignés.");
            return;
        }

        // Pool de cartes disponibles = toutes les cartes - celles déjà utilisées
        List<MiniGameCardEffectSO> pool = new List<MiniGameCardEffectSO>();
        foreach (var c in cardCollection.allMiniGameCards)
        {
            if (c != null && !_usedCards.Contains(c))
                pool.Add(c);
        }

        // Si plus aucune carte dispo, tu peux soit vider, soit autoriser de nouveau toutes les cartes
        if (pool.Count == 0)
        {
            Debug.Log("[MiniGameCardPanelManager] Plus de cartes disponibles, réinitialisation du pool.");
            _usedCards.Clear();
            pool.AddRange(cardCollection.allMiniGameCards);
        }

        // Tirage aléatoire
        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < cardsToDraw && pool.Count > 0)
            {
                int index = Random.Range(0, pool.Count);
                var card = pool[index];
                pool.RemoveAt(index);

                cardButtons[i].gameObject.SetActive(true);
                cardButtons[i].SetCard(card, this);
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void MarkCardUsed(MiniGameCardEffectSO card)
    {
        if (card != null)
            _usedCards.Add(card);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        UIManager.Instance.GameModeChoice();
        GameManager.Instance.EndHalfDay();  
    }

    public void RerollMiniGameCards()
    {
        // Optionnel : limite de rerolls par jour
        if (GameManager.Instance.RerollsRemaining <= 0)
        {
            Debug.LogWarning("[MiniGameCardPanelManager] Pas de rerolls restants !");
            return;
        }
        GameManager.Instance.RerollsRemaining--;
        RandomizeCards();
        UpdateRerollText(); // Ajoutez cet appel
    }

    private void UpdateRerollText()
    {
        if (rerollText != null)
        {
            rerollText.SetText($"Rerolls : {GameManager.Instance.RerollsRemaining}");
        }
    }
}