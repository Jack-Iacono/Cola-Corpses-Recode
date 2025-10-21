using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAction
{
    public string name = string.Empty;
    public string description = string.Empty;

    protected Weapon weapon;
    protected PlayerController player;

    public WeaponAction(Weapon weapon, PlayerController player)
    {
        this.weapon = weapon;
        this.player = player;
    }

    public abstract void Update(float dt, ActionState state, ActionState otherState);
    public abstract void Use();
}
