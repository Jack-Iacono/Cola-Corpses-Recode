using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction_CanThrow : PrimaryWeaponAction
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

    public override void Update(float dt)
    {
        // Increment the 
        if (useTimer > 0)
            useTimer -= dt;
    }
    public override void Use(ActionState state)
    {
        // Check if this is ready to use
        if(state != ActionState.UP && useTimer <= 0)
        {
            // Get the can gameobject as well as the associated script
            GameObject can = ObjectPool.GetObject(canPrefab);
            CanProjectileController projectile = (CanProjectileController)ProjectileController.ProjectileInstances[can];

            // Ready the can to be thrown
            Vector3 camSightVec = cameraController.GetCameraSightVector();
            can.transform.position = player.transform.position + Vector3.up * 0.25f + camSightVec;
            projectile.Activate(camSightVec * weapon.range, weapon.radius);

            // Register projectile events for callbacks
            RegisterProjectile(projectile);

            // Reset the use speed timer
            useTimer = useTime;
        }
    }

    public override void OtherUse()
    {
        // Do Nothing for now
    }
}
