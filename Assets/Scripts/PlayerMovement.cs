using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInput playerInput;
    private CharacterController controller;
    private PlayerProperties playerProperties;
    [HideInInspector, Networked] public bool isMovementEnabled {  get; set; } = false;

    private Vector3 moveDirection;
    public float speed;
    private bool isMoving;
    public float rotationSpeed;
    private float gravity = -9.81f;
    private bool isGrounded;
    private bool isJumping;
    private bool canJump;
    private bool isSounding;
    private bool soundPressed;
    private Vector2 moveInput;
    private bool isJumpPressed = false;
    [HideInInspector] public bool isSkillEnabled = false;
    private float verticalVelocity;
    private Coroutine disguiseResetCoroutine;
    private int disguiseTime = 15;
    private CanvaController canvaController;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerProperties = GetComponent<PlayerProperties>();
        canvaController = GameObject.FindWithTag("Canva").GetComponent<CanvaController>();

        canJump = true;
        isMoving = false;
        isJumping = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (controller == null || !controller.enabled) return;

        soundPressed = playerInput.actions["Interact"].IsPressed();
        MakeSound(soundPressed);

        if (isMovementEnabled)
        {
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            isJumpPressed = playerInput.actions["Jump"].IsPressed();
        }
        CheckIsMoving(moveInput);
        UpdateAnim(moveInput);
        Jump(isJumpPressed);
        MoveWithCameraDirection(moveInput);
        GravitySimulate();

        Vector3 horizontal = moveDirection * speed;
        Vector3 vertical = Vector3.up * verticalVelocity;

        controller.Move((horizontal + vertical) * Runner.DeltaTime);

    }
    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        if (Keyboard.current.qKey.wasPressedThisFrame && isSkillEnabled)
        {
            DoSkill();
        }
    }
    private void DoSkill()
    {
        if (playerProperties.isSeeker)
        {
            if (!playerProperties.attackEffect.gameObject.activeSelf)
            {
                if (playerProperties.UseMana(30f)) playerProperties.isAttacking = true;
            }
        }
        else
        {
            if (playerProperties.disguiseIndex == 0)
            {
                // pick a random disguise (ensure there is at least one)
                if (playerProperties.disguiseProps != null && playerProperties.disguiseProps.Count > 1)
                {
                    playerProperties.disguiseIndex = Random.Range(1, playerProperties.disguiseProps.Count);

                    // start/reset the timer that will revert the disguise after 15s
                    if (disguiseResetCoroutine != null) StopCoroutine(disguiseResetCoroutine);
                    disguiseResetCoroutine = StartCoroutine(ResetDisguiseAfterDelay(disguiseTime));
                    if(canvaController != null) canvaController.OpenDisguiseHub(playerProperties.NickName.ToString(), playerProperties.Hp, playerProperties.Mana, disguiseTime);
                }
            }
            else
            {
                // cancel pending reset and immediately revert
                if (disguiseResetCoroutine != null) { StopCoroutine(disguiseResetCoroutine); disguiseResetCoroutine = null; }
                playerProperties.disguiseIndex = 0;
                if(canvaController != null) canvaController.CloseDisguiseHub();
            }
        }
    }

    private IEnumerator ResetDisguiseAfterDelay(float delay)
    {
        for (int i = 0; i < delay; i++)
        {
            yield return new WaitForSecondsRealtime(1f);

        }
        // ensure only the input-authority instance writes the networked property
        if (Object != null && Object.HasInputAuthority)
        {
            playerProperties.disguiseIndex = 0;
        }
        disguiseResetCoroutine = null;
    }
    private void MakeSound(bool soundPressed)
    {
        if (soundPressed && !isMoving && !isJumping)
        {
            isSounding = true;
        }
        else
        {
            isSounding = false;
        }
    }
    private void CheckIsMoving(Vector2 moveInput)
    {
        isMoving = moveInput.sqrMagnitude > 0.01f;
    }
    private void UpdateAnim(Vector2 moveInput)
    {
        if(isSounding)
        {
            playerProperties.SetAnimationState(PlayerProperties.AnimState.Sound);
            return;
        }
        if (isJumping)
        {
            playerProperties.SetAnimationState(PlayerProperties.AnimState.Jump);
            return;
        }
        if (moveInput.sqrMagnitude > 0.01f)
        {
            playerProperties.SetAnimationState(PlayerProperties.AnimState.Walk);
        }
        else
        {
            playerProperties.SetAnimationState(PlayerProperties.AnimState.Idle);
        }
    }
    private void Jump(bool isJumpPressed)
    {
        if (isGrounded && isJumpPressed && canJump && playerProperties.UseMana(20f))
        {
            isJumping = true;
            StartCoroutine(JumpCoolDown());
            verticalVelocity = Mathf.Sqrt(-2f * gravity * 1.5f);
            playerProperties.SetAnimationState(PlayerProperties.AnimState.Jump);
        }
        if(isGrounded && !isJumpPressed)
        {
            isJumping = false;
        }
    }
    private IEnumerator JumpCoolDown()
    {
        canJump = false;
        yield return new WaitForSecondsRealtime(2f);
        canJump = true;
    }
    private void GravitySimulate()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -0.5f;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        verticalVelocity += gravity * Runner.DeltaTime;
    }
    void MoveWithCameraDirection(Vector2 moveInput)
    {
        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        moveDirection = (camForward * moveInput.y + camRight * moveInput.x);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            moveDirection.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Runner.DeltaTime
            );
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }
}