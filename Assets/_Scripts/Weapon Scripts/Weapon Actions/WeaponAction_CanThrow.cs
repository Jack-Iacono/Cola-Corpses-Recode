using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction_CanThrow : WeaponAction
{
    private GameObject canPrefab;
    private CameraController cameraController;

    private float useTime = 0f;
    private float useTimer = 0f;

    public WeaponAction_CanThrow(Weapon weapon, PlayerController player) : base(weapon, player)
    {
        canPrefab = PrefabHandler.Instance.thrownCan;
        ObjectPool.PoolObject(canPrefab, 10);
        cameraController = player.GetCameraController();

        useTime = weapon.useTime;
    }

    public override void Update(float dt, ActionState state)
    {
        if(state == ActionState.HELD)
        {
            if(useTimer <= 0)
            {
                Use();
                useTimer = useTime;
            }
        }

        if (useTimer > 0)
            useTimer -= dt;
    }
    public override void Use()
    {
        // Get the can gameobject as well as the associated script
        GameObject can = ObjectPool.GetObject(canPrefab);
        CanProjectileController projectile = (CanProjectileController)ProjectileController.ProjectileInstances[can];

        // Ready the can to be thrown
        Vector3 camSightVec = cameraController.GetCameraSightVector();
        can.transform.position = player.transform.position + camSightVec;
        projectile.Activate(camSightVec * 10);
    }
}
