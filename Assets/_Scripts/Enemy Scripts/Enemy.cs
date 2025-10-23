using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IDamageable;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    protected float health = 100;
    protected bool invincible = false;

    protected virtual void Awake()
    {
        // Register to be allowed to be damaged
        IDamageable.Register(gameObject, this);
    }

    // Methods to react to damage from different sources
    public void DamageArea(float damage)
    {
        ApplyDamage(damage);
    }
    public void DamageContact(float damage)
    {
        ApplyDamage(damage);
    }

    // Health related methods
    public void ApplyDamage(float damage, DamageType type = DamageType.NEUTRAL)
    {
        if (!invincible)
            ChangeHealth(-damage);

        GameObject g = ObjectPool.GetObject(PrefabHandler.Instance.damagePopup);
        PopupController p = PopupController.Instances[g];
        p.Activate(transform.position + Vector3.up, damage.ToString());
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

    private void OnDestroy()
    {
        // Unregister from the list of damageable objects
        IDamageable.Unregister(gameObject);
    }
}
