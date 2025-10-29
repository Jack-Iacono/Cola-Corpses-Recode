using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static IDamageable;
using static ModifierUtil;
using static UnityEngine.ParticleSystem;

public abstract class EffectDamageable : HealthSystem, IDamageable
{
    // The damage that this EffectDamagable can do
    protected float damage = 10;

    [SerializeField]
    protected float saltyWeakenModifier = 1;
    [SerializeField]
    protected float bitterWeakenModifier = 1;
    [SerializeField]
    protected float umamiProcMultiplier = 1;

    // Holds info for debuffs
    protected Dictionary<Trait, Timer> traitTimers = new Dictionary<Trait, Timer>();
    protected Dictionary<Trait, TraitStats> currentTraitStats = new Dictionary<Trait, TraitStats>();

    protected virtual void Awake()
    {
        // Register to be allowed to be damaged
        IDamageable.Register(gameObject, this);

        // Initialize each trait damage store with the lowest trait effect for each
        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            currentTraitStats.Add(trait, ModifierUtil.GetTraitStats(trait, 1));
        }

        // Initialize the trait timer dict for use later
        traitTimers.Add(Trait.SOUR, new Timer(null, SourTick, null));
        traitTimers.Add(Trait.SPICY, new Timer(null, SpicyTick, null));
        traitTimers.Add(Trait.SWEET, new Timer(SweetProc, null, null));
        traitTimers.Add(Trait.SALTY, new Timer(SaltyApply, null, SaltyRemove));
        traitTimers.Add(Trait.BITTER, new Timer(BitterApply, null, BitterRemove));
        traitTimers.Add(Trait.UMAMI, new Timer(UmamiApply, null, UmamiRemove));
    }

    protected virtual void Update()
    {
        // Increment each of the trait timers
        foreach(Trait t in traitTimers.Keys)
        {
            traitTimers[t].Update(Time.deltaTime);
        }
    }

    #region Damage Methods

    // Methods to react to damage from different sources
    public void DamageSplash(float damage, Dictionary<ModifierUtil.Trait, int> traits = null)
    {
        ApplyDamage(damage, DamageType.SPLASH);
        if(traits != null)
            TraitCheck(traits);
    }
    public void DamageContact(float damage, Dictionary<ModifierUtil.Trait, int> traits = null)
    {
        ApplyDamage(damage, DamageType.CONTACT);
        if(traits != null)
            TraitCheck(traits);
    }

    // Health related methods
    public void ApplyDamage(float damage, DamageType type = DamageType.NEUTRAL)
    {
        if (!invincible)
            ChangeHealth(-damage);

        CreatePopup(damage.ToString(), Color.white);
    }

    protected override void HealthEmpty()
    {
        if(!invincible)
            gameObject.SetActive(false);
    }

    #endregion

    #region Trait Methods

    protected void TraitCheck(Dictionary<Trait, int> traits)
    {
        foreach(Trait t in traits.Keys)
        {
            int level = traits[t];

            // Check if this trait has any levels i.e. if it should be used
            if(level > 0)
            {
                TraitStats stats = ModifierUtil.GetTraitStats(t, level);

                if (ModifierUtil.CheckTraitProc(stats.procChance * umamiProcMultiplier))
                {
                    CreatePopup(t.ToString(), ModifierUtil.traitColorReference[t]);
                    // The - 1 for tick count due to the first tick already happening in the timer, without this would go tickCount + 1 times
                    traitTimers[t].Start(stats.tickSpeed, stats.tickCount - 1, ModifierUtil.traitTimerOverrideReference[t]);
                    currentTraitStats[t] = stats;
                } 
            }
        }
    }

    protected void SourTick()
    {
        TraitStats stats = currentTraitStats[Trait.SOUR];
        CreatePopup(stats.potency.ToString(), ModifierUtil.traitColorReference[Trait.SOUR]);
    }
    protected void SpicyTick()
    {
        TraitStats stats = currentTraitStats[Trait.SPICY];
        CreatePopup(stats.potency.ToString(), ModifierUtil.traitColorReference[Trait.SPICY]);
    }

    protected void SweetProc()
    {
        // Heal the player
    }

    protected void SaltyApply()
    {
        // Yes, I know I'm gonna have to adjust this later to account for other sources. I'm just lazy right now
        TraitStats stats = currentTraitStats[Trait.SALTY];
        saltyWeakenModifier = stats.potency;
    }
    protected void SaltyRemove()
    {
        saltyWeakenModifier = 1;
        CreatePopup("Unsalted", ModifierUtil.traitColorReference[Trait.SALTY]);
    }

    protected void BitterApply()
    {
        // Yes, I know I'm gonna have to adjust this later to account for other sources. I'm just lazy right now
        TraitStats stats = currentTraitStats[Trait.BITTER];
        bitterWeakenModifier = stats.potency;
    }
    protected void BitterRemove()
    {
        bitterWeakenModifier = 1;
        CreatePopup("Sweetened", ModifierUtil.traitColorReference[Trait.BITTER]);
    }

    protected void UmamiApply()
    {
        // Yes, I know I'm gonna have to adjust this later to account for other sources. I'm just lazy right now
        TraitStats stats = currentTraitStats[Trait.UMAMI];
        umamiProcMultiplier = stats.potency;
    }
    protected void UmamiRemove()
    {
        umamiProcMultiplier = 1;
        CreatePopup("Un-Umamied??", ModifierUtil.traitColorReference[Trait.UMAMI]);
    }

    #endregion

    protected void CreatePopup(string text, Color color)
    {
        GameObject g = ObjectPool.GetObject(PrefabHandler.Instance.damagePopup);
        PopupController p = PopupController.Instances[g];
        p.Activate(transform.position + Vector3.up * 1.5f, text, color);
    }

    private void OnDestroy()
    {
        // Unregister from the list of damageable objects
        IDamageable.Unregister(gameObject);
    }
}
