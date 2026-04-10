using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static ModifierUtil;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemyMovementController : MovementSystem
{
    private EnemyController eCont;

    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;

    private bool agentActive = false;
    private bool obstacleActive = false;

    protected Transform target;

    private void Awake()
    {
        eCont = GetComponent<EnemyController>();
        eCont.OnSpawn += OnSpawn;
        eCont.OnDespawn += OnDespawn;

        obstacle = GetComponent<NavMeshObstacle>();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 2;
        agent.speed = moveSpeed;

        target = PlayerController.Instance.movementController.transform;
    }

    protected void Update()
    {
        if (agent.enabled)
        {
            agent.SetDestination(target.transform.position);
        }

        // Check if the player is in attack range
        if (Vector3.SqrMagnitude(target.position - transform.position) < agent.stoppingDistance * agent.stoppingDistance)
        {
            if (!obstacleActive)
                SetObstacleActive(true);
        }
        else if (obstacleActive)
            SetObstacleActive(false);
    }

    public void OnSpawn()
    {
        SetAgentActive(true);
    }
    public void OnDespawn()
    {
        SetAgentActive(false);
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
}
