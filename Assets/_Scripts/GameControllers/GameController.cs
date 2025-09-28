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

    [SerializeField]
    private bool testMode = false;

    private void Awake()
    {
        SceneController.OnMapLoaded += OnMapLoaded;
    }

    private void Initialize()
    {
        GameObject player = Instantiate(PrefabHandler.Instance.player);

        if (!testMode)
        {
            spawnedMap = MapBuilder.Instance.GetNewMap();
            MapBuilder.Instance.BuildMap(spawnedMap);
            player.GetComponent<PlayerController>().Warp(spawnedMap.originTile + Vector3.up);
        }
        else
            player.GetComponent<PlayerController>().Warp(Vector3.up);
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
