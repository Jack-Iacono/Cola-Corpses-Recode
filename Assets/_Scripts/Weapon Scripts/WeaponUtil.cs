using System;
using UnityEngine;

public static class WeaponUtil
{
    // Used to represent weapon actions, both primary and secondary
    public enum pWeaponAction { THROW };
    public enum sWeaponAction { DRINK };

    // Constraints that can be places on
    public static readonly Vector2 DAMAGE_CONSTRAINTS = new Vector2(0, 100);
    public static readonly Vector2 RADIUS_CONSTRAINTS = new Vector2(0, 100);
    public static readonly Vector2 RANGE_CONSTRAINTS = new Vector2(0, 100);
    public static readonly Vector2 USETIME_CONSTRAINTS = new Vector2(0, 100);

    // Get a random enum representing an action type. Do this to avoid storing unnecessary instances of weapon actions
    public static pWeaponAction GetRandomPrimaryActionType()
    {
        pWeaponAction[] actions = (pWeaponAction[])Enum.GetValues(typeof(pWeaponAction));
        return actions[UnityEngine.Random.Range(0, actions.Length)];
    }
    public static sWeaponAction GetRandomSecondaryActionType()
    {
        sWeaponAction[] actions = (sWeaponAction[])Enum.GetValues(typeof(sWeaponAction));
        return actions[UnityEngine.Random.Range(0, actions.Length)];
    }

    // Translate the various enums into actual weapon actions that will be used by the weapon itself
    public static PrimaryWeaponAction GetPrimaryWeaponAction(pWeaponAction pWeaponAction, Weapon weapon)
    {
        switch (pWeaponAction)
        {
            case pWeaponAction.THROW:
                return new WeaponAction_CanThrow(weapon);
            default:
                return null;
        }
    }
    public static SecondaryWeaponAction GetSecondaryWeaponAction(sWeaponAction sWeaponAction, Weapon weapon)
    {
        switch (sWeaponAction)
        {
            case sWeaponAction.DRINK:
                return new WeaponAction_Drink(weapon);
            default:
                return null;
        }
    }
}
