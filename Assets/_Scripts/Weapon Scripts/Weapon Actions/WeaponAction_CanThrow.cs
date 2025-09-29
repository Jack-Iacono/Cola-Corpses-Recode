using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAction_CanThrow : WeaponAction
{
    private GameObject canPrefab;

    public WeaponAction_CanThrow(Weapon weapon) : base(weapon)
    {
        canPrefab = PrefabHandler.Instance.thrownCan;
        ObjectPool.PoolObject(canPrefab, 10);
    }

    public override void Update(float dt, ActionState state)
    {
        if(state == ActionState.DOWN)
        {
            GameObject can = ObjectPool.GetObject(canPrefab);
            can.transform.position = Vector3.zero;
            can.SetActive(true);
        }
    }
    public override void Use()
    {
        
    }
}
