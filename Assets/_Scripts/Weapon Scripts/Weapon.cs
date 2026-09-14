using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WeaponPartUtil;
using static ModifierUtil;
using static WeaponUtil;

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

    private PlayerMovementController player;

    public Weapon(WeaponPartPrimary pPart, WeaponPartSecondary sPart, WeaponPart[] extraParts = null)
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

        // Set the stats from both parts at the same time
        damage = pPart.damage + sPart.damage;
        range = pPart.range + sPart.range;
        radius = pPart.radius + sPart.radius;
        useTime = pPart.useTime  + sPart.useTime;

        // Add the flavors/traits from the primary part
        foreach(Flavor flavor in pPart.flavors.Keys)
        {
            flavors[flavor] += pPart.flavors[flavor];
        }
        foreach(Trait trait in pPart.traits.Keys)
        {
            traits[trait] += pPart.traits[trait];
        }

        // Add the flavors/traits from the secondary part
        foreach (Flavor flavor in sPart.flavors.Keys)
        {
            flavors[flavor] += sPart.flavors[flavor];
        }
        foreach (Trait trait in sPart.traits.Keys)
        {
            traits[trait] += sPart.traits[trait];
        }

        // Check to see if there are extra parts
        if(extraParts != null)
        {
            // Loop through additional parts to add stats from them
            for (int i = 0; i < extraParts.Length; i++)
            {
                damage += extraParts[i].damage;
                range += extraParts[i].range;
                radius += extraParts[i].radius;
                useTime += extraParts[i].useTime;

                // Add the flavors/traits from the part
                foreach (Flavor flavor in extraParts[i].flavors.Keys)
                {
                    flavors[flavor] += extraParts[i].flavors[flavor];
                }
                foreach (Trait trait in extraParts[i].traits.Keys)
                {
                    traits[trait] += extraParts[i].traits[trait];
                }
            }
        }

        // Constrain the stats to fit within the desired values
        damage = Mathf.Clamp(damage, DAMAGE_CONSTRAINTS.x, DAMAGE_CONSTRAINTS.y);
        range = Mathf.Clamp(range, RANGE_CONSTRAINTS.x, RANGE_CONSTRAINTS.y);
        radius = Mathf.Clamp(radius, RADIUS_CONSTRAINTS.x, RADIUS_CONSTRAINTS.y);
        useTime = Mathf.Clamp(useTime, USETIME_CONSTRAINTS.x, USETIME_CONSTRAINTS.y);

        // TEMPORARY !!!
        useTime = 0.5f;

        // Constrain the values for traits and flavors as well
        foreach(Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            flavors[flavor] = Mathf.Clamp(flavors[flavor], 0, flavorStatReference[flavor].Length);
        }
        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            traits[trait] = Mathf.Clamp(traits[trait], 0, traitStatReference[trait].Length);
        }

        // Set the primary and secondary actions for this weapon
        primaryAction = GetPrimaryWeaponAction(pPart.action, this);
        secondaryAction = GetSecondaryWeaponAction(sPart.action, this);
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
