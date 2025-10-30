using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EffectDamageable
{
    private NavMeshAgent agent;

    private float moveSpeed;

    Transform target;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        
    }

    protected override void Update()
    {
        base.Update();

        if(PlayerMovementController.Instance != null)
            agent.SetDestination(PlayerMovementController.Instance.transform.position);
    }
}
