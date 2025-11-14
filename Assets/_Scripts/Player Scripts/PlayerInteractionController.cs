using InputUtil;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputUtil.InputBind;

public class PlayerInteractionController : MonoBehaviour, IInteractionActions
{
    private PlayerCameraController camCont;

    private float interactRange = 5;

    private InputBind inputSys;
    private InteractionActions actions;

    private InputAction interactAction;

    private void Awake()
    {
        // Register with the input system
        inputSys = new InputBind();
        actions = inputSys.Interaction;
        actions.AddCallbacks(this);

        interactAction = actions.Interact;
    }

    private void Start()
    {
        camCont = PlayerController.Instance.cameraController;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            Ray ray = camCont.GetCameraRay();
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, interactRange))
            {
                if(IInteractable.ObjectReference.ContainsKey(hit.collider.gameObject))
                {
                    IInteractable intObj = IInteractable.ObjectReference[hit.collider.gameObject];
                    intObj.OnInteract();
                }
            }
        }
    }

    void OnEnable()
    {
        actions.Enable();
    }
    void OnDisable()
    {
        actions.Disable();
    }

    private void OnDestroy()
    {
        // Unregister with the input system
        actions.RemoveCallbacks(this);
        inputSys.Dispose();
    }
}
