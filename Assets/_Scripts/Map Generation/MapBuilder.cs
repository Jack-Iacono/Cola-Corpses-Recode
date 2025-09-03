using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System.Drawing;
using System.Net;
using UnityEditor.SceneManagement;
using System;
using System.Linq;

public class MapBuilder : MonoBehaviour
{
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;

    public Mesh wallMesh;
    public Mesh floorMesh;

    [SerializeField]
    private List<RoomTheme> roomThemes = new List<RoomTheme>();

    [SerializeField]
    private List<MapPreset> roomPresets = new List<MapPreset>();

    private Vector3 mapDim = new Vector3(100, 1, 100);
    private Vector2 roomTileRange = new Vector2(50, 50);

    private float tileRadius = 2f;
    private float floorHeight = 2;
    private float wallWidth = 0.05f;

    public Map currentMap;

    // Start is called before the first frame update
    void Start()
    {
        currentMap = MapGenerator.Generate(mapDim, tileRadius, floorHeight, 2, roomTileRange, roomPresets, roomThemes);
        BuildMap(currentMap);
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

            UnityEngine.Color roomColor = UnityEngine.Random.ColorHSV();

            List<Tile> tiles = normalRooms[i].GetTiles();
            List<Wall> walls = new List<Wall>();
            for(int j = 0; j < tiles.Count; j++)
            {
                BuildTile(tiles[j], roomParent, roomColor);
                BuildWalls(tiles[j]);
                foreach(Wall w in tiles[j].GetWalls())
                {
                    if(w != null)
                        walls.Add(w);
                }
            }
            GenerateWallMesh(normalRooms[i]);
        }

        List<PresetRoom> bonusRooms = map.GetBonusRooms();
        for (int i = 0; i < bonusRooms.Count; i++)
        {
            // Generate the room from the prefab at the location of the first tile
            PresetRoom room = bonusRooms[i];
            GameObject roomParent = Instantiate(roomPresets[bonusRooms[i].presetIndex].obj, mapParent.transform);
            MapPreset preset = roomParent.GetComponent<MapPreset>();
            roomParent.name = "Bonus Room " + i;

            // Sets the room to the correct position on the map
            roomParent.transform.position = new Vector3
            (
                room.presetOrigin.x * (tileRadius * 1.5f),
                room.presetOrigin.y * map.floorHeight,
                room.presetOrigin.z * tileSideDistance
            );
            // Rotate the room to match the tile's desired rotation
            roomParent.transform.rotation = Quaternion.Euler(0, room.presetRotation, 0);
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
                            tile.obj.transform.position.y + floorHeight / 2,
                            map.hexagonExteriorSidePositions[k].y + tile.obj.transform.position.z
                        );
                        wallObject.transform.rotation = Quaternion.Euler(new Vector3(0, k * 60, 0));

                        tile.walls[k].obj = wallObject;

                        placedWalls.Add(wall);
                    }
                }
            }
        }

        Mesh GenerateWallMesh(Room room)
        {
            // Create a 2D list of combine instances that
            List<CombineInstance>[] materialGroups = new List<CombineInstance>[roomThemes[room.themeIndex].wallMaterials.Count];

            foreach (Tile tile in room.GetTiles())
            {
                Wall[] walls = tile.GetWalls();
                for (int i = 0; i < walls.Length; i++)
                {
                    // Use this to determine the index of the wall in respect to the tile for rotation
                    Wall wall = walls[i];

                    // If there is no wall in the given spot in the tile,
                    if (wall == null)
                        continue;

                    GameObject wallObject = wall.obj;
                    CombineInstance newInstance = new CombineInstance();
                    //newInstance.mesh = wallMesh;

                    Mesh n = new Mesh();
                    n.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) };
                    n.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
                    n.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                    newInstance.mesh = n;

                    Vector3 scale = new Vector3(1 + wallWidthOffset, 1, wallWidth) * tileRadius;
                    Vector3 pos = new Vector3
                    (
                        map.hexagonExteriorSidePositions[i].x + tile.obj.transform.position.x,
                        tile.obj.transform.position.y + floorHeight / 2,
                        map.hexagonExteriorSidePositions[i].y + tile.obj.transform.position.z
                    );
                    Quaternion rot = Quaternion.Euler(new Vector3(0, i * 60, 0));

                    Transform newTrans = wallObject.transform;
                    //wall.connectedTiles[0].room == room ? newTrans.rotation : Quaternion.Euler(newTrans.rotation.eulerAngles.x, newTrans.rotation.eulerAngles.y + 180, newTrans.rotation.eulerAngles.z),
                    Matrix4x4 transformationMatrix = Matrix4x4.TRS
                        (
                            pos, rot, scale
                        );
                    newInstance.transform = transformationMatrix;

                    if (materialGroups[wall.materialIndex] == null)
                        materialGroups[wall.materialIndex] = new List<CombineInstance>();
                    materialGroups[wall.materialIndex].Add(newInstance);
                }
            }

            List<Mesh> combinedMaterialMeshes = new List<Mesh>();
            foreach (List<CombineInstance> materialInstances in materialGroups)
            {
                Mesh newMesh = new Mesh();
                newMesh.CombineMeshes(materialInstances.ToArray(), true);
                combinedMaterialMeshes.Add(newMesh);
            }

            CombineInstance[] finalInstances = new CombineInstance[combinedMaterialMeshes.Count];
            for (int i = 0; i < combinedMaterialMeshes.Count; i++)
            {
                CombineInstance newInstance = new CombineInstance();
                newInstance.mesh = combinedMaterialMeshes[i];
                newInstance.transform = room.obj.transform.localToWorldMatrix;
                finalInstances[i] = newInstance;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(finalInstances, false);
            GameObject testObj = new GameObject("Static Walls Mesh");
            testObj.transform.parent = room.obj.transform;
            testObj.AddComponent<MeshFilter>().mesh = combinedMesh;
            testObj.AddComponent<MeshRenderer>().materials = roomThemes[room.themeIndex].wallMaterials.ToArray();

            return combinedMesh;
        }
}

    
}

[Serializable]
public class RoomTheme
{
    [SerializeField]
    public List<Material> floorMaterials = new List<Material>();
    [SerializeField]
    public List<Material> wallMaterials = new List<Material>();
}
