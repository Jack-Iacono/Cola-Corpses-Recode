using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static IDamageable;
using static ModifierUtil;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyStatusSystem : StatusSystem
{
    private EnemyController eCont;
    private PrefabHandler prefabHandler;

    protected override void Awake()
    {
        base.Awake();

        eCont = GetComponent<EnemyController>();
        prefabHandler = PrefabHandler.Instance;
    }

    protected override void HealthEmpty()
    {
        ResetHealth();
        ResetTraitTimers();

        // Spawns a coin randomly
        if (UnityEngine.Random.Range(0f,1f) > 0.75f)
        {
            GameObject coin = ObjectPool.GetObject(PrefabHandler.Instance.coin);
            CoinController cont = coin.GetComponent<CoinController>();

            coin.transform.position = transform.position;
            cont.Activate();
        }

        base.HealthEmpty();

        // Despawns this enemy
        eCont.Despawn();
    }

    public override DamageableType GetDamageableType()
    {
        return IDamageable.DamageableType.ENEMY;
    }
}
