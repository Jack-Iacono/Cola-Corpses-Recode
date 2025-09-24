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
    private GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        spawnedMap = MapBuilder.Instance.GetNewMap();
        MapBuilder.Instance.BuildMap(spawnedMap);

        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<PlayerController>().Warp(spawnedMap.originTile + Vector3.up);
    }
}
