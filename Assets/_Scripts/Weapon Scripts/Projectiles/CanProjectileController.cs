using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CanProjectileController : ProjectileController
{
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    public void Activate(Weapon weaponSource, Vector3 force)
    {
        rb.velocity = force;
        // Apply a random rotational force to the can for flair
        rb.angularVelocity = new Vector3
            (
                Random.Range(0,10),
                Random.Range(0, 10),
                Random.Range(0, 10)
            );

        base.Activate(weaponSource);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Do damage to the collided object when colliding with it
        if (IDamageable.Instances.ContainsKey(collision.collider.gameObject))
        {
            IDamageable.Instances[collision.collider.gameObject].DamageContact(weapon.damage);
        }

        Collider[] cols = Physics.OverlapSphere(transform.position, weapon.radius);
        foreach(Collider col in cols)
        {
            if (IDamageable.Instances.ContainsKey(col.gameObject))
            {
                IDamageable.Instances[col.gameObject].DamageArea(weapon.damage);
            }
        }
        Deactivate();
    }
}
