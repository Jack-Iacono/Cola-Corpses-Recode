using UnityEngine;

public abstract class SecondaryWeaponAction : WeaponAction
{
    protected PlayerStatusController playerStatusController;

    protected SecondaryWeaponAction(Weapon weapon) : base(weapon)
    {
        playerStatusController = PlayerController.Instance.statusController;
    }
}
