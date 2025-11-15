using System;
using System.Collections.Generic;
using UnityEngine;
using static ModifierUtil;
using static UnityEngine.ParticleSystem;

public class PlayerStatusController : HealthSystem
{
    // Holds stats nad timers for buffs
    protected Dictionary<Flavor, Timer> flavorTimers = new Dictionary<Flavor, Timer>();
    protected Dictionary<Flavor, FlavorStats> currentFlavorStats = new Dictionary<Flavor, FlavorStats>();

    private void Awake()
    {
        // Set up the stats for each flavor for when they need to be used
        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            currentFlavorStats.Add(flavor, ModifierUtil.GetFlavorStat(flavor, 1));
        }

        flavorTimers.Add(Flavor.COLA, new Timer(ApplyCola, null, RemoveCola));
        flavorTimers.Add(Flavor.ORANGE, new Timer(ApplyOrange, null, RemoveOrange));
        flavorTimers.Add(Flavor.CHERRY, new Timer(ApplyCherry, null, RemoveCherry));
        flavorTimers.Add(Flavor.GRAPE, new Timer(ApplyGrape, null, RemoveGrape));
        flavorTimers.Add(Flavor.BANANA, new Timer(ApplyBanana, null, RemoveBanana));
        flavorTimers.Add(Flavor.RASPBERRY, new Timer(ApplyRaspberry, null, RemoveRaspberry));
        flavorTimers.Add(Flavor.ROOTBEER, new Timer(null, TickRootbeer, null));
        flavorTimers.Add(Flavor.CREAM, new Timer(null, TickCream, null));

        // Add in the necessary hurt sounds onto the player
        AudioManager.AddAudioSources(AudioManager.SoundType.p_Hurt, 5, gameObject);
    }

    private void Update()
    {
        // Increment each of the trait timers
        foreach (Flavor f in flavorTimers.Keys)
        {
            flavorTimers[f].Update(Time.deltaTime);
        }
    }

    #region Buff Methods

    public void ApplyBuffs(Dictionary<Flavor, int> flavors)
    {
        foreach (Flavor f in flavors.Keys)
        {
            int level = flavors[f];

            // Check if this trait has any levels i.e. if it should be used
            if (level > 0)
            {
                FlavorStats stats = ModifierUtil.GetFlavorStat(f, level);

                flavorTimers[f].Start(stats.tickSpeed, stats.tickCount - 1, ModifierUtil.flavorTimerOverrideReference[f]);
                currentFlavorStats[f] = stats;
            }
        }
    }

    private void ApplyCherry()
    {

    }
    private void RemoveCherry()
    {

    }

    private void ApplyGrape()
    {

    }
    private void RemoveGrape()
    {

    }

    private void ApplyOrange()
    {

    }
    private void RemoveOrange()
    {

    }

    private void ApplyBanana()
    {

    }
    private void RemoveBanana()
    {

    }

    private void ApplyCola()
    {
        Debug.Log("Apply Cola");
    }
    private void RemoveCola()
    {
        Debug.Log("Remove Cola");
    }

    private void ApplyRaspberry()
    {

    }
    private void RemoveRaspberry()
    {

    }

    private void TickRootbeer()
    {

    }
    private void TickCream()
    {

    }

    #endregion

    public override void ChangeHealth(float change)
    {
        base.ChangeHealth(change);
        if(change < 0)
        {
            // Play the hurt sound
            AudioManager.Play(AudioManager.SoundType.p_Hurt, gameObject);
        }
    }
    protected override void HealthEmpty()
    {
        Debug.Log("Health Empty");
        TriggerHealthEmpty();
    }
}
