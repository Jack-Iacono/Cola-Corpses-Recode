using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System;
using UnityEngine.UIElements;
using UnityEditor.Presets;

public class MapBuilder : MonoBehaviour
{
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;

    public Mesh floorMesh;

    [SerializeField]
    private List<RoomTheme> roomThemes = new List<RoomTheme>();

    [SerializeField]
    private List<MapPreset> presets = new List<MapPreset>();

    // Double the Z that you want, the way hex grids works takes out half of the Z positions
    // Yes, I know what I'm doing, don't question me
    private readonly Vector3 mapDim = new Vector3(100, 5, 200);
    private Vector2 roomTileRange = new Vector2(100, 100);
    private int roomCount = 3;

    // The scale of the map, mostly added this for fun, but maybe allow users to mess around with it
    private const float MAP_SCALE = 1f;

    public Map currentMap;

    // Start is called before the first frame update
    void Start()
    {
        currentMap = MapGenerator.Generate(mapDim, roomCount, roomTileRange, presets, roomThemes);
        BuildMap(currentMap);
    }

    private void BuildMap(Map map)
    {
        float tileRadius = Map.TILE_RADIUS;
        float tileSideDistance = Map.TILE_SIDE_DISTANCE;
        float wallWidthOffset = (Map.WALL_THICKNESS / 2 / Mathf.Sqrt(3)) * 2;

        GameObject mapParent = new GameObject("Map");
        
        List<Wall> placedWalls = new List<Wall>();

        List<Room> normalRooms = map.GetRooms();
        for( int i = 0; i < normalRooms.Count; i++)
        {
            GameObject roomParent = new GameObject("Room " + i);
            normalRooms[i].obj = roomParent;
            roomParent.transform.parent = mapParent.transform;

            List<Tile> tiles = normalRooms[i].GetTiles();

            // Spawn the presets within the given room
            for (int j = 0; j < normalRooms[i].presets.Count; j++)
            {
                PresetData data = normalRooms[i].presets[j];
                GameObject presetParent = Instantiate(presets[data.index].obj, roomParent.transform);
                MapPreset preset = presetParent.GetComponent<MapPreset>();
                preset.CreateTileLinks();
                presetParent.name = "Preset " + j;

                // The origin of the preset in respect to it's rotation
                Vector3 rotatedOrigin = CubeCoord.GetRotatedPosition(data.localOrigin, Vector3.zero, data.rotation);

                // Sets the room to the correct position on the map
                presetParent.transform.position = new Vector3
                (
                    (data.globalOrigin.x - rotatedOrigin.x) * (tileRadius * 1.5f),
                    (data.globalOrigin.y - rotatedOrigin.y) * Map.FLOOR_HEIGHT,
                    (data.globalOrigin.z - rotatedOrigin.z) * tileSideDistance
                );
                // Rotate the room to match the tile's desired rotation
                presetParent.transform.rotation = Quaternion.Euler(0, data.rotation, 0);
                presetParent.transform.localScale = Vector3.one * tileRadius;

                // Connect the tile to the object that was instantiated
                foreach(Tile t in data.tiles.Keys)
                {
                    if(t.type == TileType.CUSTOM)
                    {
                        t.obj = preset.tileLinks[data.tiles[t]];
                        t.obj.name = "Preset Tile " + t.gridPosition;
                    }
                }
            }

            // Create the tiles and walls from each tile
            for (int j = 0; j < tiles.Count; j++)
            {
                BuildTile(tiles[j], roomParent);
                BuildWalls(tiles[j]);
            }

            // Generate the wall and tile mesh for this room
            GenerateTileMesh(normalRooms[i]);
            GenerateWallMesh(normalRooms[i]);
        }

        mapParent.transform.localScale *= MAP_SCALE;

        void BuildTile(Tile tile, GameObject roomParent)
        {
            if(tile.type == TileType.NORMAL)
            {
                GameObject tileObject = Instantiate(floorPrefabNormal);
                tileObject.transform.parent = roomParent.transform;
                Vector3 tileGridPosition = tile.gridPosition;

                tileObject.transform.position = new Vector3
                (
                    tileGridPosition.x * (tileRadius * 1.5f),
                    tileGridPosition.y * Map.FLOOR_HEIGHT,
                    tileGridPosition.z * tileSideDistance
                );
                tileObject.transform.rotation = Quaternion.identity;

                foreach (Collider c in tileObject.GetComponentsInChildren<Collider>())
                {
                    c.transform.localScale = new Vector3
                    (
                        c.transform.localScale.x * Map.TILE_RADIUS,
                        c.transform.localScale.y * Map.FLOOR_THICKNESS,
                        c.transform.localScale.z * Map.TILE_RADIUS
                    );
                }
                tileObject.name = "Tile " + tileGridPosition.ToString();

                tile.obj = tileObject;
            }
            else if(tile.type == TileType.EMPTY)
            {
                GameObject tileObject = new GameObject();
                tileObject.transform.parent = roomParent.transform;
                Vector3 tileGridPosition = tile.gridPosition;

                tileObject.transform.position = new Vector3
                (
                    tileGridPosition.x * (tileRadius * 1.5f),
                    tileGridPosition.y * Map.FLOOR_HEIGHT,
                    tileGridPosition.z * tileSideDistance
                );
                tileObject.transform.rotation = Quaternion.identity;
                tileObject.name = "Tile " + tileGridPosition.ToString();

                tile.obj = tileObject;
            }
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

                    // Make sure the wall is normal
                    if (wall != null && wall.type == WallType.NORMAL)
                    {
                        // Create the wall object
                        GameObject wallObject = Instantiate(wallPrefabNormal, tileObject.transform);
                        wallObject.name = "Wall " + k;

                        // This size mod will adjust the size of the walls on the preset to account for the change in size of the preset
                        float sizeMod = tile.type == TileType.CUSTOM ? Map.TILE_RADIUS : 1;

                        // Set the transform of the wall
                        wallObject.GetComponentInChildren<Collider>().transform.localScale = new Vector3(Map.TILE_RADIUS + wallWidthOffset * Map.TILE_RADIUS, Map.FLOOR_HEIGHT, Map.WALL_THICKNESS * Map.TILE_RADIUS) / sizeMod;
                        wallObject.transform.position = new Vector3
                        (
                            map.hexagonExteriorSidePositions[k].x + tile.obj.transform.position.x,
                            tile.obj.transform.position.y + Map.FLOOR_HEIGHT / 2,
                            map.hexagonExteriorSidePositions[k].y + tile.obj.transform.position.z
                        );
                        wallObject.transform.rotation = Quaternion.Euler(new Vector3(0, k * 60, 0));

                        // Assign this object to the wall data type
                        tile.walls[k].obj = wallObject;

                        // Add this wall to the list of placed walls
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

                    // If there is no wall in the given spot in the tile or if the wall is not normal, skip it
                    if (wall == null || wall.type != WallType.NORMAL)
                        continue;

                    GameObject wallObject = wall.obj;
                    CombineInstance newInstance = new CombineInstance();

                    // Create a basic 1 sided wall mesh for the wall since you won't be able to see it from the other side
                    Mesh n = new Mesh();
                    // This offset will place the mesh on the outer side of the wall so that it matches with collision
                    float zOffset = -Map.WALL_THICKNESS / 2 * tileRadius;
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
                    Vector3 scale = new Vector3((1 + wallWidthOffset) * tileRadius, Map.FLOOR_HEIGHT, 1) ;
                    Vector3 pos = new Vector3
                    (
                        map.hexagonExteriorSidePositions[i].x + tile.obj.transform.position.x,
                        tile.obj.transform.position.y + Map.FLOOR_HEIGHT / 2,
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
                // Exclude non normal tiles from generation
                if (tile.type != TileType.NORMAL)
                    continue;

                CombineInstance newInstance = new CombineInstance();

                GameObject tileObject = tile.obj;
                newInstance.mesh = floorMesh;

                // Get the transform of the tile
                Vector3 scale = new Vector3(tileObject.transform.localScale.x * Map.TILE_RADIUS, tileObject.transform.localScale.y / 2 * Map.FLOOR_THICKNESS, tileObject.transform.localScale.z * Map.TILE_RADIUS);
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
