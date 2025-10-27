using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierUtil
{
    public enum Flavor
    {
        COLA, ROOTBEER, ORANGE, GRAPE, CHERRY, CREAM, BANANA, RASPBERRY
    }
    public enum Trait
    {
        SOUR, SWEET, SPICY, SALTY, UMAMI, STICKY, FIZZY, CHILLED, HOT
    }

    // Dictionary to hold all stats for each flavor level
    private static  readonly Dictionary<Flavor, FlavorStats[]> flavorStatReference = new Dictionary<Flavor, FlavorStats[]>()
    {
        { Flavor.COLA, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            } 
        }
    };
    private static readonly Dictionary<Trait, TraitStats[]> traitStatReference = new Dictionary<Trait, TraitStats[]>()
    {
        { Trait.SOUR, new TraitStats[]
            {
                new TraitStats(10,1,4, 0.5f)
            }
        }
    };

    /// <summary>
    /// Gets the flavor's stats for the given flavor at the given level
    /// </summary>
    /// <param name="flavor">The flavor whose stats you want to get</param>
    /// <param name="level">The level of the flavor</param>
    /// <returns>The flavor stats for the given flavor</returns>
    public static FlavorStats GetFlavorStat(Flavor flavor, int level)
    {
        // Subtract 1 from the level since the stat reference uses indices instead of starting at 1
        return flavorStatReference[flavor][level - 1];
    }
    /// <summary>
    /// Gets the trait's stats for the given trait at the given level
    /// </summary>
    /// <param name="trait">The trait whose stats you want to get</param>
    /// <param name="level">The level of the trait</param>
    /// <returns>The flavor stats for the given trait</returns>
    public static TraitStats GetTraitStats(Trait trait, int level)
    {
        // Subtract 1 from the level since the stat reference uses indices instead of starting at 1
        return traitStatReference[trait][level - 1];
    }

    public struct TraitStats
    {
        public float potency;
        public float tickCount;
        public float tickSpeed;

        /// <summary>
        /// This is from 0-1
        /// </summary>
        public float procChance;

        public TraitStats(float potency, float tickCount, float tickSpeed, float procChance)
        {
            this.potency = potency;
            this.tickCount = tickCount;
            this.tickSpeed = tickSpeed;
            this.procChance = procChance;
        }

        public override string ToString()
        {
            return "Potency: " + potency.ToString() + "\nTick Count: " + tickCount.ToString() + "\nTick Speed: " + tickSpeed.ToString() + "\nProc Chance: " + procChance.ToString();
        }
    }
    public struct  FlavorStats
    {
        public float potency;
        public float tickCount;
        public float tickSpeed;

        public FlavorStats(float potency, float tickCount, float tickSpeed)
        {
            this.potency = potency;
            this.tickCount = tickCount;
            this.tickSpeed = tickSpeed;
        }

        public override string ToString()
        {
            return "Potency: " + potency.ToString() + "\nTick Count: " + tickCount.ToString() + "\nTick Speed: " + tickSpeed.ToString();
        }
    }
}
