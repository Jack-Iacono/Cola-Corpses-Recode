using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public static class ModifierUtil
{
    public enum Flavor
    {
        COLA, ROOTBEER, ORANGE, GRAPE, CHERRY, CREAM, BANANA, RASPBERRY
    }
    public enum Trait
    {
        SOUR, SWEET, SPICY, SALTY, UMAMI, BITTER
    }
    // Planning to add Fizzy, Flat, Hot and Iced later on

    // Dictionary to hold all stats for each flavor level
    private static  readonly Dictionary<Flavor, FlavorStats[]> flavorStatReference = new Dictionary<Flavor, FlavorStats[]>()
    {
        { Flavor.COLA, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            } 
        },
        { Flavor.ROOTBEER, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.ORANGE, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.GRAPE, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.CHERRY, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.CREAM, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.BANANA, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        },
        { Flavor.RASPBERRY, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            }
        }
    };
    private static readonly Dictionary<Trait, TraitStats[]> traitStatReference = new Dictionary<Trait, TraitStats[]>()
    {
        { Trait.SOUR, new TraitStats[]
            {
                new TraitStats(0.3f,10,0.5f, 0.5f)
            }
        },
        { Trait.SWEET, new TraitStats[]
            {
                new TraitStats(10,10,0.5f, 0.5f)
            }
        },
        { Trait.SPICY, new TraitStats[]
            {
                new TraitStats(0.1f,15,0.3f, 0.5f)
            }
        },
        { Trait.SALTY, new TraitStats[]
            {
                new TraitStats(10,1,10, 0.5f)
            }
        },
        { Trait.BITTER, new TraitStats[]
            {
                new TraitStats(10,1,15, 0.5f)
            }
        },
        { Trait.UMAMI, new TraitStats[]
            {
                new TraitStats(10,1, 20, 0.5f)
            }
        }
    };

    // A reference to hold the color associated with each of the traits for ui purposes and because I like it
    public static readonly Dictionary<Trait, Color> traitColorReference = new Dictionary<Trait, Color>() 
    { 
        { Trait.SOUR, Color.green },
        { Trait.SWEET, Color.magenta },
        { Trait.SPICY, Color.red },
        { Trait.SALTY, Color.grey },
        { Trait.UMAMI, Color.blue },
        { Trait.BITTER, Color.yellow }
    };

    // Used to decide what action timers should take with each trait's debuff timers
    // Some buffs/debuffs benefit from having the timer reset itself, while others benefit from running the current tick and extending
    public static readonly Dictionary<Trait, bool> traitTimerOverrideReference = new Dictionary<Trait, bool>()
    {
        { Trait.SPICY, false },
        { Trait.SOUR, false },
        { Trait.SWEET, false },
        { Trait.SALTY, true },
        { Trait.BITTER, true },
        { Trait.UMAMI, true },
    };
    public static readonly Dictionary<Flavor, bool> flavorTimerOverrideReference = new Dictionary<Flavor, bool>()
    {
        { Flavor.COLA, false },
        { Flavor.ROOTBEER, false },
        { Flavor.ORANGE, false },
        { Flavor.GRAPE, false },
        { Flavor.CHERRY, false },
        { Flavor.CREAM, false },
        { Flavor.BANANA, false },
        { Flavor.RASPBERRY, false },
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

    // Use these to check whether a trait should proc or not
    public static bool CheckTraitProc(TraitStats stats)
    {
        // Making this into a method in case proc chance needs to be more precise later
        return CheckTraitProc(stats.procChance);
    }
    public static bool CheckTraitProc(float chance)
    {
        return Random.Range(0, 1f) < chance;
    }

    public struct TraitStats
    {
        public float potency;
        public int tickCount;
        public float tickSpeed;

        /// <summary>
        /// This is from 0-1
        /// </summary>
        public float procChance;

        public TraitStats(float potency, int tickCount, float tickSpeed, float procChance)
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
        public int tickCount;
        public float tickSpeed;

        public FlavorStats(float potency, int tickCount, float tickSpeed)
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
