using System;
using System.Collections.Generic;
using UnityEngine;
using static IDamageable;
using static ModifierUtil;

public abstract class StatusSystem : HealthSystem
{
    // Holds stats nad timers for buffs
    protected Dictionary<Flavor, Timer> flavorTimers = new Dictionary<Flavor, Timer>();
    protected Dictionary<Flavor, FlavorStats> currentFlavorStats = new Dictionary<Flavor, FlavorStats>();

    // Holds info for debuffs
    protected Dictionary<Trait, Timer> traitTimers = new Dictionary<Trait, Timer>();
    protected Dictionary<Trait, TraitStats> currentTraitStats = new Dictionary<Trait, TraitStats>();

    public delegate void OnFlavorStatusChangedDelegate();
    public event OnFlavorStatusChangedDelegate OnFlavorStatusChanged;

    // Modifier Stuff, will change later
    protected float saltyWeakenModifier = 1;
    protected float bitterWeakenModifier = 1;
    protected float umamiProcMultiplier = 1;

    protected virtual void Awake()
    {
        // Register to be allowed to be damaged
        IDamageable.Register(gameObject, this);

        // Set up the stats for each flavor for when they need to be used
        foreach (Flavor flavor in Enum.GetValues(typeof(Flavor)))
        {
            currentFlavorStats.Add(flavor, ModifierUtil.GetFlavorStat(flavor, 1));
        }
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

        flavorTimers.Add(Flavor.COLA, new Timer(ApplyCola, null, RemoveCola));
        flavorTimers.Add(Flavor.ORANGE, new Timer(ApplyOrange, null, RemoveOrange));
        flavorTimers.Add(Flavor.CHERRY, new Timer(ApplyCherry, null, RemoveCherry));
        flavorTimers.Add(Flavor.GRAPE, new Timer(ApplyGrape, null, RemoveGrape));
        flavorTimers.Add(Flavor.BANANA, new Timer(ApplyBanana, null, RemoveBanana));
        flavorTimers.Add(Flavor.RASPBERRY, new Timer(ApplyRaspberry, null, RemoveRaspberry));
        flavorTimers.Add(Flavor.ROOTBEER, new Timer(null, TickRootbeer, null));
        flavorTimers.Add(Flavor.CREAM, new Timer(null, TickCream, null));
    }

    protected virtual void Update()
    {
        // Increment each of the trait timers
        foreach (Flavor f in flavorTimers.Keys)
        {
            flavorTimers[f].Update(Time.deltaTime);
        }
        OnFlavorStatusChanged?.Invoke();

        // Increment each of the trait timers
        foreach (Trait t in traitTimers.Keys)
        {
            traitTimers[t].Update(Time.deltaTime);
        }
    }

    #region Damage Methods

    // Methods to react to damage from different sources
    public override void DamageSplash(float damage, Dictionary<ModifierUtil.Trait, int> traits = null)
    {
        ApplyDamage(damage, DamageType.SPLASH);
        if (traits != null)
            TraitCheck(traits);
    }
    public override void DamageContact(float damage, Dictionary<ModifierUtil.Trait, int> traits = null)
    {
        ApplyDamage(damage, DamageType.CONTACT);
        if (traits != null)
            TraitCheck(traits);
    }

    // Health related methods
    public void ApplyDamage(float damage, DamageType type = DamageType.NEUTRAL)
    {
        if (!invincible)
            ChangeHealth(DamageHealthChangeMethod);

        switch (type)
        {
            case DamageType.CONTACT:
                CreatePopup(damage.ToString(), Color.white);
                break;
            case DamageType.SPLASH:
                CreatePopup(damage.ToString(), Color.white);
                break;
        }

        // Used to alter the health in a specific manner
        float DamageHealthChangeMethod(float old)
        {
            return old - damage;
        }
    }

    #endregion

    #region Flavor Methods

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
    public Dictionary<Flavor, Timer> GetFlavorTimers()
    {
        return flavorTimers;
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
        ChangeHealth(IncreaseHealth);

        float IncreaseHealth(float old)
        {
            return old + currentFlavorStats[ModifierUtil.Flavor.CREAM].potency;
        }
    }

    #endregion

    #region Trait Methods

    protected virtual void TraitCheck(Dictionary<Trait, int> traits)
    {
        foreach (Trait t in traits.Keys)
        {
            int level = traits[t];

            // Check if this trait has any levels i.e. if it should be used
            if (level > 0)
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

    protected void ResetTraitTimers()
    {
        // Run through each timer in the trait timers dictionary and stop them
        foreach (Timer timer in traitTimers.Values)
        {
            timer.Stop();
        }
    }

    protected void SourTick()
    {
        TraitStats stats = currentTraitStats[Trait.SOUR];
        float d = health * stats.potency;
        CreatePopup(Mathf.FloorToInt(d).ToString(), ModifierUtil.traitColorReference[Trait.SOUR]);
        ApplyDamage(d);
    }
    protected void SpicyTick()
    {
        TraitStats stats = currentTraitStats[Trait.SPICY];
        float d = healthBounds.y * stats.potency;
        CreatePopup(Mathf.FloorToInt(d).ToString(), ModifierUtil.traitColorReference[Trait.SPICY]);
        ApplyDamage(d);
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

    #region Health Methods

    protected override void HealthEmpty()
    {
        if (!invincible)
            gameObject.SetActive(false);
    }

    #endregion

    protected void CreatePopup(string text, Color color)
    {
        GameObject g = ObjectPool.GetObject(PrefabHandler.Instance.damagePopup);
        PopupController p = PopupController.Instances[g];
        p.Activate(transform.position + Vector3.up * 1.5f, text, color);
    }

    
}
