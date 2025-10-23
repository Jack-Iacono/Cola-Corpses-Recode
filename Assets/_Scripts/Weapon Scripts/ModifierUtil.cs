using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierUtil
{
    public enum Flavor
    {
        COLA, ROOTBEER, ORANGE, GRAPE, CHERRY, CREAM, BANANA, RASPBERRY
    }
    public enum Modifier
    {
        SOUR, SWEET, SPICY, SALTY, UMAMI, STICKY, FIZZY, CHILLED, HOT
    }

    // Dictionary to hold all stats for each flavor level
    public static Dictionary<Flavor, FlavorStats[]> flavorStatReference = new Dictionary<Flavor, FlavorStats[]>()
    {
        { Flavor.COLA, new FlavorStats[]
            {
                new FlavorStats(10,1,4)
            } 
        }
    };
    public static Dictionary<Modifier, ModifierStats[]> attributeStatReference = new Dictionary<Modifier, ModifierStats[]>()
    {
        { Modifier.SOUR, new ModifierStats[]
            {
                new ModifierStats(10,1,4, 0.5f)
            }
        }
    };

    public struct ModifierStats
    {
        public float potency;
        public float tickCount;
        public float tickSpeed;

        /// <summary>
        /// This is from 0-1
        /// </summary>
        public float procChance;

        public ModifierStats(float potency, float tickCount, float tickSpeed, float procChance)
        {
            this.potency = potency;
            this.tickCount = tickCount;
            this.tickSpeed = tickSpeed;
            this.procChance = procChance;
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
    }
}
