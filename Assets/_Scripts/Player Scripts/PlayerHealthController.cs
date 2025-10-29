using System;
using System.Collections.Generic;
using UnityEngine;
using static ModifierUtil;

public class PlayerHealthController : HealthSystem
{
    public static PlayerHealthController Instance;

    // Holds stats nad timers for buffs
    protected Dictionary<Flavor, Timer> flavorTimers = new Dictionary<Flavor, Timer>();
    protected Dictionary<Flavor, FlavorStats> currentFlavorStats = new Dictionary<Flavor, FlavorStats>();

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
        {
            Instance = this;

            // Set up the stats for each flavor for when they need to be used
            foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
            {
                currentFlavorStats.Add(flavor, ModifierUtil.GetFlavorStat(flavor, 1));
            }
        }
    }

    public void ApplyCherry()
    {

    }
    public void ApplyGrape()
    {

    }
    public void ApplyOrange()
    {

    }
    public void ApplyBanana()
    {

    }
    public void ApplyCola()
    {

    }
    public void ApplyRootbeer()
    {

    }
    public void ApplyCream()
    {

    }
    public void ApplyRaspberry()
    {

    }

    protected override void HealthEmpty()
    {
        Debug.Log("Player Die");
    }
}
