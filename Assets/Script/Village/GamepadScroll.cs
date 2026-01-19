using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamepadScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 1f;
    public float deadZone = 0.2f;

    private Vector2 stickValue;
    private Coroutine scrollCoroutine;

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        stickValue = ctx.ReadValue<Vector2>();

        // Démarrer la coroutine si le stick dépasse la deadzone
        if (Mathf.Abs(stickValue.y) > deadZone && scrollCoroutine == null)
        {
            scrollCoroutine = StartCoroutine(ContinuousScroll());
        }
    }

    IEnumerator ContinuousScroll()
    {
        while (Mathf.Abs(stickValue.y) > deadZone)
        {
            // Scroll proportionnel à l'inclinaison du stick
            float scrollAmount = stickValue.y * scrollSpeed * Time.deltaTime;
            scrollRect.verticalNormalizedPosition += scrollAmount;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);

            yield return null;
        }

        scrollCoroutine = null;
    }

    void Update()
    {
        // Mise à jour continue de la valeur du stick via le Gamepad actuel
        if (Gamepad.current != null)
        {
            stickValue = Gamepad.current.rightStick.ReadValue();

            // Démarrer le scroll si nécessaire
            if (Mathf.Abs(stickValue.y) > deadZone && scrollCoroutine == null)
            {
                scrollCoroutine = StartCoroutine(ContinuousScroll());
            }
        }
    }
}