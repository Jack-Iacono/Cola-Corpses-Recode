using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAction
{
    public string name = string.Empty;
    public string description = string.Empty;

    private Weapon weapon;

    public WeaponAction(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public abstract void Update(float dt, ActionState state);
    public abstract void Use();
}
