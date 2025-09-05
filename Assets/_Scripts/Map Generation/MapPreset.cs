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

    public Dictionary<Vector3, GameObject> tileLinks = new Dictionary<Vector3, GameObject>();

    public GameObject obj;

    public List<Vector3> entryPoints = new List<Vector3>();

    public List<Preset_Tile> GetFootprint()
    {
        return footprint;
    }

    private void OnValidate()
    {
        tileLinks = new Dictionary<Vector3, GameObject>();
        for(int i = 0; i < footprint.Count; i++)
        {
            tileLinks.Add(footprint[i].gridPosition, footprint[i].tileObject);
        }
    }
}
[Serializable]
public class Preset_Tile
{
    public GameObject tileObject;
    public Vector3 gridPosition;
}
