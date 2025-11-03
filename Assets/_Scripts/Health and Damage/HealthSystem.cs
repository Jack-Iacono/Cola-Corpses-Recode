using UnityEngine;

public abstract class HealthSystem : MonoBehaviour
{
    protected float health = 100;
    protected float maxHealth = 100;

    // Should this EffectDamageable take damage?
    protected bool invincible = false;

    public void ResetHealth()
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
}
