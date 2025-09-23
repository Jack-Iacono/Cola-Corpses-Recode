using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapUtil;

public class GameController : MonoBehaviour
{
    private Map spawnedMap = null;

    // Start is called before the first frame update
    void Start()
    {
        spawnedMap = MapBuilder.Instance.GetNewMap();
        MapBuilder.Instance.BuildMap(spawnedMap);
    }
}
