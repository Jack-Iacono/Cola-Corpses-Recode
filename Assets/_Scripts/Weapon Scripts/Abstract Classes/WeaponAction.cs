using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModifierUtil;

public abstract class WeaponAction
{
    public string name = string.Empty;
    public string description = string.Empty;

    protected Weapon weapon;

    public WeaponAction(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public abstract void Update(float dt);

    public abstract void Use(ActionState state);
    public abstract void OtherUse();
}
