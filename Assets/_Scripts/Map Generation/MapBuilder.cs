using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System.Drawing;
using System.Net;

public class MapBuilder : MonoBehaviour
{
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;

    [SerializeField]
    private List<MapPreset> roomPresets = new List<MapPreset>();

    private Vector3 mapDim = new Vector3(100, 1, 100);
    private Vector2 roomTileRange = new Vector2(50, 100);

    private float tileRadius = 2f;
    private float floorHeight = 2;
    private float wallWidth = 0.05f;

    public Map currentMap;

    // Start is called before the first frame update
    void Start()
    {
        //currentMap = MapGenerator.Generate(mapDim, tileRadius, floorHeight, 5, roomTileRange, roomPresets);
        //BuildMap(currentMap);
    }

    private void BuildMap(Map map)
    {
        float tileRadius = map.tileRadius;
        float tileSideDistance = map.tileSideDistance;
        float wallWidthOffset = (wallWidth / 2 / Mathf.Sqrt(3)) * 2;

        GameObject mapParent = new GameObject("Map");
        
        List<Wall> placedWalls = new List<Wall>();

        List<Room> normalRooms = map.GetRooms();
        for( int i = 0; i < normalRooms.Count; i++)
        {
            GameObject roomParent = new GameObject("Room " + i);
            normalRooms[i].obj = roomParent;
            roomParent.transform.parent = mapParent.transform;

            UnityEngine.Color roomColor = Random.ColorHSV();

            List<Tile> tiles = normalRooms[i].GetTiles();
            for(int j = 0; j < tiles.Count; j++)
            {
                BuildTile(tiles[j], roomParent, roomColor);
                BuildWalls(tiles[j]);
            }
        }

        List<Room> bonusRooms = map.GetBonusRooms();
        for (int i = 0; i < bonusRooms.Count; i++)
        {
            // Generate the room from the prefab at the location of the first tile
            Tile originTile = bonusRooms[i].GetTiles()[0];
            GameObject roomParent = Instantiate(roomPresets[originTile.prefabIndex].obj, mapParent.transform);
            MapPreset preset = roomParent.GetComponent<MapPreset>();
            roomParent.name = "Bonus Room " + i;

            // Sets the room to the correct position on the map
            roomParent.transform.position = new Vector3
            (
                originTile.gridPosition.x * (tileRadius * 1.5f),
                originTile.gridPosition.y * map.floorHeight,
                originTile.gridPosition.z * tileSideDistance
            );
            // Rotate the room to match the tile's desired rotation
            roomParent.transform.rotation = Quaternion.Euler(0, originTile.prefabRotation, 0);
            roomParent.transform.localScale = Vector3.one * tileRadius;

            List<Tile> tiles = bonusRooms[i].GetTiles();
            for (int j = 0; j < tiles.Count; j++)
            {
                // Set the tile object from the linked objects dictionary in the preset
                tiles[j].obj = preset.GetFootprint()[j].tileObject;
                tiles[j].obj.name = "Tile " + tiles[j].gridPosition.ToString();
                BuildWalls(tiles[j]);
            }
        }

        void BuildTile(Tile tile, GameObject roomParent, UnityEngine.Color roomColor)
        {
            GameObject tileObject = Instantiate(floorPrefabNormal);
            tileObject.transform.parent = roomParent.transform;
            Vector3 tileGridPosition = tile.gridPosition;

            Vector3 tilePosition = new Vector3
            (
                tileGridPosition.x * (tileRadius * 1.5f),
                tileGridPosition.y * map.floorHeight,
                tileGridPosition.z * tileSideDistance
            );
            tileObject.transform.position = tilePosition;
            tileObject.transform.rotation = Quaternion.identity;
            tileObject.name = "Tile " + tileGridPosition.ToString();
            tileObject.transform.localScale = Vector3.one * map.tileRadius;

            tile.obj = tileObject;

            tileObject.GetComponentInChildren<MeshRenderer>().material.color = roomColor;
        }
        void BuildWalls(Tile tile)
        {
            GameObject tileObject = tile.obj;
            for (int k = 0; k < tile.walls.Length; k++)
            {
                // Check if this wall was already placed by another tile
                if (!placedWalls.Contains(tile.walls[k]))
                {
                    Wall wall = tile.walls[k];

                    if (wall != null)
                    {
                        GameObject wallObject = Instantiate(wallPrefabNormal, tileObject.transform);
                        wallObject.name = "Wall " + k;
                        // the extra amount accounts for 
                        wallObject.transform.localScale = new Vector3(1 + wallWidthOffset, 1, wallWidth);
                        wallObject.transform.position = new Vector3
                        (
                            map.hexagonExteriorSidePositions[k].x + tile.obj.transform.position.x,
                            0 + tile.obj.transform.position.y,
                            map.hexagonExteriorSidePositions[k].y + tile.obj.transform.position.z
                        );
                        wallObject.transform.rotation = Quaternion.Euler(new Vector3(0, k * 60, 0));

                        tile.walls[k].obj = wallObject;

                        placedWalls.Add(wall);
                    }
                }

                
            }
        }
    }
}
