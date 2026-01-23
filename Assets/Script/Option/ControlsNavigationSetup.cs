using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Script pour configurer automatiquement la navigation UI
/// Attachez ce script à votre ControlsPanel
/// </summary>
public class ControlsNavigationSetup : MonoBehaviour
{
    [Header("Keyboard Section")]
    [Tooltip("Tous les boutons de rebind de la section Traverse")]
    public List<Button> keyboardTraverseButtons = new List<Button>();

    [Tooltip("Tous les boutons de rebind de la section Rhythm")]
    public List<Button> keyboardRhythmButtons = new List<Button>();

    public Button keyboardResetButton;

    [Header("Gamepad Section")]
    [Tooltip("Tous les boutons de rebind de la section Traverse")]
    public List<Button> gamepadTraverseButtons = new List<Button>();

    [Tooltip("Tous les boutons de rebind de la section Rhythm")]
    public List<Button> gamepadRhythmButtons = new List<Button>();

    public Button gamepadResetButton;

    [Header("Device Toggle")]
    public Button keyboardToggleButton;
    public Button gamepadToggleButton;

    private void Start()
    {
        SetupKeyboardNavigation();
        SetupGamepadNavigation();
    }

    [ContextMenu("Setup Navigation")]
    public void SetupAllNavigation()
    {
        SetupKeyboardNavigation();
        SetupGamepadNavigation();
    }

    private void SetupKeyboardNavigation()
    {
        // Navigation pour les boutons Traverse
        SetupVerticalNavigation(keyboardTraverseButtons);

        // Navigation pour les boutons Rhythm
        SetupVerticalNavigation(keyboardRhythmButtons);

        // Connecter Traverse au Rhythm (navigation horizontale)
        if (keyboardTraverseButtons.Count > 0 && keyboardRhythmButtons.Count > 0)
        {
            // Le dernier bouton de Traverse peut aller au premier de Rhythm
            SetCustomNavigation(
                keyboardTraverseButtons[keyboardTraverseButtons.Count - 1],
                null, // up
                keyboardRhythmButtons[0], // down ou right selon votre layout
                keyboardRhythmButtons[0], // left
                null // right
            );

            // Le premier bouton de Rhythm peut revenir au dernier de Traverse
            SetCustomNavigation(
                keyboardRhythmButtons[0],
                keyboardTraverseButtons[keyboardTraverseButtons.Count - 1], // up
                null, // down
                keyboardTraverseButtons[keyboardTraverseButtons.Count - 1], // left
                null // right
            );
        }

        // Connecter le dernier bouton Rhythm au Reset button
        if (keyboardRhythmButtons.Count > 0 && keyboardResetButton != null)
        {
            SetCustomNavigation(
                keyboardRhythmButtons[keyboardRhythmButtons.Count - 1],
                null,
                keyboardResetButton,
                null,
                null
            );

            SetCustomNavigation(
                keyboardResetButton,
                keyboardRhythmButtons[keyboardRhythmButtons.Count - 1],
                null,
                null,
                null
            );
        }
    }

    private void SetupGamepadNavigation()
    {
        // Même logique que keyboard
        SetupVerticalNavigation(gamepadTraverseButtons);
        SetupVerticalNavigation(gamepadRhythmButtons);

        if (gamepadTraverseButtons.Count > 0 && gamepadRhythmButtons.Count > 0)
        {
            SetCustomNavigation(
                gamepadTraverseButtons[gamepadTraverseButtons.Count - 1],
                null,
                gamepadRhythmButtons[0],
                gamepadRhythmButtons[0],
                null
            );

            SetCustomNavigation(
                gamepadRhythmButtons[0],
                gamepadTraverseButtons[gamepadTraverseButtons.Count - 1],
                null,
                gamepadTraverseButtons[gamepadTraverseButtons.Count - 1],
                null
            );
        }

        if (gamepadRhythmButtons.Count > 0 && gamepadResetButton != null)
        {
            SetCustomNavigation(
                gamepadRhythmButtons[gamepadRhythmButtons.Count - 1],
                null,
                gamepadResetButton,
                null,
                null
            );

            SetCustomNavigation(
                gamepadResetButton,
                gamepadRhythmButtons[gamepadRhythmButtons.Count - 1],
                null,
                null,
                null
            );
        }
    }

    /// <summary>
    /// Configure la navigation verticale pour une liste de boutons
    /// </summary>
    private void SetupVerticalNavigation(List<Button> buttons)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            // Up
            if (i > 0)
                nav.selectOnUp = buttons[i - 1];

            // Down
            if (i < buttons.Count - 1)
                nav.selectOnDown = buttons[i + 1];

            buttons[i].navigation = nav;
        }
    }

    /// <summary>
    /// Configure une navigation custom pour un bouton spécifique
    /// </summary>
    private void SetCustomNavigation(Button button, Selectable up, Selectable down, Selectable left, Selectable right)
    {
        if (button == null) return;

        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit;

        if (up != null) nav.selectOnUp = up;
        if (down != null) nav.selectOnDown = down;
        if (left != null) nav.selectOnLeft = left;
        if (right != null) nav.selectOnRight = right;

        button.navigation = nav;
    }
}