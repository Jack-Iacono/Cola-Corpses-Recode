using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InputUtil;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour, InputBind.IAttackActions
{
    [SerializeField]
    private GameObject canObject;

    InputBind inputActions;
    InputBind.AttackActions attackActions;

    private Weapon currentWeapon = null;

    private ActionState primaryState;
    private ActionState secondaryState;

    private PlayerMovementController ownerPlayer;

    private void Awake()
    {
        inputActions = new InputBind();
        attackActions = inputActions.Attack;
        attackActions.AddCallbacks(this);

        primaryState = ActionState.NONE;
        secondaryState = ActionState.NONE;
        
        ownerPlayer = GetComponent<PlayerMovementController>();
    }

    private void Start()
    {
        // Run this here so sinletons have a chance to be created
        currentWeapon = new Weapon(ownerPlayer, 10, 10, 50, 0.3f);
    }

    private void Update()
    {
        // Update the current weapon and pass in the derived inputs
        currentWeapon.Update(Time.deltaTime, primaryState, secondaryState);

        // Change the states to reflect the player's input
        if (primaryState == ActionState.DOWN)
            primaryState = ActionState.HELD;
        else if (primaryState == ActionState.UP)
            primaryState = ActionState.NONE;

        if(secondaryState == ActionState.DOWN)
            secondaryState = ActionState.HELD;
        else if(secondaryState == ActionState.UP)
            secondaryState = ActionState.NONE;
    }

    // These methods use the new input system
    public void OnPrimary(InputAction.CallbackContext context)
    {
        // Derive key phase via player input
        if(context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed)
            primaryState = ActionState.DOWN;
        else if (context.phase == InputActionPhase.Canceled)
            primaryState = ActionState.UP;
    }
    public void OnSecondary(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed)
            secondaryState = ActionState.DOWN;
        else if(context.phase == InputActionPhase.Canceled)
            secondaryState = ActionState.UP;
    }

    void OnEnable()
    {
        attackActions.Enable();
    }
    void OnDisable()
    {
        attackActions.Disable();
    }

    private void OnDestroy()
    {
        attackActions.RemoveCallbacks(this);
        inputActions.Dispose();
    }
}
