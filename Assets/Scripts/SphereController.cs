using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereController : NetworkBehaviour
{
    private CharacterController controller;

    private PlayerInput playerInput;

    public float speed = 5f;

    

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }
    void Start()
    {
    }
    public override void FixedUpdateNetwork()
    {
        var moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        controller.Move(move * speed * Runner.DeltaTime);
    }
}