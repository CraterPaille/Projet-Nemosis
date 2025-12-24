using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class InteractionEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button actionButton;

    private EffectSO currentEffect;

    public static InteractionEntryUI Instance;

    public void Setup(EffectSO effect)
    {
        currentEffect = effect;

        //iconImage.sprite = effect.icon;
        nameText.text = effect.effectName;
        descriptionText.text = effect.description;

    }

    public void OnActionButtonClicked()
    {
        if (currentEffect != null)
        {
            currentEffect.CreateInstance();
            Debug.Log($"Effect {currentEffect.effectName} applied.");
            UIManager.Instance.closeInteractionMenu();
        }
    }
    
}
