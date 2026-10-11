using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    Transform cameraTransform;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;

    bool isMovementPressed;
    bool isRunPressed;

    float rotationFactorPerFrame = 10.0f;
    float walkSpeed = 2.0f;
    float runSpeed = 9.0f;
    float gravity = -9.81f;
    float groundedGravity = -2.0f;
    float verticalVelocity;

    float initialJumpVelocity;
    float maxJumpHeight = 1.0f;
    float maxJumpTime = 0.5f;
    float jumpBufferTime = 0.15f;
    float jumpBufferCounter = 0.0f;

    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;

        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;

        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVariables();
    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2f;
        gravity = (-2f * maxJumpHeight) / (timeToApex * timeToApex);
        initialJumpVelocity = (2f * maxJumpHeight) / timeToApex;
    }

    void handleJump()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedGravity;
            }

            // Start jumping when a jump input is buffered.
            if (jumpBufferCounter > 0f)
            {
                verticalVelocity = initialJumpVelocity;
                jumpBufferCounter = 0f;

                animator.SetBool(isJumpingHash, true);
            }
            else
            {
                // Return to locomotion when grounded and not jumping.
                animator.SetBool(isJumpingHash, false);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    void onJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();

        isMovementPressed = currentMovementInput.sqrMagnitude > 0.01f;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void handleMovement()
    {
        Vector3 cameraForward = cameraTransform != null
            ? cameraTransform.forward
            : transform.forward;

        Vector3 cameraRight = cameraTransform != null
            ? cameraTransform.right
            : transform.right;

        // Keep movement horizontal.
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Recalculate direction using the camera's CURRENT rotation.
        Vector3 moveDirection =
            cameraForward * currentMovementInput.y +
            cameraRight * currentMovementInput.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        float speed = isRunPressed ? runSpeed : walkSpeed;

        currentMovement.x = moveDirection.x * speed;
        currentMovement.z = moveDirection.z * speed;

        // Rotate toward the direction of horizontal movement.
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationFactorPerFrame * Time.deltaTime
            );
        }

        // Apply the vertical velocity calculated by handleJump().
        currentMovement.y = verticalVelocity;

        // Move the character.
        characterController.Move(currentMovement * Time.deltaTime);
    }

    void handleAnimation()
    {
        animator.SetBool(isWalkingHash, isMovementPressed);
        animator.SetBool(
            isRunningHash,
            isMovementPressed && isRunPressed
        );
    }

    void Update()
    {
        handleJump();
        handleMovement();
        handleAnimation();
    }

    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

    void OnDestroy()
    {
        playerInput.Dispose();
    }
}