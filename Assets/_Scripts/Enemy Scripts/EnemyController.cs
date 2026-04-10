using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    public static Dictionary<GameObject, EnemyController> Instances = new Dictionary<GameObject, EnemyController>();

    public static MovementSystem movementController;
    public static StatusSystem statusController;
    public static EnemyAttackSystem attackController;

    public delegate void d_EnemySpawnEvent();
    public event d_EnemySpawnEvent OnSpawn;
    public event d_EnemySpawnEvent OnDespawn;

    private void Awake()
    {
        Instances.Add(gameObject, this);

        movementController = GetComponent<MovementSystem>();
        statusController = GetComponent<StatusSystem>();
        attackController = GetComponent<EnemyAttackSystem>();
    }

    public void Spawn(Vector3 pos)
    {
        // Spawn on ground
        transform.position = pos + Vector3.up;
        gameObject.SetActive(true);
        OnSpawn?.Invoke();
    }
    public void Despawn()
    {
        gameObject.SetActive(false);
        OnDespawn?.Invoke();
    }

    private void OnDestroy()
    {
        Instances.Remove(gameObject);
    }
}
