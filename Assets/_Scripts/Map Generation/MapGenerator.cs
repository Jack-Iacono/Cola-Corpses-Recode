using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System.Drawing;

public class MapGenerator : MonoBehaviour
{
    public GameObject floorObject;
    public GameObject wallObject;

    private Vector3 mapDim = new Vector3(50, 1, 50);
    private Vector2 roomTileRange = new Vector2(50, 100);

    // These offsets are used to set the positions of the floors
    private float tileRadius = 1f;

    public Map currentMap;

    // Start is called before the first frame update
    void Start()
    {
        currentMap = new Map(mapDim, tileRadius, 3, roomTileRange);
        currentMap.GenerateMap();
        BuildMap(currentMap);
    }

    private void BuildMap(Map map)
    {
        float tileRadius = map.tileRadius;
        float tileSideDistance = map.tileSideDistance;

        List<Room> rooms = map.GetRooms();
        for( int i = 0; i < rooms.Count; i++)
        {
            UnityEngine.Color roomColor = Random.ColorHSV();

            List<Tile> tiles = rooms[i].GetTiles();
            for(int j = 0; j < tiles.Count; j++)
            {
                Tile tile = tiles[j];
                GameObject tileObject = Instantiate(floorObject);
                Vector3 tileGridPosition = tile.gridPosition;

                Vector3 tilePosition = new Vector3
                (
                    tileGridPosition.x * (tileRadius * 1.5f),
                    0,
                    tileGridPosition.z * (tileSideDistance * 2) + (tileGridPosition.x % 2 * tileSideDistance * Mathf.Sign(tileGridPosition.z))
                );
                tileObject.transform.position = tilePosition;
                tileObject.transform.rotation = Quaternion.identity;
                tileObject.name = tileGridPosition.ToString();

                tileObject.GetComponentInChildren<MeshRenderer>().material.color = roomColor;

                foreach (Vector2 midPoint in map.hexagonExteriorSidePositions)
                {
                    GameObject wall = Instantiate(wallObject);
                    wall.transform.parent = tileObject.transform;
                    wall.transform.localScale = new Vector3(tileRadius, 1, 0.1f);
                    wall.transform.localPosition = new Vector3
                    (
                        midPoint.x,
                        0,
                        midPoint.y
                    );
                    wall.transform.LookAt(tilePosition);
                }
            }
        }

        /*

        GameObject floor = Instantiate(floorObject);
        Vector3 tilePosition = new Vector3
        (
            graphPoint.x * (HEX_RADIUS * 1.5f),
            0,
            graphPoint.y * (hexShortLength * 2) + (graphPoint.x % 2 * hexShortLength * Mathf.Sign(graphPoint.y))
        );
        floor.transform.position = tilePosition;
        floor.transform.rotation = Quaternion.identity;
        floor.name = graphPoint.ToString();

        floor.GetComponentInChildren<MeshRenderer>().material.color = Random.ColorHSV();

        foreach(Vector2 midPoint in hexagonExteriorSidePositions)
        {
            GameObject wall = Instantiate(wallObject);
            wall.transform.parent = floor.transform;
            wall.transform.localScale = new Vector3(HEX_RADIUS, 1, 0.1f);
            wall.transform.localPosition = new Vector3
            (
                midPoint.x,
                0,
                midPoint.y
            );
            wall.transform.LookAt(tilePosition);
        }

        */
    }
}
