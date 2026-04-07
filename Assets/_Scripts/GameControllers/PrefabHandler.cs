using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabHandler : MonoBehaviour
{
    // This class will work as a universal storage for any prefabs that may be needed by any script
    // This allows for reuse of many prefabs across scripts without reassigning them to variables

    public static PrefabHandler Instance;

    [Header("Player / Enemies")]
    public GameObject player;
    public GameObject enemy;

    [Header("Projectiles")]
    public GameObject thrownCan;

    [Header("Items")]
    public GameObject coin;

    [Header("UI Elements")]
    public GameObject damagePopup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        // Initialize the prefabs that willbe used across many scenes
        ObjectPool.PoolObject(damagePopup, 10);
        ObjectPool.PoolObject(coin, 10);
    }

    private void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
