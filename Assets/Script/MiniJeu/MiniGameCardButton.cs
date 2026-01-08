
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameCardButton : MonoBehaviour
{
    public MiniGameCardEffectSO cardData;

    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private Button _button;
    private MiniGameCardPanelManager _panelManager;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnClick);

        _panelManager = GetComponentInParent<MiniGameCardPanelManager>();
    }

    private void Start()
    {
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
        Debug.Log($"[MiniGameCardButton] OnClick sur {cardData?.cardName}");

        if (cardData == null) return;

        // 1) Sauvegarder la carte choisie pour le prochain mini-jeu
        if (MiniGameCardRuntime.Instance != null)
            MiniGameCardRuntime.Instance.SelectCard(cardData);

        // 2) Retirer définitivement la carte de la collection (elle ne pourra plus être tirée)
        if (_panelManager != null && _panelManager.CardCollection != null)
        {
            _panelManager.CardCollection.RemoveCard(cardData);
        }

        // 3) Avancer le temps (Matin -> Aprem, Aprem -> jour suivant) via GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndHalfDay();
        }

        // 4) Fermer le panel mini-jeu et revenir au ModeChoiceUI (qui se rouvrira via ChooseGameMode si besoin)
        if (UIManager.Instance != null)
            UIManager.Instance.CloseMiniJeuCardPanelAndBackToModeChoice();
    }
}