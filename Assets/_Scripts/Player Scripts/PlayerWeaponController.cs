using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InputUtil;

public class PlayerWeaponController : MonoBehaviour, InputSystem.IAttackActions
{
    [SerializeField]
    private GameObject canObject;

    InputSystem inputActions;
    InputSystem.AttackActions attackActions;

    private bool throwPressed = false;
    private bool drinkPreseed = false;

    private void Awake()
    {
        inputActions = new InputSystem();
        attackActions = inputActions.Attack;
        attackActions.AddCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(throwPressed);
    }

    // These methods use the new input system
    public void OnPrimary(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        throwPressed = !context.canceled;
    }
    public void OnSecondary(UnityEngine.InputSystem.InputAction.CallbackContext context)
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
