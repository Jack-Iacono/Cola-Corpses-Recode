using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System;

public class MapBuilder : MonoBehaviour
{
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;

    public Mesh floorMesh;

    [SerializeField]
    private List<RoomTheme> roomThemes = new List<RoomTheme>();

    [SerializeField]
    private List<MapPreset> roomPresets = new List<MapPreset>();

    private Vector3 mapDim = new Vector3(100, 1, 100);
    private Vector2 roomTileRange = new Vector2(50, 50);

    private float tileRadius = 1f;
    private float floorHeight = 1;
    private float wallThickness = 0.05f;
    private float floorThickness = 0.05f;

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
        float wallWidthOffset = (wallThickness / 2 / Mathf.Sqrt(3)) * 2;

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
            GenerateTileMesh(normalRooms[i]);
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

            tileObject.transform.position = new Vector3
            (
                tileGridPosition.x * (tileRadius * 1.5f),
                tileGridPosition.y * map.floorHeight,
                tileGridPosition.z * tileSideDistance
            );
            tileObject.transform.rotation = Quaternion.identity;

            foreach(Collider c in tileObject.GetComponentsInChildren<Collider>())
            {
                c.transform.localScale = new Vector3
                (
                    c.transform.localScale.x * map.tileRadius,
                    c.transform.localScale.y * floorThickness,
                    c.transform.localScale.z * map.tileRadius
                );
            }
            tileObject.name = "Tile " + tileGridPosition.ToString();

            tile.obj = tileObject;
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
                        wallObject.transform.localScale = new Vector3(1 + wallWidthOffset, floorHeight, wallThickness);
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

            // Loop through all tiles within the given room
            foreach (Tile tile in room.GetTiles())
            {
                // Get the walls present within the current tile
                Wall[] walls = tile.GetWalls();

                // Loop through the walls in the tile to add their meshes to the correct "submesh" meshes
                for (int i = 0; i < walls.Length; i++)
                {
                    // Use this to determine the index of the wall in respect to the tile for rotation
                    Wall wall = walls[i];

                    // If there is no wall in the given spot in the tile,
                    if (wall == null)
                        continue;

                    GameObject wallObject = wall.obj;
                    CombineInstance newInstance = new CombineInstance();

                    // Create a basic 1 sided wall mesh for the wall since you won't be able to see it from the other side
                    Mesh n = new Mesh();
                    // This offset will place the mesh on the outer side of the wall so that it matches with collision
                    float zOffset = -wallThickness / 2 * tileRadius;
                    n.vertices = new Vector3[] 
                    { 
                        new Vector3(-0.5f, -0.5f, zOffset), 
                        new Vector3(0.5f, -0.5f, zOffset), 
                        new Vector3(-0.5f, 0.5f, zOffset), 
                        new Vector3(0.5f, 0.5f, zOffset) 
                    };
                    n.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
                    n.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                    newInstance.mesh = n;

                    // Set the "transform" of the mesh which is basically just the transform of the wall
                    Vector3 scale = new Vector3((1 + wallWidthOffset) * tileRadius, floorHeight, 1) ;
                    Vector3 pos = new Vector3
                    (
                        map.hexagonExteriorSidePositions[i].x + tile.obj.transform.position.x,
                        tile.obj.transform.position.y + floorHeight / 2,
                        map.hexagonExteriorSidePositions[i].y + tile.obj.transform.position.z
                    );
                    Quaternion rot = Quaternion.Euler(new Vector3(0, i * 60, 0));

                    Matrix4x4 transformationMatrix = Matrix4x4.TRS
                        (
                            pos, rot, scale
                        );
                    newInstance.transform = transformationMatrix;

                    // Add this mesh to the appropriate group for submesh creation
                    if (materialGroups[wall.materialIndexes[tile]] == null)
                        materialGroups[wall.materialIndexes[tile]] = new List<CombineInstance>();
                    materialGroups[wall.materialIndexes[tile]].Add(newInstance);
                }
            }

            // Create a list of material meshes that each contain meshes with one material type
            List<Mesh> combinedMaterialMeshes = new List<Mesh>();
            foreach (List<CombineInstance> materialInstances in materialGroups)
            {
                Mesh newMesh = new Mesh();
                newMesh.CombineMeshes(materialInstances.ToArray(), true);
                combinedMaterialMeshes.Add(newMesh);
            }

            // Create a final combined mesh that has a submesh for each material
            CombineInstance[] finalInstances = new CombineInstance[combinedMaterialMeshes.Count];
            for (int i = 0; i < combinedMaterialMeshes.Count; i++)
            {
                CombineInstance newInstance = new CombineInstance();
                newInstance.mesh = combinedMaterialMeshes[i];
                newInstance.transform = room.obj.transform.localToWorldMatrix;
                finalInstances[i] = newInstance;
            }

            // Assign this mesh to a new gameobject within the room
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(finalInstances, false);
            GameObject testObj = new GameObject("Static Walls Mesh");
            testObj.transform.parent = room.obj.transform;
            testObj.AddComponent<MeshFilter>().mesh = combinedMesh;
            testObj.AddComponent<MeshRenderer>().materials = roomThemes[room.themeIndex].wallMaterials.ToArray();

            return combinedMesh;
        }
        Mesh GenerateTileMesh(Room room)
        {
            // Create a 2D list of combine instances that
            List<CombineInstance>[] materialGroups = new List<CombineInstance>[roomThemes[room.themeIndex].floorMaterials.Count];

            // Loop through all tiles within the given room
            foreach (Tile tile in room.GetTiles())
            {
                CombineInstance newInstance = new CombineInstance();

                GameObject tileObject = tile.obj;
                newInstance.mesh = floorMesh;

                Vector3 scale = new Vector3(tileObject.transform.localScale.x, tileObject.transform.localScale.y * floorThickness, tileObject.transform.localScale.z);
                Vector3 pos = tileObject.transform.position;
                Quaternion rot = tileObject.transform.rotation;

                newInstance.transform = Matrix4x4.TRS( pos, rot, scale );

                // Add this mesh to the appropriate group for submesh creation
                if (materialGroups[tile.materialIndex] == null)
                    materialGroups[tile.materialIndex] = new List<CombineInstance>();
                materialGroups[tile.materialIndex].Add(newInstance);
            }

            // Create a list of material meshes that each contain meshes with one material type
            List<Mesh> combinedMaterialMeshes = new List<Mesh>();
            foreach (List<CombineInstance> materialInstances in materialGroups)
            {
                Mesh newMesh = new Mesh();
                newMesh.CombineMeshes(materialInstances.ToArray(), true);
                combinedMaterialMeshes.Add(newMesh);
            }

            // Create a final combined mesh that has a submesh for each material
            CombineInstance[] finalInstances = new CombineInstance[combinedMaterialMeshes.Count];
            for (int i = 0; i < combinedMaterialMeshes.Count; i++)
            {
                CombineInstance newInstance = new CombineInstance();
                newInstance.mesh = combinedMaterialMeshes[i];
                newInstance.transform = room.obj.transform.localToWorldMatrix;
                finalInstances[i] = newInstance;
            }

            // Assign this mesh to a new gameobject within the room
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(finalInstances, false);
            GameObject testObj = new GameObject("Static Tile Mesh");
            testObj.transform.parent = room.obj.transform;
            testObj.AddComponent<MeshFilter>().mesh = combinedMesh;
            testObj.AddComponent<MeshRenderer>().materials = roomThemes[room.themeIndex].floorMaterials.ToArray();

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
