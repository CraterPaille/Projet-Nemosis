using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MiniGameCardButton : MonoBehaviour
{
    public MiniGameCardEffectSO cardData;

    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        // Si une carte est déjà assignée dans l’Inspector, initialiser l’UI
        if (cardData != null)
            RefreshUI();
    }

    public void SetCard(MiniGameCardEffectSO newCard)
    {
        cardData = newCard;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (cardData == null) return;

        if (iconImage != null) iconImage.sprite = cardData.icon;
        if (nameText != null) nameText.text = cardData.cardName;
        if (descriptionText != null) descriptionText.text = cardData.description;
    }

    private void OnClick()
    {
        if (cardData == null) return;

        // 1) Sauvegarder la carte choisie
        if (MiniGameCardRuntime.Instance != null)
            MiniGameCardRuntime.Instance.SelectCard(cardData);

        // 2) Fermer le panel mini-jeu et revenir au ModeChoiceUI
        if (UIManager.Instance != null)
            UIManager.Instance.CloseMiniJeuCardPanelAndBackToModeChoice();
    }
}