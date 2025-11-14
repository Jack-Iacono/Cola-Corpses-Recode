using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapUtil;
using System;

public class GameController : MonoBehaviour
{
    private Map spawnedMap = null;

    public static bool isPaused = false;

    private void Awake()
    {
        SceneController.OnMapLoaded += OnMapLoaded;
    }

    private void Initialize()
    {
        // Check to see if the game scene is loaded
        if(SceneController.GetMapScene() == SceneController.m_Scene.GAME)
        {
            spawnedMap = MapBuilder.Instance.GetNewMap();
            MapBuilder.Instance.BuildMap(spawnedMap);
            PlayerController.Instance.movementController.Warp(spawnedMap.originTile + Vector3.up);
        }
        else
            PlayerController.Instance.movementController.Warp(Vector3.up);

        // Subsribe to the event that triggers when the player dies
        PlayerController.Instance.statusController.ResetHealth();
        PlayerController.Instance.statusController.OnHealthEmpty += OnPlayerHealthEmpty;

        // Use this to initialize any objects that need to be pooled ONLY DURING THE GAME
        ObjectPool.PoolObject(PrefabHandler.Instance.enemy, 10);
    }

    private void OnPlayerHealthEmpty()
    {
        // Sends the player back to the lobby
        SceneController.LoadLobbyScene();
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
