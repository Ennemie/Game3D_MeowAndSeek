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
    }
}