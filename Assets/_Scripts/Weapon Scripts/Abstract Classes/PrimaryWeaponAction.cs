using UnityEngine;

public abstract class PrimaryWeaponAction : WeaponAction
{
    protected PrimaryWeaponAction(Weapon weapon, PlayerController player) : base(weapon, player)
    {

    }

    protected virtual void ProjectileImpact(IDamageable hit) { }
    protected virtual void ProjectileSplash(IDamageable[] hits) { }
    protected virtual void ProjectileExpire(ProjectileController sender) { }
}
