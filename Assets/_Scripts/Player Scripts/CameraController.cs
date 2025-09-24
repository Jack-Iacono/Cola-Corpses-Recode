using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("GameObjects")]
    public PlayerController playerCont;

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

    private Vector3 normalPosition = Vector3.zero;

    private bool isLocked = false;

    private void Awake()
    {
        // Sets the normal position that the camera should be in
        normalPosition = transform.localPosition;
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
        //Taking in the input from the mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        //Gets the real rotation of the camera
        xRotation = xRotation - mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Getting the horizontal rotations from mouse inputs
        yRotation = yRotation + mouseX;

        //Moves the camera around the player
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -90, 90), 0f, 0f);

        //Rotates the player to always be facing the direction of the camera
        playerCont.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
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

    #endregion
}