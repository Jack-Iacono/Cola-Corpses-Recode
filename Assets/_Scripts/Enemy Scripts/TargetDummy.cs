using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IDamageable;

public class TargetDummy : StatusSystem
{
    protected override void Awake()
    {
        base.Awake();
        invincible = true;
    }

    public override DamageableType GetDamageableType()
    {
        return IDamageable.DamageableType.ENEMY;
    }
}
