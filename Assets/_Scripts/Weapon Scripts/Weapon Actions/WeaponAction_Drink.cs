using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction_Drink : WeaponAction
{
    private float drinkTime = 0f;
    private float drinkTimer = 0f;
    private bool isDrinking = false;

    public WeaponAction_Drink(Weapon weapon, PlayerController player) : base(weapon, player)
    {
        drinkTime = weapon.useTime * 5;
    }

    public override void Update(float dt, ActionState state)
    {
        if (state == ActionState.DOWN)
        {
            if(!isDrinking)
            {
                isDrinking = true;
                drinkTimer = drinkTime;
            }
        }

        if (isDrinking)
        {
            if (drinkTimer > 0)
                drinkTimer -= dt;
            else
            {
                isDrinking = false;
                Use();
            }
        }
    }
    public override void Use()
    {
        Debug.Log("done");
    }
}
