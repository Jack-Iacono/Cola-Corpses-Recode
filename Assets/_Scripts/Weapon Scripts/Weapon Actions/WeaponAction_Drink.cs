using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModifierUtil;

public class WeaponAction_Drink : SecondaryWeaponAction
{
    private float drinkTime = 0f;
    private float drinkTimer = 0f;
    private bool isDrinking = false;

    public WeaponAction_Drink(Weapon weapon, PlayerMovementController player) : base(weapon, player)
    {
        drinkTime = weapon.useTime * 5;
    }

    public override void Update(float dt)
    {
        if (isDrinking)
        {
            if (drinkTimer > 0)
                drinkTimer -= dt;
            else
            {
                // Play the throw sound
                AudioManager.Play(AudioManager.SoundType.w_Drink);

                isDrinking = false;
                PlayerController.statusController.ApplyBuffs(weapon.flavors);
            }
        }
    }

    public override void Use(ActionState state)
    {
        if (state == ActionState.DOWN && !isDrinking)
        {
            isDrinking = true;
            drinkTimer = drinkTime;
        }
    }
    public override void OtherUse()
    {
        isDrinking = false;
        drinkTimer = drinkTime;
    }
}
