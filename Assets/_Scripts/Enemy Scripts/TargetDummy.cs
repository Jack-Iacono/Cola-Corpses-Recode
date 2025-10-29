using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDummy : EffectDamageable
{
    protected override void Awake()
    {
        base.Awake();
        invincible = true;
    }
}
