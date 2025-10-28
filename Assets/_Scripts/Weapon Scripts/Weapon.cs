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
    public Dictionary<Flavor, int> flavors = new Dictionary<Flavor, int>();
    public Dictionary<Trait, int> traits = new Dictionary<Trait, int>();

    private PlayerController player;

    public Weapon(PlayerController player, float damage, float radius, float range, float useTime)
    {
        // Initialize the flavor dictionary with the correct keys for usage later
        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            flavors.Add(flavor, 0);
        }
        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            traits.Add(trait, 0);
        }

        this.damage = damage;
        this.range = range;
        this.radius = radius;
        this.useTime = useTime;

        // TEMPORARY!!! testing purposes only
        primaryAction = new WeaponAction_CanThrow(this, player);
        secondaryAction = new WeaponAction_Drink(this, player);

        flavors[Flavor.COLA] = 1;

        traits[Trait.SOUR] = 1;
        traits[Trait.SPICY] = 1;

        this.player = player;
    }

    public void Update(float dt, ActionState primaryState, ActionState secondaryState)
    {
        if (primaryState != ActionState.NONE)
        {
            primaryAction.Use(primaryState);
            secondaryAction.OtherUse();
        }
        if (secondaryState != ActionState.NONE)
        {
            secondaryAction.Use(secondaryState);
            primaryAction.OtherUse();
        }

        primaryAction.Update(dt);
        secondaryAction.Update(dt);
    }
}
