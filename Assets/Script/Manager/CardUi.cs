using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class CardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button actionButton;

    private VillageCard currentCard;

    public static CardUI Instance;

    public void Setup(VillageCard Card)
    {
        currentCard = Card;

        iconImage.sprite = Card.icon;
        nameText.text = Card.cardName;
        descriptionText.text = Card.description;

    }

    public void OnActionButtonClicked()
    {
        if (currentCard != null)
        {
            currentCard.PlayCard();
            UIManager.Instance.closeInteractionMenu();
            GameManager.Instance.EndHalfDay();
        }
    }
    
}
