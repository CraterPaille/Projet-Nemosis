using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EndGameScreen : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public TMP_Text finText;
    public Button menuButton;

    void Start()
    {
        // Apparition douce du fond
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad);

        // Animation du texte "FIN"
        finText.text = "FIN";
        finText.transform.localScale = Vector3.zero;
        finText.transform.DOScale(2f, 1.8f).SetEase(Ease.OutBack);

        // Animation du bouton (pulsation douce)
        menuButton.transform.localScale = Vector3.one;
        menuButton.transform.DOScale(2.1f, 1.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        // Accessibilité : focus clavier/manette sur le bouton
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(menuButton.gameObject);
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu_principal");
    }
}