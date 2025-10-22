using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    protected float health = 100;

    protected virtual void Awake()
    {
        // Register to be allowed to be damaged
        IDamageable.Register(gameObject, this);
    }

    // Methods to react to damage from different sources
    public void DamageArea(float damage)
    {
        ChangeHealth(-damage);

        GameObject g = ObjectPool.GetObject(PrefabHandler.Instance.damagePopup);
        PopupController p = PopupController.Instances[g];
        p.Activate(transform.position + Vector3.up, damage.ToString());
    }
    public void DamageContact(float damage)
    {
        ChangeHealth(-damage);
    }

    // Health related methods
    public void ChangeHealth(float change)
    {
        health += change;
        CheckHealth();
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
        Debug.Log("Killed");
        //gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unregister from the list of damageable objects
        IDamageable.Unregister(gameObject);
    }
}
