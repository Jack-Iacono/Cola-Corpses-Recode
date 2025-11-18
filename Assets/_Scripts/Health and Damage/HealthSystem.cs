using UnityEngine;

public abstract class HealthSystem : MonoBehaviour
{
    protected float health = 100;
    protected float maxHealth = 100;

    // Should this EffectDamageable take damage?
    protected bool invincible = false;

    public delegate void OnHealthEmptyDelegate();
    public event OnHealthEmptyDelegate OnHealthEmpty;

    public delegate void OnHealthChangeDelegate(float newHealth);
    public event OnHealthChangeDelegate OnHealthChange;

    public virtual void ResetHealth()
    {
        SetHealth(maxHealth);
    }
    public virtual void ChangeHealth(float change)
    {
        SetHealth(health + change);
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
