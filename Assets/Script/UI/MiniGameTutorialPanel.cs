using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MiniGameTutorialPanel : MonoBehaviour
{
    public TMP_Text titleText;
    public Button continueButton;
    public VideoPlayer videoPlayer;

    [Header("Rebind UI - Clavier")]
    public TMP_Text[] laneLabelsKeyboard; // 0 à 3
    public Button[] rebindLaneButtonsKeyboard; // 0 à 3

    [Header("Rebind UI - Manette")]
    public TMP_Text[] laneLabelsGamepad; // 0 à 3
    public Button[] rebindLaneButtonsGamepad; // 0 à 3

    private InputAction[] actionsKeyboard;
    private InputAction[] actionsGamepad;

    public void Show(
        string miniGameName,
        InputAction[] actionsKeyboard,
        InputAction[] actionsGamepad,
        VideoClip tutorialClip = null)
    {
        titleText.text = $"Contrôles : {miniGameName}";

        this.actionsKeyboard = actionsKeyboard;
        this.actionsGamepad = actionsGamepad;

        // --- Clavier ---
        for (int i = 0; i < actionsKeyboard.Length; i++)
        {
            if (laneLabelsKeyboard != null && i < laneLabelsKeyboard.Length)
                laneLabelsKeyboard[i].text = GetBindingDisplayString(actionsKeyboard[i], "<Keyboard>");

            if (rebindLaneButtonsKeyboard != null && i < rebindLaneButtonsKeyboard.Length)
            {
                int idx = i;
                rebindLaneButtonsKeyboard[i].onClick.RemoveAllListeners();
                rebindLaneButtonsKeyboard[i].onClick.AddListener(() =>
                    StartRebind(actionsKeyboard[idx], laneLabelsKeyboard[idx], false));
            }
        }

        // --- Manette ---
        for (int i = 0; i < actionsGamepad.Length; i++)
        {
            if (laneLabelsGamepad != null && i < laneLabelsGamepad.Length)
                laneLabelsGamepad[i].text = GetBindingDisplayString(actionsGamepad[i], "<Gamepad>");

            if (rebindLaneButtonsGamepad != null && i < rebindLaneButtonsGamepad.Length)
            {
                int idx = i;
                rebindLaneButtonsGamepad[i].onClick.RemoveAllListeners();
                rebindLaneButtonsGamepad[i].onClick.AddListener(() =>
                    StartRebind(actionsGamepad[idx], laneLabelsGamepad[idx], true));
            }
        }

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

    public void Hide()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    // Rebinding pour clavier ou manette
    private void StartRebind(InputAction action, TMP_Text label, bool isGamepad)
    {
        label.text = "...";
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
            label.text = GetBindingDisplayString(action, isGamepad ? "<Gamepad>" : "<Keyboard>");
            InputManager.Instance?.SaveRebinds(isGamepad);
        }).Start();
    }

    // Utilitaire pour afficher la bonne touche selon le device
    private string GetBindingDisplayString(InputAction action, string deviceLayout)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isPartOfComposite && action.bindings[i].effectivePath.Contains(deviceLayout))
                return action.GetBindingDisplayString(i);
        }
        return "";
    }
}