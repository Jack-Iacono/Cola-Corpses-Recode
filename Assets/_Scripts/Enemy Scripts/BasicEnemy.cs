using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EffectDamageable
{
    public static Dictionary<GameObject, BasicEnemy> Instances = new Dictionary<GameObject, BasicEnemy>();

    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private bool agentActive = false;

    private float moveSpeed;

    private float attackSpeed = 2;
    private Timer attackTimer;
    private bool attackReady = true;

    private float attackRange = 2;

    private Transform target;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        Instances.Add(gameObject, this);

        attackTimer = new Timer(null, null, OnAttackReady);
        attackTimer.Start(attackSpeed, 0);

        target = PlayerController.movementController.transform;
    }

    protected override void Update()
    {
        base.Update();

        if (agent.enabled)
        {
            agent.SetDestination(PlayerController.movementController.transform.position);
        }

        // Check if the player is in attack range
        if(Vector3.SqrMagnitude(target.position - transform.position) < attackRange * attackRange)
        {
            if (attackReady)
                AttackPlayer();

            agent.enabled = false;
            obstacle.enabled = true;
        }
        else
        {
            agent.enabled = agentActive;
            obstacle.enabled = false;
        }
        

        attackTimer.Update(Time.deltaTime);
    }

    public void Spawn(Vector3 pos)
    {
        // Spawn on ground
        transform.position = pos + Vector3.up;

        SetAgentActive(true);
    }
    protected override void HealthEmpty()
    {
        ResetHealth();
        SetAgentActive(false);
        ResetTraitTimers();

        base.HealthEmpty();
    }

    protected void AttackPlayer()
    {
        attackReady = false;
        PlayerController.statusController.ChangeHealth(-damage);
        attackTimer.Restart();
    }
    protected void OnAttackReady()
    {
        attackReady = true;
    }

    protected void SetAgentActive(bool active)
    {
        agentActive = active;
        agent.enabled = agentActive;
    }

    private void OnDestroy()
    {
        // this may be redundant, but do it just in case
        if(Instances.ContainsKey(gameObject))
            Instances.Remove(gameObject);
    }
}
