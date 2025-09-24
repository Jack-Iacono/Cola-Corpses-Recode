using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    private Vector3 currentInput = Vector3.zero;
    private Vector3 currentMove = Vector3.zero;

    private Vector3 previousFramePosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private CharacterController charCont;

    private KeyCode keySprint = KeyCode.LeftShift;
    private bool isSprinting = false;

    private bool isTrapped = false;
    private float trapTimer = 0;

    private void Awake()
    {
        // Get components on the player
        charCont = GetComponent<CharacterController>();

        // Get the player's layer from the editor
        playerLayerMask = gameObject.layer;
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
            GetInput();
            if (!isLocked)
            {
                CalculateNormalMove();
                Move();
            }
        }
    }
    private void FixedUpdate()
    {
        velocity = (transform.position - previousFramePosition) / Time.fixedDeltaTime;
        previousFramePosition = transform.position;
    }

    private void GetInput()
    {
        currentInput = new Vector3
            (
                Input.GetAxis("Horizontal"),
                Input.GetButtonDown("Jump") ? 1 : 0,
                Input.GetAxis("Vertical")
            );
        isSprinting = Input.GetKey(keySprint);
    }
    private void CalculateNormalMove()
    {
        float moveX = currentInput.x * transform.right.x * moveSpeed + currentInput.z * transform.forward.x * moveSpeed;
        float moveZ = currentInput.x * transform.right.z * moveSpeed + currentInput.z * transform.forward.z * moveSpeed;

        // TEMPORARY
        if (isSprinting)
        {
            moveX *= 2f;
            moveZ *= 2f;
        }

        if (charCont.isGrounded)
        {
            if (currentInput.y != 0)
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
}
