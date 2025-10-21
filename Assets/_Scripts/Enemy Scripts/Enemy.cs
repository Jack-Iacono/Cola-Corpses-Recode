using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    protected float health;

    protected virtual void Awake()
    {
        IDamageable.Register(gameObject, this);
    }

    public void DamageArea(float damage)
    {
        Debug.Log("Area: " + damage);
    }

    public void DamageContact(float damage)
    {
        Debug.Log("Contact: " + damage);
    }

    private void OnDestroy()
    {
        IDamageable.Unregister(gameObject);
    }
}
