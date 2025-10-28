using UnityEngine;

public abstract class PrimaryWeaponAction : WeaponAction
{
    protected PrimaryWeaponAction(Weapon weapon, PlayerController player) : base(weapon, player)
    {

    }

    protected virtual void ProjectileImpact(IDamageable hit) 
    {
        // Apply impact damage to the hit object
        hit.DamageContact(weapon.damage, weapon.traits);
    }
    protected virtual void ProjectileSplash(IDamageable[] hits)
    {
        // Apply splash damage to all objects hit by this explosion
        foreach (IDamageable hit in hits)
        {
            hit.DamageSplash(weapon.damage, weapon.traits);
        }
    }
    protected virtual void ProjectileExpire(ProjectileController sender)
    {
        // Unregister from the projectile events to prevent further calling
        UnregisterProjectile(sender);
    }

    protected void RegisterProjectile(ProjectileController cont)
    {
        cont.OnImpactCollide += ProjectileImpact;
        cont.OnSplashCollide += ProjectileSplash;
        cont.OnExpire += ProjectileExpire;
    }
    protected void UnregisterProjectile(ProjectileController cont)
    {
        cont.OnImpactCollide -= ProjectileImpact;
        cont.OnSplashCollide -= ProjectileSplash;
        cont.OnExpire -= ProjectileExpire;
    }
}
