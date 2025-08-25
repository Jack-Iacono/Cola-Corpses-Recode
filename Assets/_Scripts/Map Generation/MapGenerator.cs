using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System.Drawing;
using System.Net;

public class MapGenerator : MonoBehaviour
{
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;

    private Vector3 mapDim = new Vector3(100, 3, 100);
    private Vector2 roomTileRange = new Vector2(500, 1000);

    private float tileRadius = 1f;
    private float floorHeight = 2;

    public Map currentMap;

    // Start is called before the first frame update
    void Start()
    {
        currentMap = new Map(mapDim, tileRadius, floorHeight, 3, roomTileRange);
        currentMap.GenerateMap();
        BuildMap(currentMap);
    }

    private void BuildMap(Map map)
    {
        float tileRadius = map.tileRadius;
        float tileSideDistance = map.tileSideDistance;

        GameObject mapParent = new GameObject("Map");

        List<Room> rooms = map.GetRooms();
        for( int i = 0; i < rooms.Count; i++)
        {
            GameObject roomParent = new GameObject("Room " + i);
            roomParent.transform.parent = mapParent.transform;

            UnityEngine.Color roomColor = Random.ColorHSV();

            List<Tile> tiles = rooms[i].GetTiles();
            for(int j = 0; j < tiles.Count; j++)
            {
                Tile tile = tiles[j];
                GameObject tileObject = Instantiate(floorPrefabNormal);
                tileObject.transform.parent = roomParent.transform;
                Vector3 tileGridPosition = tile.gridPosition;

                Vector3 tilePosition = new Vector3
                (
                    tileGridPosition.x * (tileRadius * 1.5f),
                    tileGridPosition.y * map.floorHeight,
                    tileGridPosition.z * (tileSideDistance * 2) + (tileGridPosition.x % 2 * tileSideDistance * Mathf.Sign(tileGridPosition.z))
                );
                tileObject.transform.position = tilePosition;
                tileObject.transform.rotation = Quaternion.identity;
                tileObject.name = (j).ToString() + ": " + tileGridPosition.ToString();
                tileObject.transform.localScale = Vector3.one * (map.tileRadius * (tile.type == Tile.TileType.STAIR ? 0.5f : 1));

                tileObject.GetComponentInChildren<MeshRenderer>().material.color = roomColor;

                for(int k = 0; k < tile.walls.Length; k++)
                {
                    Wall wall = tile.walls[k];
                    if(wall != null)
                    {
                        GameObject wallObject = Instantiate(wallPrefabNormal);
                        wallObject.transform.parent = tileObject.transform;
                        wallObject.transform.localScale = new Vector3(tileRadius, 1, 0.1f);
                        wallObject.transform.localPosition = new Vector3
                        (
                            map.hexagonExteriorSidePositions[k].x,
                            0,
                            map.hexagonExteriorSidePositions[k].y
                        );
                        wallObject.transform.LookAt(tilePosition);
                    }
                }
            }
        }
    }
}
