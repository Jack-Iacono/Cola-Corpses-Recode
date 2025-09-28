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

    private bool throwPressed = false;
    private bool drinkPreseed = false;

    private Weapon currentWeapon = null;

    private void Awake()
    {
        inputActions = new InputBind();
        attackActions = inputActions.Attack;
        attackActions.AddCallbacks(this);
    }

    // These methods use the new input system
    public void OnPrimary(InputAction.CallbackContext context)
    {
        throwPressed = !context.canceled;
    }
    public void OnSecondary(InputAction.CallbackContext context)
    {
        drinkPreseed = !context.canceled;
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
