using System.Collections.Generic;
using UnityEngine;

public abstract class HealthSystem : MonoBehaviour, IDamageable
{
    protected float health = 100;
    protected Vector2 healthBounds = new Vector2(0, 100);

    // Should this EffectDamageable take damage?
    protected bool invincible = false;

    public delegate void d_OnHealthEmpty();
    public event d_OnHealthEmpty OnHealthEmpty;

    public delegate void d_OnHealthChange(float newHealth);
    public event d_OnHealthChange OnHealthChange;

    public delegate float d_ChangeHealth(float oldHealth);

    public virtual void ResetHealth()
    {
        SetHealth(healthBounds.y);
    }

    // I'm using a delegate here in case any damage methods want to use some more complex functions later on such as draining a percentage of health
    public virtual void ChangeHealth(d_ChangeHealth change)
    {
        SetHealth(change(health));
        health = Mathf.Clamp(health, healthBounds.x, healthBounds.y);
    }
    public virtual void SetHealth(float health)
    {
        this.health = health;
        CheckHealth();
    }
    protected virtual void CheckHealth()
    {
        if (health < 0)
            HealthEmpty();
    }
    protected abstract void HealthEmpty();
    public float GetHealth()
    {
        return health;
    }

    // Methods related to damaging the entity
    public abstract void DamageContact(float damage, Dictionary<ModifierUtil.Trait, int> traits = null);
    public abstract void DamageSplash(float damage, Dictionary<ModifierUtil.Trait, int> traits = null);

    // These methods are not set to always trigger since enemies do not need to broadcast this necessarily, while the player does
    protected void InvokeOnHealthEmpty()
    {
        OnHealthEmpty?.Invoke();
    }
    protected void InvokeOnHealthChange()
    {
        OnHealthChange?.Invoke(health);
    }

    
}
