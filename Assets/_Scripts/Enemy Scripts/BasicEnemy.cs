using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EffectDamageable
{
    public static Dictionary<GameObject, BasicEnemy> Instances = new Dictionary<GameObject, BasicEnemy>();

    private PrefabHandler prefabHandler;

    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;

    private bool agentActive = false;
    private bool obstacleActive = false;

    private float moveSpeed = 8;

    private float attackSpeed = 2;
    private Timer attackTimer;
    private bool attackReady = true;

    private float attackRange = 2;

    private Transform target;

    protected override void Awake()
    {
        base.Awake();

        prefabHandler = PrefabHandler.Instance;

        obstacle = GetComponent<NavMeshObstacle>();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;
        agent.speed = moveSpeed;

        Instances.Add(gameObject, this);

        attackTimer = new Timer(null, null, OnAttackReady);
        attackTimer.Start(attackSpeed, 0);

        target = PlayerController.Instance.movementController.transform;
    }

    protected override void Update()
    {
        base.Update();

        if (agent.enabled)
        {
            agent.SetDestination(target.transform.position);
        }

        // Check if the player is in attack range
        if (Vector3.SqrMagnitude(target.position - transform.position) < attackRange * attackRange)
        {
            if (attackReady)
                AttackPlayer();

            if (!obstacleActive)
                SetObstacleActive(true);
        }
        else if (obstacleActive)
            SetObstacleActive(false);
        
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

        if (UnityEngine.Random.Range(0f,1f) > 0.75f)
        {
            GameObject coin = ObjectPool.GetObject(PrefabHandler.Instance.coin);
            CoinController cont = coin.GetComponent<CoinController>();

            coin.transform.position = transform.position;
            cont.Activate();
        }

        base.HealthEmpty();
    }

    protected void AttackPlayer()
    {
        attackReady = false;
        PlayerController.Instance.statusController.ChangeHealth(HurtPlayerMethod);
        attackTimer.Restart();

        float HurtPlayerMethod(float oldHealth)
        {
            return oldHealth - damage;
        }
    }
    protected void OnAttackReady()
    {
        attackReady = true;
    }

    // These methods are used to control what portions of the nav agent is active
    protected void SetAgentActive(bool active)
    {
        agentActive = active;
        CheckNavAgent();
    }
    protected void SetObstacleActive(bool active)
    {
        obstacleActive = active;
        CheckNavAgent();
    }
    protected void CheckNavAgent()
    {
        if (agentActive)
        {
            if (obstacleActive)
            {
                if (agent.isOnNavMesh)
                    agent.isStopped = true;
                agent.enabled = false;
                obstacle.enabled = true;
            }
            else
            {
                obstacle.enabled = false;
                WaitEnable();
            }
        }
        else
        {
            agent.enabled = false;
            obstacle.enabled = false;
        }
    }
    protected async void WaitEnable()
    {
        // Waits for 1 frame to see if the agent is back on the mesh
        await Awaitable.NextFrameAsync();
        agent.enabled = true;
        if (agent.isOnNavMesh)
            agent.isStopped = false;
    }

    private void OnDestroy()
    {
        // this may be redundant, but do it just in case
        if(Instances.ContainsKey(gameObject))
            Instances.Remove(gameObject);
    }
}
