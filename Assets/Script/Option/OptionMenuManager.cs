using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionMenuManager : MonoBehaviour
{
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

    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Référence Dropdown Daltonisme")]
    public TMP_Dropdown colorblindDropdown;


    [Header("UI")]
    public GameObject firstSelected;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;
    private Resolution[] resolutions;

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("OptionMenuManager : InputManager non trouvé !");
            return;
        }

        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;

        SetupRebindButtons(keyboardControls, "Keyboard");
        SetupRebindButtons(gamepadControls, "Gamepad");

        InitGraphics();
        RefreshUI();

        // Initialiser le dropdown avec la valeur sauvegardée
        int saved = PlayerPrefs.GetInt("ColorblindMode", 0);

        // S'assurer qu'on a UN SEUL listener
        colorblindDropdown.onValueChanged.RemoveListener(OnColorblindModeChanged);
        colorblindDropdown.onValueChanged.AddListener(OnColorblindModeChanged);

        // Initialiser sans déclencher l'event
        colorblindDropdown.SetValueWithoutNotify(saved);
        colorblindDropdown.RefreshShownValue();

        // Appliquer le mode au démarrage
        if (ColorblindManager.Instance != null)
        {
            ColorblindManager.Instance.SetMode((ColorblindMode)saved);
        }


    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

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

    public void ResetBindings(bool forGamepad = false)
    {
        if (!forGamepad)
        {
            // Supprime tous les overrides clavier
            keyboardControls.asset.RemoveAllBindingOverrides();
            // Met à jour l’UI si tu utilises OptionMenuManager
            RefreshUI();
            PlayerPrefs.DeleteKey("rebinds_keyboard");
        }
        else
        {
            // Supprime tous les overrides manette
            gamepadControls.asset.RemoveAllBindingOverrides();
            RefreshUI();
            PlayerPrefs.DeleteKey("rebinds_gamepad");
        }
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

    private void StartRebind(InputAction action, TMP_Text label, string deviceType)
    {
        label.text = "...";
        action.Disable();
        var rebinding = action.PerformInteractiveRebinding();
        if (deviceType == "Keyboard") rebinding.WithControlsExcluding("<Gamepad>/*");
        else rebinding.WithControlsExcluding("<Keyboard>/*");

        rebinding.OnComplete(op =>
        {
            action.Enable();
            op.Dispose();
            RefreshUI();
            InputManager.Instance.SaveRebinds(deviceType == "Gamepad");
        }).Start();
    }

    public void ApplySettings() => PlayerPrefs.Save();

    // Appelée par le dropdown OnValueChanged(int)
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
            Debug.LogWarning("OptionMenuManager : ColorblindManager.Instance est null, impossible d'appliquer le filtre.");
        }
    }


}
