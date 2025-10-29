using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using InputUtil;
using UnityEngine.InputSystem;
using static InputUtil.InputBind;

public class CameraController : MonoBehaviour, InputBind.ICameraActions
{
    [Header("GameObjects")]
    public PlayerMovementController playerCont;

    // Using 2 different camera for post processing effects later on, could change to layermasks
    public Camera cam;

    [Header("Characteristics")]
    [Tooltip("The sensistivity of the mouse moving the camera")]
    public float sensitivity = 100;

    [Header("Layer Masks")]
    public LayerMask collideLayers;
    public LayerMask interactLayers;

    private float xRotation = 0;
    private float yRotation = 0;

    private float mouseInputX = 0;
    private float mouseInputY = 0;

    InputBind inputActions;
    InputBind.CameraActions cameraActions;

    private void Awake()
    {
        // Setting up key bindings
        inputActions = new InputBind();
        cameraActions = inputActions.Camera;
        cameraActions.AddCallbacks(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the camera
        if(!GameController.isPaused)
            MoveCamera();
    }

    private void MoveCamera()
    {
        //Gets the real rotation of the camera
        xRotation = xRotation - mouseInputY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Getting the horizontal rotations from mouse inputs
        yRotation = yRotation + mouseInputX;

        //Moves the camera around the player
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -90, 90), 0f, 0f);

        //Rotates the player to always be facing the direction of the camera
        playerCont.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        Vector2 input = cameraActions.Rotate.ReadValue<Vector2>();
        mouseInputX = input.x * sensitivity;
        mouseInputY = input.y * sensitivity;
    }

    #region Get Methods

    public bool GetCameraSight(Collider col)
    {
        // Get a ray from the camera's position in a straight line forward
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Check if the ray hits any layers that we are interested in
        if (Physics.Raycast(ray, out hit, 1000, collideLayers))
        {
            // If the collider is the one we are looking for, return true
            if (hit.collider == col)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a ray straight forward from the camera's position
    /// </summary>
    /// <returns>The ray representing the camera's sightline</returns>
    public Ray GetCameraRay()
    {
        return new Ray(transform.position, transform.forward);
    }
    public Vector3 GetCameraSightVector()
    {
        return transform.forward;
    }

    #endregion

    void OnEnable()
    {
        cameraActions.Enable();
    }
    void OnDisable()
    {
        cameraActions.Disable();
    }

    private void OnDestroy()
    {
        cameraActions.RemoveCallbacks(this);
        inputActions.Dispose();
    }

}