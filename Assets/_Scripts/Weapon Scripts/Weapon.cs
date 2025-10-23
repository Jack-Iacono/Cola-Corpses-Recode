using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ModifierUtil;

public enum ActionState { DOWN, UP, HELD, NONE };

public class Weapon
{
    public string name;

    public float damage { get; private set; }
    public float radius { get; private set; }
    public float range { get; private set; }
    public float useTime { get; private set; }

    // Add other stats later, just need this skeleton class for now
    private WeaponAction primaryAction;
    private WeaponAction secondaryAction;

    // Create arrays with enough spaces for each level for the flavors and modifiers
    public int[] flavors = new int[Enum.GetValues(typeof(Flavor)).Length];
    public int[] modifiers = new int[Enum.GetValues(typeof(Modifier)).Length];

    private PlayerController player;

    public Weapon(PlayerController player, float damage, float radius, float range, float useTime)
    {
        this.damage = damage;
        this.range = range;
        this.radius = radius;
        this.useTime = useTime;

        // TEMPORARY!!! testing purposes only
        primaryAction = new WeaponAction_CanThrow(this, player);
        secondaryAction = new WeaponAction_Drink(this, player);

        flavors[0] = 1;
        modifiers[0] = 1;

        this.player = player;
    }

    public void Update(float dt, ActionState primaryState, ActionState secondaryState)
    {
        primaryAction.Update(dt, primaryState, secondaryState);
        secondaryAction.Update(dt, secondaryState, primaryState);
    }
}
