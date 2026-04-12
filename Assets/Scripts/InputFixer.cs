using UnityEngine;
using UnityEngine.InputSystem;

public class InputFixer : MonoBehaviour
{
    void Start()
    {
        if (Mouse.current != null && !Mouse.current.enabled)
        {
            InputSystem.EnableDevice(Mouse.current);
        }
        if (Keyboard.current != null && !Keyboard.current.enabled)
        {
            InputSystem.EnableDevice(Keyboard.current);
            Debug.Log("Keyboard enabled");
        }

        if (Gamepad.current != null && !Gamepad.current.enabled)
        {
            InputSystem.EnableDevice(Gamepad.current);
        }
    }
}