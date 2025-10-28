using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IDamageable;
using static ModifierUtil;
using static UnityEngine.ParticleSystem;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    protected float health = 100;
    protected bool invincible = false;

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
        traitTimers.Add(Trait.SOUR, new Timer(SourTick));
        traitTimers.Add(Trait.SPICY, new Timer(SpicyTick));
    }

    protected virtual void Update()
    {
        // Increment each of the trait timers
        foreach(Trait t in traitTimers.Keys)
        {
            traitTimers[t].Update(Time.deltaTime);
        }
    }

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
    public void ChangeHealth(float change)
    {
        SetHealth(health + change);
    }
    public void SetHealth(float health)
    {
        this.health = health;
        CheckHealth();
    }
    protected void CheckHealth()
    {
        if(health < 0)
            Kill();
    }
    protected void Kill()
    {
        if(!invincible)
            gameObject.SetActive(false);
    }

    // Trait Application Methods
    protected void TraitCheck(Dictionary<Trait, int> traits)
    {
        foreach(Trait t in traits.Keys)
        {
            int level = traits[t];

            // Check if this trait has any levels i.e. if it should be used
            if(level > 0)
            {
                TraitStats stats = ModifierUtil.GetTraitStats(t, level);

                if (ModifierUtil.CheckTraitProc(stats))
                {
                    CreatePopup(t.ToString(), ModifierUtil.traitColorReference[t]);
                    // The - 1 for tick count due to the first tick already happening in the timer, without this would go tickCount + 1 times
                    traitTimers[t].Start(stats.tickSpeed, stats.tickCount - 1);
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
