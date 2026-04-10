using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private Vector3 spawnLocation = -Vector3.one;

    private float spawnTime = 5;
    private Timer spawnTimer;

    // This is used to ensure that spawners work in testing modes
    [SerializeField]
    private bool autoInitialize = false;

    private bool initialized = false;
    private bool open = false;

    private void Awake()
    {
        if (autoInitialize)
            Initialize();
    }

    public void Initialize()
    {
        // Find the closest point on the nav mesh to spawn enemies
        NavMeshHit hit;
        int areaMask = (1 << NavMesh.GetAreaFromName("Walkable"));
        NavMesh.SamplePosition(transform.position, out hit, 2, areaMask);
        spawnLocation = hit.position;

        initialized = true;

        open = false;
    }
    public void SetOpen(bool open)
    {
        if (open)
        {
            spawnTimer = new Timer(null, SpawnEnemy, null);
            spawnTimer.Start(spawnTime, -1);
        }
        else
        {
            spawnTimer = null;
        }

        this.open = open;
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized && open)
            spawnTimer.Update(Time.deltaTime);
    }

    public void SpawnEnemy()
    {
        GameObject enemy = ObjectPool.GetObject(PrefabHandler.Instance.enemy);
        EnemyController cont = EnemyController.Instances[enemy];

        cont.Spawn(spawnLocation);
    }
}
