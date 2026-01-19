    using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class RebindUI : MonoBehaviour
{
    [Header("Keyboard Controls")]
    public TMP_Text leftKeyLabel;
    public TMP_Text rightKeyLabel;
    public Button rebindLeftButton;
    public Button rebindRightButton;

    [Header("Keyboard Rhythm Lanes")]
    public TMP_Text lane0Label;
    public TMP_Text lane1Label;
    public TMP_Text lane2Label;
    public TMP_Text lane3Label;
    public Button rebindLane0Button;
    public Button rebindLane1Button;
    public Button rebindLane2Button;
    public Button rebindLane3Button;

    [Header("Gamepad Controls")]
    public TMP_Text leftGamepadLabel;
    public TMP_Text rightGamepadLabel;
    public Button rebindLeftGamepadButton;
    public Button rebindRightGamepadButton;

    [Header("Gamepad Rhythm Lanes")]
    public TMP_Text lane0GamepadLabel;
    public TMP_Text lane1GamepadLabel;
    public TMP_Text lane2GamepadLabel;
    public TMP_Text lane3GamepadLabel;
    public Button rebindLane0GamepadButton;
    public Button rebindLane1GamepadButton;
    public Button rebindLane2GamepadButton;
    public Button rebindLane3GamepadButton;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private void Awake()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("RebindUI : InputManager non trouvé !");
            return;
        }

        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;

        // Bind des boutons clavier
        rebindLeftButton.onClick.AddListener(() => StartRebind(keyboardControls.Gameplay.MoveLeft, leftKeyLabel, false));
        rebindRightButton.onClick.AddListener(() => StartRebind(keyboardControls.Gameplay.MoveRight, rightKeyLabel, false));

        rebindLane0Button.onClick.AddListener(() => StartRebind(keyboardControls.Rhytm.Lane0, lane0Label, false));
        rebindLane1Button.onClick.AddListener(() => StartRebind(keyboardControls.Rhytm.Lane1, lane1Label, false));
        rebindLane2Button.onClick.AddListener(() => StartRebind(keyboardControls.Rhytm.Lane2, lane2Label, false));
        rebindLane3Button.onClick.AddListener(() => StartRebind(keyboardControls.Rhytm.Lane3, lane3Label, false));

        // Bind des boutons manette
        rebindLeftGamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Gameplay.MoveLeft, leftGamepadLabel, true));
        rebindRightGamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Gameplay.MoveRight, rightGamepadLabel, true));

        rebindLane0GamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Rhytm.Lane0, lane0GamepadLabel, true));
        rebindLane1GamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Rhytm.Lane1, lane1GamepadLabel, true));
        rebindLane2GamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Rhytm.Lane2, lane2GamepadLabel, true));
        rebindLane3GamepadButton.onClick.AddListener(() => StartRebind(gamepadControls.Rhytm.Lane3, lane3GamepadLabel, true));
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        // --- Clavier ---
        leftKeyLabel.text = keyboardControls.Gameplay.MoveLeft.GetBindingDisplayString();
        rightKeyLabel.text = keyboardControls.Gameplay.MoveRight.GetBindingDisplayString();

        lane0Label.text = keyboardControls.Rhytm.Lane0.GetBindingDisplayString();
        lane1Label.text = keyboardControls.Rhytm.Lane1.GetBindingDisplayString();
        lane2Label.text = keyboardControls.Rhytm.Lane2.GetBindingDisplayString();
        lane3Label.text = keyboardControls.Rhytm.Lane3.GetBindingDisplayString();

        // --- Manette ---
        leftGamepadLabel.text = gamepadControls.Gameplay.MoveLeft.GetBindingDisplayString();
        rightGamepadLabel.text = gamepadControls.Gameplay.MoveRight.GetBindingDisplayString();

        lane0GamepadLabel.text = gamepadControls.Rhytm.Lane0.GetBindingDisplayString();
        lane1GamepadLabel.text = gamepadControls.Rhytm.Lane1.GetBindingDisplayString();
        lane2GamepadLabel.text = gamepadControls.Rhytm.Lane2.GetBindingDisplayString();
        lane3GamepadLabel.text = gamepadControls.Rhytm.Lane3.GetBindingDisplayString();
    }

    private void StartRebind(InputAction action, TMP_Text label, bool isGamepad)
    {
        if (action == null)
        {
            Debug.LogWarning("StartRebind : action null");
            return;
        }

        label.text = "...";
        action.Disable();

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnComplete(operation =>
            {
                action.Enable();
                operation.Dispose();
                RefreshUI();
                InputManager.Instance.SaveRebinds(isGamepad);
            })
            .Start();
    }
}
