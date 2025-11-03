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

        spawnTimer = new Timer(null, SpawnEnemy, null);
        spawnTimer.Start(spawnTime, -1);

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
            spawnTimer.Update(Time.deltaTime);
    }

    public void SpawnEnemy()
    {
        GameObject enemy = ObjectPool.GetObject(PrefabHandler.Instance.enemy);
        BasicEnemy cont = BasicEnemy.Instances[enemy];

        cont.Spawn(spawnLocation);
        enemy.SetActive(true);
    }
}
