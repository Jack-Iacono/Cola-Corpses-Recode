using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapUtil;
using System;

/*
 * 
 * Side Positions:  0,0,0.86602 || 0.75, 0, 0.43301 || 0.75, 0, -0.43301    || 0, 0, 0.086602   || -0.75, 0, -0.43301   || -0.75, 0, 0.43301
 * Side Angles:     0,0,0       || 0,60,0           || 0, 120, 0            || 0, 180, 0        || 0, 240, 0            || 0, 300, 0
 * 
*/

[Serializable]
public class MapPreset: MonoBehaviour
{
    [Tooltip("Make sure that the primary tile is at index 0 in the list or else spawning will not work as intended")]
    [SerializeField]
    private List<Preset_Tile> footprint = new List<Preset_Tile>();

    public GameObject obj;

    public List<Preset_Tile> GetFootprint()
    {
        return footprint;
    }
}
[Serializable]
public class Preset_Tile
{
    public GameObject tileObject;
    public Vector3 gridPosition;
}
