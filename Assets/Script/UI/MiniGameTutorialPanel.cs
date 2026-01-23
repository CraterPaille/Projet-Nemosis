using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class MiniGameTutorialPanel : MonoBehaviour
{
    [Header("Main UI")]
    public TMP_Text titleText;
    public Button continueButton;
    public Button backButton; // Optionnel
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Animation Targets")]
    public CanvasGroup backgroundOverlay;
    public RectTransform contentContainer;
    public RectTransform videoSection;
    public RectTransform controlsSection;

    [Header("Device Toggle")]
    public Button keyboardToggleButton;
    public Button gamepadToggleButton;
    public GameObject keyboardControlsPanel;
    public GameObject gamepadControlsPanel;

    [Header("Device Toggle Colors")]
    public Color keyboardActiveColor = new Color(0.58f, 0.20f, 0.92f, 1f);
    public Color gamepadActiveColor = new Color(0.86f, 0.15f, 0.47f, 1f);
    public Color inactiveDeviceColor = new Color(0.22f, 0.25f, 0.32f, 1f);

    [Header("Rebind UI - Clavier")]
    public TMP_Text[] laneLabelsKeyboard; // 0 à 3
    public TMP_Text[] currentKeyLabelsKeyboard; // Affichage "Touche actuelle : D"
    public Button[] rebindLaneButtonsKeyboard; // 0 à 3
    public Image[] laneIconsKeyboard; // Icônes colorées des lanes

    [Header("Rebind UI - Manette")]
    public TMP_Text[] laneLabelsGamepad; // 0 à 3
    public TMP_Text[] currentKeyLabelsGamepad;
    public Button[] rebindLaneButtonsGamepad; // 0 à 3
    public Image[] laneIconsGamepad;

    [Header("Tips Section")]
    public GameObject tipsSection;
    public TMP_Text tipText;

    [Header("Lane Colors")]
    public Color lane0Color = new Color(0.23f, 0.51f, 0.96f, 1f);
    public Color lane1Color = new Color(0.13f, 0.77f, 0.37f, 1f);
    public Color lane2Color = new Color(0.98f, 0.80f, 0.13f, 1f);
    public Color lane3Color = new Color(0.86f, 0.15f, 0.15f, 1f);

    private InputAction[] actionsKeyboard;
    private InputAction[] actionsGamepad;
    private bool isAnimating = false;

    // animation parameters
    [Header("Animation Parameters")]
    public float backgroundFadeDuration = 0.35f;
    public float containerPopDuration = 0.45f;
    public float sectionSlideDuration = 0.38f;
    public float controlsSwitchDuration = 0.28f;
    public float continuePulseScale = 1.05f;
    public float continuePulseDuration = 0.9f;
    public Vector2 sectionSlideOffset = new Vector2(300f, 0f);
    public Ease popEase = Ease.OutBack;
    public Ease slideEase = Ease.OutCubic;

    private Vector2 videoOriginalAnchoredPos;
    private Vector2 controlsOriginalAnchoredPos;
    private Vector3 contentOriginalScale;

    private void Awake()
    {
        if (keyboardToggleButton != null)
            keyboardToggleButton.onClick.AddListener(() => ShowDeviceControls(true));

        if (gamepadToggleButton != null)
            gamepadToggleButton.onClick.AddListener(() => ShowDeviceControls(false));

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (backButton != null)
            backButton.onClick.AddListener(Hide);

        SetupLaneIconColors();

        if (videoSection != null) videoOriginalAnchoredPos = videoSection.anchoredPosition;
        if (controlsSection != null) controlsOriginalAnchoredPos = controlsSection.anchoredPosition;
        if (contentContainer != null) contentOriginalScale = contentContainer.localScale;
    }

    private void OnEnable()
    {
        // InputManager ne contient pas d'événement global pour les bindings.
        // Mettre à jour à l'activation pour refléter l'état actuel des bindings.
        UpdateControlBindings();
    }

    private void OnDisable()
    {
        // Aucun abonnement à désabonner (InputManager n'expose pas d'événement OnBindingsChanged).
    }

    // Méthode publique que d'autres composants peuvent appeler lorsque les bindings ont changé.
    public void RefreshBindings()
    {
        UpdateControlBindings();
    }

    private void HandleBindingsChanged()
    {
        // Gardée pour compatibilité interne si appelée manuellement.
        UpdateControlBindings();
    }

    public void Show(
        string miniGameName,
        InputAction[] actionsKeyboard,
        InputAction[] actionsGamepad,
        VideoClip tutorialClip = null,
        string tip = null)
    {
        this.actionsKeyboard = actionsKeyboard;
        this.actionsGamepad = actionsGamepad;

        titleText.text = $"Contrôles : {miniGameName}";
        UpdateControlBindings();
        SetupVideo(tutorialClip);

        if (tipsSection != null)
        {
            if (!string.IsNullOrEmpty(tip))
            {
                tipsSection.SetActive(true);
                tipText.text = tip;
            }
            else
            {
                tipsSection.SetActive(false);
            }
        }

        ShowDeviceControls(true);
        gameObject.SetActive(true);
        AnimateIn();
    }

    public void ShowSimple(string miniGameName, VideoClip tutorialClip = null, string tip = null)
    {
        titleText.text = $"Contrôles : {miniGameName}";

        if (keyboardControlsPanel != null) keyboardControlsPanel.SetActive(false);
        if (gamepadControlsPanel != null) gamepadControlsPanel.SetActive(false);
        if (keyboardToggleButton != null) keyboardToggleButton.gameObject.SetActive(false);
        if (gamepadToggleButton != null) gamepadToggleButton.gameObject.SetActive(false);

        SetupVideo(tutorialClip);

        if (tipsSection != null)
        {
            if (!string.IsNullOrEmpty(tip))
            {
                tipsSection.SetActive(true);
                tipText.text = tip;
            }
            else
            {
                tipsSection.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        AnimateIn();
    }

    public void Hide()
    {
        if (isAnimating) return;

        AnimateOut(() =>
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        });
    }

    private void ShowDeviceControls(bool showKeyboard)
    {
        if (keyboardControlsPanel != null)
            keyboardControlsPanel.SetActive(showKeyboard);

        if (gamepadControlsPanel != null)
            gamepadControlsPanel.SetActive(!showKeyboard);

        UpdateToggleButtonColors(showKeyboard);
        AnimateControlsSwitch(showKeyboard);
    }

    private void UpdateToggleButtonColors(bool keyboardActive)
    {
        if (keyboardToggleButton != null)
        {
            var keyboardImage = keyboardToggleButton.GetComponent<Image>();
            keyboardImage.color = keyboardActive ? keyboardActiveColor : inactiveDeviceColor;
        }

        if (gamepadToggleButton != null)
        {
            var gamepadImage = gamepadToggleButton.GetComponent<Image>();
            gamepadImage.color = keyboardActive ? inactiveDeviceColor : gamepadActiveColor;
        }
    }

    private void UpdateControlBindings()
    {
        if (actionsKeyboard != null)
        {
            for (int i = 0; i < actionsKeyboard.Length && i < laneLabelsKeyboard.Length; i++)
            {
                string binding = GetBindingDisplayString(actionsKeyboard[i], "<Keyboard>");
                if (laneLabelsKeyboard[i] != null)
                    laneLabelsKeyboard[i].text = $"Lane {i}";

                if (currentKeyLabelsKeyboard != null && i < currentKeyLabelsKeyboard.Length && currentKeyLabelsKeyboard[i] != null)
                    currentKeyLabelsKeyboard[i].text = $" {binding}";

                if (rebindLaneButtonsKeyboard != null && i < rebindLaneButtonsKeyboard.Length && rebindLaneButtonsKeyboard[i] != null)
                {
                    int idx = i;
                    rebindLaneButtonsKeyboard[i].onClick.RemoveAllListeners();
                    rebindLaneButtonsKeyboard[i].onClick.AddListener(() =>
                        StartRebind(actionsKeyboard[idx], idx, false));
                }
            }
        }

        if (actionsGamepad != null)
        {
            for (int i = 0; i < actionsGamepad.Length && i < laneLabelsGamepad.Length; i++)
            {
                string binding = GetBindingDisplayString(actionsGamepad[i], "<Gamepad>");
                if (laneLabelsGamepad[i] != null)
                    laneLabelsGamepad[i].text = $"Lane {i}";

                if (currentKeyLabelsGamepad != null && i < currentKeyLabelsGamepad.Length && currentKeyLabelsGamepad[i] != null)
                    currentKeyLabelsGamepad[i].text = $"{binding}";

                if (rebindLaneButtonsGamepad != null && i < rebindLaneButtonsGamepad.Length && rebindLaneButtonsGamepad[i] != null)
                {
                    int idx = i;
                    rebindLaneButtonsGamepad[i].onClick.RemoveAllListeners();
                    rebindLaneButtonsGamepad[i].onClick.AddListener(() =>
                        StartRebind(actionsGamepad[idx], idx, true));
                }
            }
        }
    }

    private void SetupLaneIconColors()
    {
        Color[] colors = { lane0Color, lane1Color, lane2Color, lane3Color };

        if (laneIconsKeyboard != null)
        {
            for (int i = 0; i < laneIconsKeyboard.Length && i < colors.Length; i++)
            {
                if (laneIconsKeyboard[i] != null)
                    laneIconsKeyboard[i].color = colors[i];
            }
        }

        if (laneIconsGamepad != null)
        {
            for (int i = 0; i < laneIconsGamepad.Length && i < colors.Length; i++)
            {
                if (laneIconsGamepad[i] != null)
                    laneIconsGamepad[i].color = colors[i];
            }
        }
    }

    private void SetupVideo(VideoClip tutorialClip)
    {
        if (videoPlayer != null)
        {
            if (tutorialClip != null)
            {
                videoPlayer.clip = tutorialClip;

                if (videoDisplay != null)
                {
                    videoPlayer.targetTexture = null;
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                    if (videoPlayer.targetTexture == null)
                    {
                        RenderTexture rt = new RenderTexture(1920, 1080, 0);
                        videoPlayer.targetTexture = rt;
                        videoDisplay.texture = rt;
                    }
                }

                videoPlayer.gameObject.SetActive(true);
                videoPlayer.isLooping = true;
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.gameObject.SetActive(false);
            }
        }
    }

    private void StartRebind(InputAction action, int laneIndex, bool isGamepad)
    {
        TMP_Text currentKeyLabel = isGamepad
            ? (currentKeyLabelsGamepad != null && laneIndex < currentKeyLabelsGamepad.Length ? currentKeyLabelsGamepad[laneIndex] : null)
            : (currentKeyLabelsKeyboard != null && laneIndex < currentKeyLabelsKeyboard.Length ? currentKeyLabelsKeyboard[laneIndex] : null);

        Button rebindButton = isGamepad
            ? (rebindLaneButtonsGamepad != null && laneIndex < rebindLaneButtonsGamepad.Length ? rebindLaneButtonsGamepad[laneIndex] : null)
            : (rebindLaneButtonsKeyboard != null && laneIndex < rebindLaneButtonsKeyboard.Length ? rebindLaneButtonsKeyboard[laneIndex] : null);

        if (currentKeyLabel != null)
            currentKeyLabel.text = "Appuyez sur une touche...";

        if (rebindButton != null)
        {
            var buttonText = rebindButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = "...";
            rebindButton.interactable = false;
        }

        action.Disable();
        var rebinding = action.PerformInteractiveRebinding();

        if (isGamepad)
            rebinding.WithControlsExcluding("<Keyboard>/*");
        else
            rebinding.WithControlsExcluding("<Gamepad>/*");

        rebinding.WithControlsExcluding("<Mouse>/position");
        rebinding.WithControlsExcluding("<Mouse>/delta");

        rebinding.OnComplete(op =>
        {
            action.Enable();
            op.Dispose();

            string binding = GetBindingDisplayString(action, isGamepad ? "<Gamepad>" : "<Keyboard>");

            if (currentKeyLabel != null)
                currentKeyLabel.text = isGamepad ? $"Bouton actuel : {binding}" : $" {binding}";

            if (rebindButton != null)
            {
                var buttonText = rebindButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null) buttonText.text = "Modifier";
                rebindButton.interactable = true;
            }

            InputManager.Instance?.SaveRebinds(isGamepad);
        }).Start();
    }

    private string GetBindingDisplayString(InputAction action, string deviceLayout)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isPartOfComposite) continue;

            string path = binding.effectivePath;
            if (string.IsNullOrEmpty(path)) path = binding.path;

            // Ne garder que les bindings du layout demandé
            if (!path.Contains(deviceLayout)) continue;

            // Essayer d'abord la chaîne formatée fournie par InputAction (si disponible)
            string displayString = null;
            try
            {
                displayString = action.GetBindingDisplayString(i);
            }
            catch
            {
                displayString = null;
            }

            // Si rien, convertir le path en texte lisible
            if (string.IsNullOrEmpty(displayString))
            {
                try
                {
                    displayString = InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
                }
                catch
                {
                    displayString = path;
                }
            }

            if (!string.IsNullOrEmpty(displayString))
            {
                // Nettoyage mineur pour enlever les préfixes courants
                displayString = displayString.Replace("Keyboard/", "").Replace("Gamepad/", "").Trim();
                return displayString;
            }
        }
        return "Non assigné";
    }

    // ==================== ANIMATIONS ====================

    private void AnimateIn()
    {
        isAnimating = true;

        if (backgroundOverlay != null)
            backgroundOverlay.alpha = 0;

        if (contentContainer != null)
        {
            contentContainer.localScale = Vector3.zero;
            contentContainer.rotation = Quaternion.Euler(0, 0, 5);
        }

        Sequence seq = DOTween.Sequence();

        if (backgroundOverlay != null)
            seq.Append(backgroundOverlay.DOFade(1, backgroundFadeDuration).SetEase(Ease.OutQuad));

        if (contentContainer != null)
        {
            seq.Append(contentContainer.DOScale(1, containerPopDuration).SetEase(popEase));
            seq.Join(contentContainer.DORotate(Vector3.zero, containerPopDuration).SetEase(popEase));
        }

        if (videoSection != null)
        {
            videoSection.anchoredPosition = videoOriginalAnchoredPos - (Vector2)Vector2.right * Mathf.Abs(sectionSlideOffset.x);
            videoSection.gameObject.SetActive(true);
            seq.Append(videoSection.DOAnchorPos(videoOriginalAnchoredPos, sectionSlideDuration).SetEase(slideEase));
        }

        if (controlsSection != null)
        {
            controlsSection.anchoredPosition = controlsOriginalAnchoredPos + (Vector2)Vector2.right * Mathf.Abs(sectionSlideOffset.x);
            controlsSection.gameObject.SetActive(true);
            seq.Join(controlsSection.DOAnchorPos(controlsOriginalAnchoredPos, sectionSlideDuration).SetEase(slideEase));
        }

        seq.OnComplete(() =>
        {
            isAnimating = false;
            if (continueButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

                continueButton.transform.DOKill();
                continueButton.transform.localScale = Vector3.one;
                continueButton.transform.DOScale(continuePulseScale, continuePulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        });
    }

    private void AnimateOut(System.Action onComplete = null)
    {
        isAnimating = true;

        if (continueButton != null)
        {
            DOTween.Kill(continueButton.transform);
            continueButton.transform.localScale = Vector3.one;
        }

        Sequence seq = DOTween.Sequence();

        if (videoSection != null)
            seq.Append(videoSection.DOAnchorPos(videoOriginalAnchoredPos - (Vector2)Vector2.right * Mathf.Abs(sectionSlideOffset.x), sectionSlideDuration * 0.8f).SetEase(Ease.InCubic));

        if (controlsSection != null)
            seq.Join(controlsSection.DOAnchorPos(controlsOriginalAnchoredPos + (Vector2)Vector2.right * Mathf.Abs(sectionSlideOffset.x), sectionSlideDuration * 0.8f).SetEase(Ease.InCubic));

        if (contentContainer != null)
        {
            seq.Append(contentContainer.DOScale(0.8f, containerPopDuration * 0.7f).SetEase(Ease.InBack));
            seq.Join(contentContainer.DORotate(new Vector3(0, 0, -5), containerPopDuration * 0.7f).SetEase(Ease.InBack));
        }

        if (backgroundOverlay != null)
            seq.Append(backgroundOverlay.DOFade(0, backgroundFadeDuration * 0.6f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            isAnimating = false;
            onComplete?.Invoke();
        });
    }

    private void AnimateControlsSwitch(bool toKeyboard)
    {
        GameObject panelToShow = toKeyboard ? keyboardControlsPanel : gamepadControlsPanel;
        GameObject panelToHide = toKeyboard ? gamepadControlsPanel : keyboardControlsPanel;

        if (panelToHide != null)
        {
            CanvasGroup cgHide = panelToHide.GetComponent<CanvasGroup>();
            if (cgHide == null) cgHide = panelToHide.AddComponent<CanvasGroup>();
            cgHide.DOKill();
            cgHide.DOFade(0, controlsSwitchDuration).OnComplete(() => panelToHide.SetActive(false));
        }

        if (panelToShow != null)
        {
            CanvasGroup cgShow = panelToShow.GetComponent<CanvasGroup>();
            if (cgShow == null) cgShow = panelToShow.AddComponent<CanvasGroup>();
            cgShow.DOKill();
            cgShow.alpha = 0;
            panelToShow.SetActive(true);
            cgShow.DOFade(1, controlsSwitchDuration);

            RectTransform rt = panelToShow.GetComponent<RectTransform>();
            Vector2 originalPos = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(originalPos.x + 50, originalPos.y);
            rt.DOKill();
            rt.DOAnchorPos(originalPos, controlsSwitchDuration).SetEase(slideEase);
        }

        if (toKeyboard && keyboardToggleButton != null)
        {
            keyboardToggleButton.transform.DOKill();
            keyboardToggleButton.transform.DOScale(1.06f, 0.18f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        if (!toKeyboard && gamepadToggleButton != null)
        {
            gamepadToggleButton.transform.DOKill();
            gamepadToggleButton.transform.DOScale(1.06f, 0.18f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    public void ShowClick(
       string miniGameName,
       VideoClip tutorialClip = null)
    {
        titleText.text = $"Contrôles : {miniGameName}";


        if (videoPlayer != null)
        {
            if (tutorialClip != null)
            {
                videoPlayer.clip = tutorialClip;
                videoPlayer.gameObject.SetActive(true);
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        continueButton.Select();
    }
    private void OnContinueClicked()
    {
        Hide();
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
        if (continueButton != null)
            DOTween.Kill(continueButton.transform);
    }
}