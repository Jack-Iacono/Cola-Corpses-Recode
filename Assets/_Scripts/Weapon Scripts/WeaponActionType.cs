using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponActionType
{
    public string name = string.Empty;
    public string description = string.Empty;

    public abstract void Use(Weapon usedWeapon);
}
