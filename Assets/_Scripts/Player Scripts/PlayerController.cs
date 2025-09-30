using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InputUtil;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, InputBind.IMovementActions
{
    public static LayerMask playerLayerMask;

    private const int playerLayer = 6;
    private const int ghostLayer = 14;

    public CameraController camCont;

    [Header("Movement Variables")]
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float jumpHeight = 10;
    [SerializeField]
    [Tooltip("Negative values will pull player downward, Positive value will push them up")]
    private float gravity = -0.98f;

    private bool isLocked = false;

    [Header("Acceleration Variables", order = 2)]
    [SerializeField]
    private float groundAcceleration = 1;
    [SerializeField]
    private float airAcceleration = 1;
    [SerializeField]
    private float groundDeceleration = 1;
    [SerializeField]
    private float airDeceleration = 1;

    [Header("Interaction Variables")]
    public LayerMask environmentLayers;

    private Vector2 currentMoveInput = Vector2.zero;
    private bool currentJumpInput = false;
    private bool currentSprintInput = false;
    private Vector3 currentMove = Vector3.zero;

    private Vector3 previousFramePosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private CharacterController charCont;

    InputBind inputSys;
    InputBind.MovementActions moveActions;

    private void Awake()
    {
        // Get components on the player
        charCont = GetComponent<CharacterController>();

        // Get the player's layer from the editor
        playerLayerMask = gameObject.layer;

        // Set up the input system
        inputSys = new InputBind();
        moveActions = inputSys.Movement;
        moveActions.AddCallbacks(this);
    }

    public void Warp(Vector3 pos)
    {
        // Disabling the character controller allows the player to be warped, otherwise it doesn't work
        charCont.enabled = false;
        transform.position = pos;
        charCont.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.isPaused)
        {
            CalculateNormalMove();
            Move();
        }
    }
    private void FixedUpdate()
    {
        velocity = (transform.position - previousFramePosition) / Time.fixedDeltaTime;
        previousFramePosition = transform.position;
    }

    private void CalculateNormalMove()
    {
        float moveX = currentMoveInput.x * transform.right.x * moveSpeed + currentMoveInput.y * transform.forward.x * moveSpeed;
        float moveZ = currentMoveInput.x * transform.right.z * moveSpeed + currentMoveInput.y * transform.forward.z * moveSpeed;

        // TEMPORARY
        if (currentSprintInput)
        {
            moveX *= 2f;
            moveZ *= 2f;
        }

        if (charCont.isGrounded)
        {
            // Check whether jump is held or not
            if (currentJumpInput)
            {
                currentMove.y = jumpHeight;
            }

            // Decide whether to use the accel or decel for the player given the presence of input
            float accelX = moveX == 0 ? groundDeceleration : groundAcceleration;
            float accelZ = moveZ == 0 ? groundDeceleration : groundAcceleration;

            // Change the player's current movement vector to reflect the changes made through input
            currentMove.x = Mathf.Lerp(currentMove.x, moveX, accelX);
            currentMove.z = Mathf.Lerp(currentMove.z, moveZ, accelZ);
        }
        else
        {
            // Sets into fall if hitting a ceiling
            if (Physics.Raycast(transform.position, Vector3.up, 1.1f, environmentLayers) && currentMove.y > 0)
                currentMove.y = 0;

            currentMove.y -= gravity * -2 * Time.deltaTime;

            // Decide whether to use the accel or decel for the player given the presence of input
            float accelX = moveX == 0 ? airDeceleration : airAcceleration;
            float accelZ = moveZ == 0 ? airDeceleration : airAcceleration;

            // Change the player's current movement vector to reflect the changes made through input
            currentMove.x = Mathf.Lerp(currentMove.x, moveX, accelX);
            currentMove.z = Mathf.Lerp(currentMove.z, moveZ, accelZ);
        }
    }
    private void Move()
    {
        charCont.Move(currentMove * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        currentMoveInput = moveActions.Move.ReadValue<Vector2>();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if(!currentSprintInput && context.phase == InputActionPhase.Started)
            currentSprintInput = true;
        else if(currentSprintInput && context.phase == InputActionPhase.Canceled)
            currentSprintInput = false;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        currentJumpInput = context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed;
    }

    public CameraController GetCameraController()
    {
        return camCont;
    }

    void OnEnable()
    {
        moveActions.Enable();
    }
    void OnDisable()
    {
        moveActions.Disable();
    }
    private void OnDestroy()
    {
        moveActions.RemoveCallbacks(this);
        inputSys.Dispose();
    }
}
