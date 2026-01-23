using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionMenuManager : MonoBehaviour
{
    [Header("Tabs")]
    public Button tabControls;
    public Button tabAudio;
    public Button tabGraphics;
    public Button tabAccessibility;

    [Header("Panels")]
    public GameObject controlsPanel;
    public GameObject audioPanel;
    public GameObject graphicsPanel;
    public GameObject accessibilityPanel;

    [Header("Device Toggle Buttons")]
    public Button keyboardToggleButton;
    public Button gamepadToggleButton;
    public GameObject keyboardSection;
    public GameObject gamepadSection;

    [Header("Tab Colors")]
    public Color inactiveTabColor = new Color(0.61f, 0.64f, 0.69f, 1f); // #9CA3AF
    public Color activeTabColor = new Color(0.77f, 0.71f, 0.99f, 1f); // #C4B5FD
    public Color activeTabBgColor = new Color(0.58f, 0.20f, 0.92f, 0.2f); // #9333EA33

    [Header("Device Toggle Colors")]
    public Color keyboardActiveColor = new Color(0.58f, 0.20f, 0.92f, 1f); // #9333EA purple
    public Color gamepadActiveColor = new Color(0.86f, 0.15f, 0.47f, 1f); // #DB2777 pink
    public Color inactiveDeviceColor = new Color(0.22f, 0.25f, 0.32f, 1f); // #374151 gray

    [Header("Gameplay - Keyboard")]
    public TMP_Text leftKeyLabelKeyboard;
    public TMP_Text rightKeyLabelKeyboard;
    public Button rebindLeftButtonKeyboard;
    public Button rebindRightButtonKeyboard;

    [Header("Gameplay - Gamepad")]
    public TMP_Text leftKeyLabelGamepad;
    public TMP_Text rightKeyLabelGamepad;
    public Button rebindLeftButtonGamepad;
    public Button rebindRightButtonGamepad;

    [Header("Rhythm - Keyboard")]
    public TMP_Text lane0LabelKeyboard;
    public TMP_Text lane1LabelKeyboard;
    public TMP_Text lane2LabelKeyboard;
    public TMP_Text lane3LabelKeyboard;
    public Button rebindLane0ButtonKeyboard;
    public Button rebindLane1ButtonKeyboard;
    public Button rebindLane2ButtonKeyboard;
    public Button rebindLane3ButtonKeyboard;

    [Header("Rhythm - Gamepad")]
    public TMP_Text lane0LabelGamepad;
    public TMP_Text lane1LabelGamepad;
    public TMP_Text lane2LabelGamepad;
    public TMP_Text lane3LabelGamepad;
    public Button rebindLane0ButtonGamepad;
    public Button rebindLane1ButtonGamepad;
    public Button rebindLane2ButtonGamepad;
    public Button rebindLane3ButtonGamepad;

    [Header("Reset Buttons")]
    public Button resetKeyboardButton;
    public Button resetGamepadButton;

    [Header("Audio Sliders")]
    public Slider generalVolumeSlider;
    public TMP_Text generalVolumeText;
    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeText;
    public Slider sfxVolumeSlider;
    public TMP_Text sfxVolumeText;
    public Slider voiceVolumeSlider;
    public TMP_Text voiceVolumeText;

    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Accessibility")]
    public TMP_Dropdown languageDropdown;
    public TMP_Dropdown colorblindDropdown;

    [Header("Footer Buttons")]
    public Button mainMenuButton;
    public Button applyButton;

    [Header("UI")]
    public GameObject firstSelected;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;
    private Resolution[] resolutions;

    [Header("Scroll")]
    public ScrollRect controlsScrollRect;
    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("OptionMenuManager : InputManager non trouvé !");
            return;
        }

        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;

        // Setup tabs
        SetupTabs();

        // Setup device toggle
        SetupDeviceToggleButtons();

        // Setup controls
        SetupRebindButtons(keyboardControls, "Keyboard");
        SetupRebindButtons(gamepadControls, "Gamepad");

        // Setup reset buttons
        SetupResetButtons();

        // Setup audio
        SetupAudioSliders();

        // Setup graphics
        InitGraphics();

        // Setup accessibility
        SetupAccessibility();

        // Setup footer buttons
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        applyButton.onClick.AddListener(ApplySettings);

        RefreshUI();
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    private void Update()
    {
        // Scroll automatique quand on navigue
        if (controlsPanel.activeSelf && controlsScrollRect != null)
        {
            // Si un bouton UI est sélectionné
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                // Vérifier si c'est dans notre scroll view
                if (selected.transform.IsChildOf(controlsScrollRect.content))
                {
                    ScrollToSelected(selected);
                }
            }
        }
    }

    private void ScrollToSelected(GameObject selected)
    {
        // Calculer la position du bouton sélectionné
        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        RectTransform contentRect = controlsScrollRect.content;
        RectTransform viewportRect = controlsScrollRect.viewport;

        // Convertir en position locale
        Vector2 selectedPos = (Vector2)contentRect.InverseTransformPoint(selectedRect.position);
        Vector2 viewportSize = viewportRect.rect.size;
        Vector2 contentSize = contentRect.rect.size;

        // Calculer la position de scroll idéale
        float scrollPos = Mathf.Clamp01(1f - (selectedPos.y / (contentSize.y - viewportSize.y)));

        // Lerp pour un mouvement fluide
        controlsScrollRect.verticalNormalizedPosition = Mathf.Lerp(
            controlsScrollRect.verticalNormalizedPosition,
            scrollPos,
            Time.deltaTime * 10f
        );
    }

    // ==================== TABS ====================
    private void SetupTabs()
    {
        tabControls.onClick.AddListener(() => ShowPanel(controlsPanel, tabControls));
        tabAudio.onClick.AddListener(() => ShowPanel(audioPanel, tabAudio));
        tabGraphics.onClick.AddListener(() => ShowPanel(graphicsPanel, tabGraphics));
        tabAccessibility.onClick.AddListener(() => ShowPanel(accessibilityPanel, tabAccessibility));

        // Afficher le premier panel par défaut
        ShowPanel(controlsPanel, tabControls);
    }

    private void ShowPanel(GameObject panelToShow, Button activeTab)
    {
        // Cacher tous les panels
        controlsPanel.SetActive(false);
        audioPanel.SetActive(false);
        graphicsPanel.SetActive(false);
        accessibilityPanel.SetActive(false);

        // Montrer le panel sélectionné
        panelToShow.SetActive(true);

        // Mettre à jour les styles des tabs
        UpdateTabStyle(tabControls, false);
        UpdateTabStyle(tabAudio, false);
        UpdateTabStyle(tabGraphics, false);
        UpdateTabStyle(tabAccessibility, false);
        UpdateTabStyle(activeTab, true);
    }

    private void UpdateTabStyle(Button tab, bool isActive)
    {
        var tabImage = tab.GetComponent<Image>();
        var tabText = tab.GetComponentInChildren<TMP_Text>();

        if (isActive)
        {
            tabImage.color = activeTabBgColor;
            tabText.color = activeTabColor;
        }
        else
        {
            tabImage.color = Color.clear;
            tabText.color = inactiveTabColor;
        }
    }

    // ==================== DEVICE TOGGLE ====================
    private void SetupDeviceToggleButtons()
    {
        keyboardToggleButton.onClick.AddListener(() => ShowDeviceSection(true));
        gamepadToggleButton.onClick.AddListener(() => ShowDeviceSection(false));

        // Afficher keyboard par défaut
        ShowDeviceSection(true);
    }

    private void ShowDeviceSection(bool showKeyboard)
    {
        keyboardSection.SetActive(showKeyboard);
        gamepadSection.SetActive(!showKeyboard);

        // Mettre à jour les couleurs des boutons
        var keyboardImage = keyboardToggleButton.GetComponent<Image>();
        var gamepadImage = gamepadToggleButton.GetComponent<Image>();

        if (showKeyboard)
        {
            keyboardImage.color = keyboardActiveColor;
            gamepadImage.color = inactiveDeviceColor;
        }
        else
        {
            keyboardImage.color = inactiveDeviceColor;
            gamepadImage.color = gamepadActiveColor;
        }
    }

    // ==================== AUDIO ====================
    private void SetupAudioSliders()
    {
        // General Volume
        generalVolumeSlider.value = PlayerPrefs.GetFloat("GeneralVolume", 0.8f);
        generalVolumeText.text = Mathf.RoundToInt(generalVolumeSlider.value * 100) + "%";
        generalVolumeSlider.onValueChanged.AddListener(val =>
        {
            generalVolumeText.text = Mathf.RoundToInt(val * 100) + "%";
            AudioListener.volume = val;
            PlayerPrefs.SetFloat("GeneralVolume", val);
        });

        // Music Volume
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
        musicVolumeSlider.onValueChanged.AddListener(val =>
        {
            musicVolumeText.text = Mathf.RoundToInt(val * 100) + "%";
            PlayerPrefs.SetFloat("MusicVolume", val);
        });

        // SFX Volume
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
        sfxVolumeSlider.onValueChanged.AddListener(val =>
        {
            sfxVolumeText.text = Mathf.RoundToInt(val * 100) + "%";
            PlayerPrefs.SetFloat("SFXVolume", val);
        });

        // Voice Volume
        voiceVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 0.5f);
        voiceVolumeText.text = Mathf.RoundToInt(voiceVolumeSlider.value * 100) + "%";
        voiceVolumeSlider.onValueChanged.AddListener(val =>
        {
            voiceVolumeText.text = Mathf.RoundToInt(val * 100) + "%";
            PlayerPrefs.SetFloat("VoiceVolume", val);
        });
    }

    // ==================== ACCESSIBILITY ====================
    private void SetupAccessibility()
    {
        // Language dropdown
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "English", "Français", "Español", "Deutsch"
        });
        languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
        languageDropdown.onValueChanged.AddListener(i =>
        {
            PlayerPrefs.SetInt("Language", i);
        });

        // Colorblind dropdown
        colorblindDropdown.ClearOptions();
        colorblindDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "None", "Protanopia", "Deuteranopia", "Tritanopia"
        });

        int saved = PlayerPrefs.GetInt("ColorblindMode", 0);
        colorblindDropdown.onValueChanged.RemoveAllListeners();
        colorblindDropdown.onValueChanged.AddListener(OnColorblindModeChanged);
        colorblindDropdown.SetValueWithoutNotify(saved);
        colorblindDropdown.RefreshShownValue();

        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.Instance.SetMode((ColorblindMode)saved);
        }
    }

    public void OnColorblindModeChanged(int dropdownValue)
    {
        Debug.Log("Colorblind dropdown value: " + dropdownValue);
        PlayerPrefs.SetInt("ColorblindMode", dropdownValue);

        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.Instance.SetMode((ColorblindMode)dropdownValue);
        }
        else
        {
            Debug.LogWarning("ColorblindManager.Instance est null.");
        }
    }

    // ==================== CONTROLS ====================
    private void SetupRebindButtons(PlayerControls controls, string deviceType)
    {
        if (deviceType == "Keyboard")
        {
            rebindLeftButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Gameplay.MoveLeft, leftKeyLabelKeyboard, deviceType));
            rebindRightButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Gameplay.MoveRight, rightKeyLabelKeyboard, deviceType));

            rebindLane0ButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane0, lane0LabelKeyboard, deviceType));
            rebindLane1ButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane1, lane1LabelKeyboard, deviceType));
            rebindLane2ButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane2, lane2LabelKeyboard, deviceType));
            rebindLane3ButtonKeyboard.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane3, lane3LabelKeyboard, deviceType));
        }
        else
        {
            rebindLeftButtonGamepad.onClick.AddListener(() => StartRebind(controls.Gameplay.MoveLeft, leftKeyLabelGamepad, deviceType));
            rebindRightButtonGamepad.onClick.AddListener(() => StartRebind(controls.Gameplay.MoveRight, rightKeyLabelGamepad, deviceType));

            rebindLane0ButtonGamepad.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane0, lane0LabelGamepad, deviceType));
            rebindLane1ButtonGamepad.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane1, lane1LabelGamepad, deviceType));
            rebindLane2ButtonGamepad.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane2, lane2LabelGamepad, deviceType));
            rebindLane3ButtonGamepad.onClick.AddListener(() => StartRebind(controls.Rhytm.Lane3, lane3LabelGamepad, deviceType));
        }
    }

    private void SetupResetButtons()
    {
        resetKeyboardButton.onClick.AddListener(() => ResetBindings(false));
        resetGamepadButton.onClick.AddListener(() => ResetBindings(true));
    }

    private void StartRebind(InputAction action, TMP_Text label, string deviceType)
    {
        label.text = "...";
        action.Disable();
        var rebinding = action.PerformInteractiveRebinding();

        if (deviceType == "Keyboard")
            rebinding.WithControlsExcluding("<Gamepad>/*");
        else
            rebinding.WithControlsExcluding("<Keyboard>/*");

        rebinding.OnComplete(op =>
        {
            action.Enable();
            op.Dispose();
            RefreshUI();
            InputManager.Instance.SaveRebinds(deviceType == "Gamepad");
        }).Start();
    }

    public void ResetBindings(bool forGamepad = false)
    {
        if (!forGamepad)
        {
            keyboardControls.asset.RemoveAllBindingOverrides();
            RefreshUI();
            PlayerPrefs.DeleteKey("rebinds_keyboard");
        }
        else
        {
            gamepadControls.asset.RemoveAllBindingOverrides();
            RefreshUI();
            PlayerPrefs.DeleteKey("rebinds_gamepad");
        }
    }

    private void RefreshUI()
    {
        leftKeyLabelKeyboard.text = GetBindingDisplayString(keyboardControls.Gameplay.MoveLeft, "<Keyboard>");
        rightKeyLabelKeyboard.text = GetBindingDisplayString(keyboardControls.Gameplay.MoveRight, "<Keyboard>");
        lane0LabelKeyboard.text = GetBindingDisplayString(keyboardControls.Rhytm.Lane0, "<Keyboard>");
        lane1LabelKeyboard.text = GetBindingDisplayString(keyboardControls.Rhytm.Lane1, "<Keyboard>");
        lane2LabelKeyboard.text = GetBindingDisplayString(keyboardControls.Rhytm.Lane2, "<Keyboard>");
        lane3LabelKeyboard.text = GetBindingDisplayString(keyboardControls.Rhytm.Lane3, "<Keyboard>");

        leftKeyLabelGamepad.text = GetBindingDisplayString(gamepadControls.Gameplay.MoveLeft, "<Gamepad>");
        rightKeyLabelGamepad.text = GetBindingDisplayString(gamepadControls.Gameplay.MoveRight, "<Gamepad>");
        lane0LabelGamepad.text = GetBindingDisplayString(gamepadControls.Rhytm.Lane0, "<Gamepad>");
        lane1LabelGamepad.text = GetBindingDisplayString(gamepadControls.Rhytm.Lane1, "<Gamepad>");
        lane2LabelGamepad.text = GetBindingDisplayString(gamepadControls.Rhytm.Lane2, "<Gamepad>");
        lane3LabelGamepad.text = GetBindingDisplayString(gamepadControls.Rhytm.Lane3, "<Gamepad>");
    }

    private string GetBindingDisplayString(InputAction action, string deviceLayout)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isPartOfComposite && action.bindings[i].effectivePath.Contains(deviceLayout))
                return action.GetBindingDisplayString(i);
        }
        return "";
    }

    // ==================== GRAPHICS ====================
    private void InitGraphics()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Low", "Medium", "High", "Ultra" });
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.onValueChanged.AddListener(i =>
        {
            QualitySettings.SetQualityLevel(i);
            PlayerPrefs.SetInt("QualityLevel", i);
        });

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(i =>
        {
            var r = resolutions[i];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", i);
        });

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(val => Screen.fullScreen = val);
    }

    // ==================== FOOTER ====================
    public void ApplySettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings applied and saved!");
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Retour au menu principal");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu_principal");
    }
}