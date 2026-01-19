using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class UniversalMouse : MonoBehaviour
{
    public float speed = 1200f;

    private Mouse virtualMouse;
    private Vector2 currentPos;

    void OnEnable()
    {
        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }

        InputSystem.EnableDevice(virtualMouse);

        currentPos = new Vector2(Screen.width / 2, Screen.height / 2);
        InputState.Change(virtualMouse.position, currentPos);
    }

    void OnDisable()
    {
        if (virtualMouse != null)
            InputSystem.RemoveDevice(virtualMouse);
    }

    void Update()
    {
        Vector2 move = Vector2.zero;

        // Gamepad
        if (Gamepad.current != null)
            move += Gamepad.current.leftStick.ReadValue();

        // Clavier
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) move.y += 1;
            if (Keyboard.current.sKey.isPressed) move.y -= 1;
            if (Keyboard.current.aKey.isPressed) move.x -= 1;
            if (Keyboard.current.dKey.isPressed) move.x += 1;
        }

        if (move != Vector2.zero)
        {
            currentPos += move * speed * Time.deltaTime;
            currentPos.x = Mathf.Clamp(currentPos.x, 0, Screen.width);
            currentPos.y = Mathf.Clamp(currentPos.y, 0, Screen.height);

            InputState.Change(virtualMouse.position, currentPos);
        }

        //  Clic gauche (manette A ou clavier espace)
        bool leftClick =
            (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed) ||
            (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);

        InputState.Change(virtualMouse.leftButton, leftClick);

        //  Clic droit (manette B ou clavier Escape)
        bool rightClick =
            (Gamepad.current != null && Gamepad.current.buttonEast.isPressed) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.isPressed);

        InputState.Change(virtualMouse.rightButton, rightClick);
    }
}
