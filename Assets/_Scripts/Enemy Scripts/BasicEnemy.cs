using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EffectDamageable
{
    public static Dictionary<GameObject, BasicEnemy> Instances = new Dictionary<GameObject, BasicEnemy>();

    private NavMeshAgent agent;

    private float moveSpeed;

    private Transform target;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        Instances.Add(gameObject, this);
    }

    private void Start()
    {
        
    }

    protected override void Update()
    {
        base.Update();

        if(agent.enabled)
            agent.SetDestination(PlayerMovementController.Instance.transform.position);
    }

    public void Spawn(Vector3 pos)
    {
        // Spawn on ground
        Debug.Log("Spawn At");
        transform.position = pos + Vector3.up;
        agent.enabled = true;
    }
    protected override void HealthEmpty()
    {
        ResetHealth();
        agent.enabled = false;
        ResetTraitTimers();

        base.HealthEmpty();
    }

    private void OnDestroy()
    {
        // this may be redundant, but do it just in case
        if(Instances.ContainsKey(gameObject))
            Instances.Remove(gameObject);
    }
}
