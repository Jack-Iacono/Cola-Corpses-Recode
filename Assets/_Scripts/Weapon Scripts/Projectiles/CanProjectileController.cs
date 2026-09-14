using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CanProjectileController : ProjectileController
{
    private Rigidbody rb;
    private float radius;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    public void Activate(Vector3 force, float explodeRadius)
    {
        rb.linearVelocity = force;
        // Apply a random rotational force to the can for flair
        rb.angularVelocity = new Vector3
            (
                Random.Range(0,10),
                Random.Range(0, 10),
                Random.Range(0, 10)
            );
        radius = explodeRadius;

        base.Activate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Do damage to the collided object when colliding with it
        if (IDamageable.Instances.ContainsKey(collision.collider.gameObject))
        {
            // Get the thing that was hit
            IDamageable hit = IDamageable.Instances[collision.collider.gameObject];

            // If this thing was a player, do not hit it
            if(hit.GetDamageableType() != IDamageable.DamageableType.PLAYER)
                ImpactCollide(hit);
        }

        // Get every object hit by the area explosion
        Collider[] cols = Physics.OverlapSphere(transform.position, radius);
        List<IDamageable> hits = new List<IDamageable>();
        foreach(Collider col in cols)
        {
            if (IDamageable.Instances.ContainsKey(col.gameObject))
            {
                // Get the thing that was hit
                IDamageable hit = IDamageable.Instances[col.gameObject];

                // If this thing was a player, do not hit it
                if (hit.GetDamageableType() != IDamageable.DamageableType.PLAYER)
                    hits.Add(hit);
            }
        }
        SplashCollide(hits.ToArray());

        // Play the explosion sound
        AudioManager.Play(AudioManager.SoundType.w_CanExplode, transform.position);

        // For now, may add bounces later
        Deactivate();
    }
}
