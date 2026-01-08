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

    [Header("Config")]
    public string miniGameSceneName = "RhythmScene"; // à définir dans l’inspector

    private void Start()
    {
        if (cardData != null)
        {
            if (iconImage != null) iconImage.sprite = cardData.icon;
            if (nameText != null) nameText.text = cardData.cardName;
            if (descriptionText != null) descriptionText.text = cardData.description;
        }

        // Bouton sur le même GameObject
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // 1) Sauvegarder la carte choisie
        if (MiniGameCardRuntime.Instance != null)
            MiniGameCardRuntime.Instance.SelectCard(cardData);

        // 2) Fermer le panel mini-jeu et revenir au ModeChoiceUI
        if (UIManager.Instance != null)
            UIManager.Instance.CloseMiniJeuCardPanelAndBackToModeChoice();
    }
}