using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapUtil;
using System;

public class GameController : MonoBehaviour
{
    private Map spawnedMap = null;

    public static bool isPaused = false;
    public static event EventHandler<bool> OnPlayerAliveChanged;

    // Used for quick shifting into test mode
    public static readonly bool testMode = false;

    private void Awake()
    {
        SceneController.OnMapLoaded += OnMapLoaded;
    }

    private void Initialize()
    {
        if (!testMode)
        {
            spawnedMap = MapBuilder.Instance.GetNewMap();
            MapBuilder.Instance.BuildMap(spawnedMap);
            PlayerController.movementController.Warp(spawnedMap.originTile + Vector3.up);
        }
        else
            PlayerController.movementController.Warp(Vector3.up);

        // Use this to initialize any objects that need to be pooled after map loading
        ObjectPool.PoolObject(PrefabHandler.Instance.damagePopup, 10);
        ObjectPool.PoolObject(PrefabHandler.Instance.enemy, 10);
    }

    private void OnMapLoaded(string mapName)
    {
        Initialize();
    }

    private void OnDestroy()
    {
        SceneController.OnMapLoaded -= OnMapLoaded;
    }
}
