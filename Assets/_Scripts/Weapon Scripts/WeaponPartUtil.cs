using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static ModifierUtil;
using static WeaponUtil;

namespace WeaponPartUtil
{
    public class WeaponPart
    {
        public float damage = 0;
        public float radius = 0;
        public float range = 0;
        public float useTime = 0;

        public Dictionary<Flavor, int> flavors = new Dictionary<Flavor, int>();
        public Dictionary<Trait, int> traits = new Dictionary<Trait, int>();

        private int m_flavorPoints = 3;
        private int m_traitPoints = 3;

        public WeaponPart()
        {
            // Give this part random stats
            damage = UnityEngine.Random.Range((int)DAMAGE_CONSTRAINTS.x, (int)DAMAGE_CONSTRAINTS.y + 1);
            radius = UnityEngine.Random.Range((int)RADIUS_CONSTRAINTS.x, (int)RADIUS_CONSTRAINTS.y + 1);
            range = UnityEngine.Random.Range((int)RANGE_CONSTRAINTS.x, (int)RANGE_CONSTRAINTS.y + 1);
            useTime = UnityEngine.Random.Range((int)USETIME_CONSTRAINTS.x, (int)USETIME_CONSTRAINTS.y + 1);

            // These will be referenced throughout the process of assigning flavors/traits, saves processing power
            List<Flavor> fList = new List<Flavor>((Flavor[])Enum.GetValues(typeof(Flavor)));
            List<Trait> tList = new List<Trait>((Trait[])Enum.GetValues(typeof(Trait)));

            // Add random flavors and traits to this part
            for (int i = 0; i < m_flavorPoints; i++)
            {
                Flavor randFlavor = fList[UnityEngine.Random.Range(0, fList.Count)];

                // Add if key has not been added before
                if (!flavors.ContainsKey(randFlavor))
                    flavors.Add(randFlavor, 1);
                else
                    flavors[randFlavor]++;

                // If this flavor has max stats, remove it from consideration for future point distribution
                if (flavors[randFlavor] == 4)
                    fList.Remove(randFlavor);
            }
            for (int i = 0; i < m_traitPoints; i++)
            {
                Trait randTrait = tList[UnityEngine.Random.Range(0, tList.Count)];

                // Add if key has not been added before
                if (!traits.ContainsKey(randTrait))
                    traits.Add(randTrait, 1);
                else
                    traits[randTrait]++;

                // If this trait has max stats, remove it from consideration for future point distribution
                if (traits[randTrait] == 4)
                    tList.Remove(randTrait);
            }

            Debug.Log(this);
        }
        public WeaponPart(float damage, float radius, float range, float useTime, Dictionary<Flavor, int> flavors, Dictionary<Trait, int> traits)
        {
            this.damage = damage;
            this.radius = radius;
            this.range = range;
            this.useTime = useTime;
            this.flavors = flavors;
            this.traits = traits;
        }

        public override string ToString()
        {
            // Set up a string with the basic stats
            string text = $"Damage: {damage}\nRadius: {radius}\nRange: {range}\nUse Time: {useTime}\n";

            // Add each flavor
            foreach (Flavor flavor in flavors.Keys)
            {
                text += $"F-{flavor.ToString()}: {flavors[flavor]}\n";
            }

            // Add each trait
            foreach (Trait trait in traits.Keys)
            {
                text += $"T-{trait.ToString()}: {traits[trait]}\n";
            }

            return text;
        }
    }

    public class WeaponPartPrimary : WeaponPart
    {
        public pWeaponAction action;

        public WeaponPartPrimary() : base()
        {
            action = WeaponUtil.GetRandomPrimaryActionType();
        }
        public WeaponPartPrimary(pWeaponAction action, float damage, float radius, float range, float useTime, Dictionary<Flavor, int> flavors, Dictionary<Trait, int> traits) : base(damage, radius, range, useTime, flavors, traits)
        {
            this.action = action;
        }
    }

    public class WeaponPartSecondary : WeaponPart
    {
        public sWeaponAction action;

        public WeaponPartSecondary() : base()
        {
            action = WeaponUtil.GetRandomSecondaryActionType();
        }
        public WeaponPartSecondary(sWeaponAction action, float damage, float radius, float range, float useTime, Dictionary<Flavor, int> flavors, Dictionary<Trait, int> traits) : base(damage, radius, range, useTime, flavors, traits)
        {
            this.action = action;
        }
    }
}