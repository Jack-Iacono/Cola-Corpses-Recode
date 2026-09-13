using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using MapUtil;
using System;
using Unity.AI.Navigation;
using System.Linq;

public class MapBuilder : MonoBehaviour
{
    // This class would be static if not for the prefabs that need to be given in the editor
    public static MapBuilder Instance;

    [Header("Foundation Prefabs")]
    public GameObject floorPrefabNormal;
    public GameObject wallPrefabNormal;
    public GameObject ceilingPrefab;
    public GameObject doorPrefab;

    [Header("Accessory Prefabs")]
    // Store important elements in variables for more concrete reference
    public TileElement spawner;

    // Store decorations in a more generic list for reference later
    public TileElement[] decoElements;

    // Use this to store all references for every tile element for building purposes
    private Dictionary<int, TileElement> tileElementReference = new Dictionary<int, TileElement>();

    [Header("Meshes")]
    public Mesh hexMesh;
    public Mesh wallMesh;

    [SerializeField]
    private List<RoomTheme> roomThemes = new List<RoomTheme>();

    [SerializeField]
    private List<MapPreset> roomPresets = new List<MapPreset>();
    

    // Double the Z that you want, the way hex grids works takes out half of the Z positions
    // Yes, I know what I'm doing, don't question me
    private readonly Vector3 mapDim = new Vector3(100, 5, 200);
    private Vector2 roomTileRange = new Vector2(40, 40);
    private int roomCount = 10;
    private float prefabPlaceChance = 0.01f;

    // The scale of the map, mostly added this for fun, but maybe allow users to mess around with it
    private const float MAP_SCALE = 1f;

    // Create a singleton for this class
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;

            int currentID = 1;

            // Import Tile Elements
            spawner.id = currentID++;
            tileElementReference.Add(spawner.id, spawner);

            // Initialize the id values for each of the tile elements
            foreach (TileElement decoElement in decoElements)
            {
                decoElement.id = currentID++;
                tileElementReference[decoElement.id] = decoElement;
            }

            // This is used for debugging purposes
            int seed = UnityEngine.Random.Range(0, 9999);
            Debug.Log(seed);
            UnityEngine.Random.InitState(seed);
        }
        else
            Destroy(this);
    }

    public Map GetNewMap()
    {
        Map newMap = GenerateMap();
        newMap.originTile = new Vector3
                (
                    newMap.originTile.x * (Map.TILE_RADIUS * 1.5f),
                    newMap.originTile.y * Map.FLOOR_HEIGHT,
                    newMap.originTile.z * Map.TILE_SIDE_DISTANCE
                );
        return newMap;
    }
    
    public Map GenerateMap()
    {
        // While this is not technically null, the graph that generates the points will never have to use this position
        Vector3 NULL_VECTOR = new Vector3(-1, -1, -1);

        // The map will store all the placed tiles and rooms
        Map genMap = new Map(mapDim);

        // Dead zones and roomGenPaths allow the generator to correctly generate rooms with full exploration
        List<Vector3> deadZones = new List<Vector3>();
        List<List<Vector3>> roomGenPaths = new List<List<Vector3>>();

        // Store the potential doors that are found from generating the map
        Dictionary<RoomPair, List<Wall>> potentialDoors = new Dictionary<RoomPair, List<Wall>>();

        // Store the doors that lead out of contained presets
        // This is a little more complicated since each preset needs to store multiple exit tiles and their doors
        Dictionary<Tile, List<Wall>> containedPresetWalls = new Dictionary<Tile, List<Wall>>();

        // Orgainze the preset indices based on their function
        List<int> stairPresetIndices = new List<int>();
        List<int> normalPresetIndices = new List<int>();

        for (int i = 0; i < roomPresets.Count; i++)
        {
            if (roomPresets[i].isStair)
                stairPresetIndices.Add(i);
            else
                normalPresetIndices.Add(i);
        }

        // Get the mid point of the map bounds and set that to the origin point of the map
        Vector3 currentLocation = new Vector3(Mathf.FloorToInt(genMap.mapBounds.x / 2), Mathf.FloorToInt(genMap.mapBounds.y / 2), Mathf.FloorToInt(genMap.mapBounds.z / 2));
        genMap.originTile = currentLocation;

        // Create the desired amount of rooms
        for (int i = 0; i < roomCount; i++)
        {
            // Create a new room to hold the tiles, but don't add it to the map yet in case it doesn't generate fully
            Room newRoom = GenerateRoom(i);
            if (newRoom != null)
            {
                // Add each tile to the tile map for later access and comparison
                foreach (Tile t in newRoom.GetTiles())
                {
                    Vector3 pos = t.gridPosition;
                    // Change this to uninclude presets later
                    t.SetMaterialIndex(roomThemes[newRoom.themeIndex].GetRandomFloorMaterial());
                    genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] = t;
                }
            }
        }

        // Generate the walls for the map
        foreach (Room room in genMap.GetRooms())
        {
            SetRoomWalls(room);
            SetRoomCeilings(room);
        }

        // Place all the doors on the map
        SetDoors();
        SetTileElements();

        return genMap;

        // The below functions are used purely for organizational purposes -----------------------------------------------------

        // You'll never guess what this does
        Room GenerateRoom(int roomIndex)
        {
            // Create the new Room to hold the data needed
            Room newRoom = new Room();
            newRoom.index = roomIndex;

            // Assign a theme to the room randomly from the list of themes
            newRoom.themeIndex = UnityEngine.Random.Range(0, roomThemes.Count);

            // Get a temporary clone of the map tiles for modification here
            List<Vector3> genPath = new List<Vector3>();

            // The amount of tiles that should be generated for this room
            int roomTileGoal = UnityEngine.Random.Range((int)roomTileRange.x, (int)roomTileRange.y);

            // Generate the tiles in the given room
            for (int j = 0; j < roomTileGoal; j++)
            {
                Vector3 nextTilePosition = NULL_VECTOR;

                if (UnityEngine.Random.Range(0, 1f) < prefabPlaceChance)
                {
                    // Get a random preset to place at thhis position
                    PresetData preset = GetPreset(-1, currentLocation, genPath, newRoom);

                    // Check if the preset is invalid
                    if (preset != null)
                    {
                        // Add the preset to the room's list of presets
                        // This allows the room to instantiate the preset later in the building phase
                        newRoom.presets.Add(preset);

                        // Loop through all preset tiles within the preset
                        foreach (Tile tile in preset.tiles.Keys)
                        {
                            // Add the new tile to the room
                            newRoom.AddTile(tile);
                        }

                        // Check if there are any valid exit positions from this preset
                        //  If there are none: Don't do anything, the algorithm will continue from the previous position
                        //  If there are: Continue through this prefab, this looks different for contained vs traversal presets
                        if (preset.exitPositions.Count > 0)
                        {
                            // If the room had an exit point, get a random one
                            if (!preset.isContained)
                            {
                                // Add the current location as well as the next location to the genPath for use later
                                genPath.Add(currentLocation);
                                Vector3 exitPosition = preset.exitPositions[UnityEngine.Random.Range(0, preset.exitPositions.Count)];
                                genPath.Add(exitPosition);
                                currentLocation = exitPosition;
                            }
                            else
                            {
                                // Pick a random exit and get a tile that could come next
                                // This tile will likely be, but won't necessarily be, connected to the chosen exit
                                int index = UnityEngine.Random.Range(0, preset.exitPositions.Count);
                                Vector3 next = GetNextTile(preset.exitPositions[index], ref genPath, newRoom);
                                genPath.Add(next);

                                // Create a new tile to take the place of the next tile placed by the 
                                Tile newTile = new Tile(next, newRoom);
                                newRoom.AddTile(newTile);

                                // Move the current location to match the changes made here
                                currentLocation = next;
                            }
                        }
                    }
                    else
                    {
                        // Place a normal tile here since the preset placement was failed, and otherwise no tile will be placed here
                        
                        // Add this tile to the path of generated tiles
                        genPath.Add(currentLocation);

                        // create a new tile at this location
                        Tile newTile = new Tile(currentLocation, newRoom);

                        newRoom.AddTile(newTile);
                    }
                }
                else
                {
                    // Add this tile to the path of generated tiles
                    genPath.Add(currentLocation);

                    // create a new tile at this location
                    Tile newTile = new Tile(currentLocation, newRoom);

                    newRoom.AddTile(newTile);
                }

                // Get next tile in the map
                nextTilePosition = GetNextTile(currentLocation, ref genPath, newRoom);
                if (nextTilePosition == NULL_VECTOR)
                    break;

                // Set the current location to the next location
                currentLocation = nextTilePosition;
            }

            // Check to see if the room fully generated
            if (newRoom.GetTiles().Count < roomTileGoal * 0.75f)
            {
                // Add the room tiles to the deadzone
                foreach (Tile t in newRoom.GetTiles())
                {
                    deadZones.Add(t.gridPosition);
                }

                // If this triggers, all possible neighbors are invalid and the map is basically filled
                if (roomIndex - 1 < 0)
                {
                    return null;
                }

                // Loop through the whole map to find a new starting location
                Vector3 nextLocation = BacktrackMap();

                // This will run if no previous rooms have any open tiles at all, in which case the map is dead and done
                if (nextLocation == NULL_VECTOR)
                    return null;

                // Set the current location and generate a room at the new location
                currentLocation = nextLocation;
                return GenerateRoom(roomIndex);
            }

            // Add the current room to the map since it generated correctly
            genMap.rooms.Add(newRoom);
            currentLocation = BacktrackRoom(ref genPath, newRoom);
            roomGenPaths.Add(genPath);

            return newRoom;
        }

        // These methods control the flow of the algorithm through the map
        Vector3 GetNextTile(Vector3 current, ref List<Vector3> roomGenPath, Room room = null)
        {
            Vector3 nextTilePosition = GetRandomValidNeighbor(current, room);
            if (nextTilePosition == NULL_VECTOR)
            {
                // Remove this tile from the path since it is invlaid due to not having a neighbor
                if (roomGenPath.Count < 1)
                    return NULL_VECTOR;

                roomGenPath.RemoveAt(roomGenPath.Count - 1);
                return BacktrackRoom(ref roomGenPath, room);
            }

            return nextTilePosition;
        }
        Vector3 GetRandomValidNeighbor(Vector3 current, Room room = null)
        {
            List<Vector3> validHorizontalPositions = new List<Vector3>();
            List<Vector3> validVerticalPosition = new List<Vector3>();
            Vector3[] neighbors = genMap.neighbors;

            // Go through all neighbors for the given cell
            for (int i = 0; i < neighbors.Length; i++)
            {
                Vector3 neighbor = neighbors[i] + current;
                if (PositionOpen(neighbor, room) && !deadZones.Contains(neighbor))
                {
                    if (neighbors[i].y == 0)
                        validHorizontalPositions.Add(neighbor);
                    else
                        validVerticalPosition.Add(neighbor);
                }
            }

            // If there is a vertical neighbor and either the cahnce to change floors happens, or there are no horizontal neighbors
            if (validVerticalPosition.Count > 0 && (UnityEngine.Random.Range(0, 1f) < prefabPlaceChance || validHorizontalPositions.Count == 0))
                return validVerticalPosition[UnityEngine.Random.Range(0, validVerticalPosition.Count)];
            else if (validHorizontalPositions.Count > 0)
                return validHorizontalPositions[UnityEngine.Random.Range(0, validHorizontalPositions.Count)];

            // If no valid neighbor was found, return a null
            return NULL_VECTOR;
        }
        Vector3 BacktrackRoom(ref List<Vector3> roomGenPath, Room room = null)
        {
            // Run through the list, removing items as you go to find one that has an open tile
            for (int i = roomGenPath.Count - 1; i > 0; i--)
            {
                // Evaluate the position for neighbors and return if a neighbor with open spaces is found
                Vector3 neighborCheck = GetRandomValidNeighbor(roomGenPath[i], room);
                
                if (neighborCheck != NULL_VECTOR)
                    return neighborCheck;
                else
                    roomGenPath.RemoveAt(i);
            }

            return NULL_VECTOR;
        }
        Vector3 BacktrackMap(int startingRoom = -1)
        {
            // Check whether to start at the end of the map generation or at one specific room
            int startIndex = startingRoom == -1 ? roomGenPaths.Count - 1 : startingRoom;

            // Go backwards through all rooms in the map
            for (int i = startIndex; i >= 0; i--)
            {
                List<Vector3> path = roomGenPaths[i];
                Vector3 neighbor = BacktrackRoom(ref path);

                // Check to see if a neighbor was found in the previous room
                if (neighbor != NULL_VECTOR)
                    return neighbor;
            }

            // THERE IS NOTHING AVAILABLE
            return NULL_VECTOR;
        }

        // These methods get valid preset locations based on the current state of the map
        PresetData GetPreset(int presetIndex, Vector3 origin, List<Vector3> path, Room room)
        {
            // Get the list of valid presets for the location
            // if the user enters -1 for the preset, a random preset will be chosen
            List<PresetData> validPresets = presetIndex != -1 ? GetValidPresetRotations(presetIndex, origin, room) : GetValidPresets(origin, room);

            // Check if there are no presets to choose from
            if (validPresets == null || validPresets.Count == 0)
            {
                // This code could occasionally cause the entire room to be backtracked
                // the intention was to help a prefab spawn in order to maintain the desired amount, but may require a backtracking rework
                //Vector3 nextNeighbor = BacktrackRoom(ref path, room);
                //if (nextNeighbor != NULL_VECTOR)
                //{
                //    path.RemoveAt(path.Count - 1);
                //    return GetPreset(presetIndex, nextNeighbor, path, room);
                //}
                //else
                //    return null;
                return null;
            }

            // Return a random rotation of this preset
            return validPresets[UnityEngine.Random.Range(0, validPresets.Count)];
        }
        PresetData GetPresetFrom(List<int> presetIndex, Vector3 origin, List<Vector3> path, Room room)
        {
            // Initialize a list to contain all valid presets
            List<PresetData> validPresets = new List<PresetData>();

            // Run through all preset indices within the given list
            for (int i = 0; i < presetIndex.Count; i++)
            {
                // Get the valid preset orientations
                List<PresetData> p = GetValidPresetRotations(presetIndex[i], origin, room);

                // Check if there are any valid presets at the given tile
                if (p != null)
                    validPresets.AddRange(p);
            }

            // Check if there are presets to choose from
            if (validPresets.Count == 0)
            {
                Vector3 nextNeighbor = BacktrackRoom(ref path, room);
                if (nextNeighbor != NULL_VECTOR)
                {
                    path.RemoveAt(path.Count - 1);
                    return GetPresetFrom(presetIndex, nextNeighbor, path, room);
                }
                else
                    return null;
            }

            // Return a random rotation of this preset
            return validPresets[UnityEngine.Random.Range(0, validPresets.Count)];
        }
        List<PresetData> GetValidPresets(Vector3 presetStart, Room room)
        {
            // Make a copy of the preset list for use in this function
            List<MapPreset> presets = new List<MapPreset>(roomPresets);

            // Loop until either preset is found or the list is empty
            while (presets.Count > 0)
            {
                // Get a random preset index and remove it from the list
                int index = UnityEngine.Random.Range(0, presets.Count);
                presets.RemoveAt(index);

                // Get the valid rotations of this preset at the position
                List<PresetData> validPresets = GetValidPresetRotations(index, presetStart, room);

                // Check to see if this preset has any valid rotations
                if (validPresets.Count > 0)
                {
                    // Return a preset data with the random rotation
                    return validPresets;
                }
            }

            return null;
        }
        List<PresetData> GetValidPresetRotations(int presetIndex, Vector3 origin, Room room)
        {
            MapPreset preset = roomPresets[presetIndex];
            List<Preset_Tile> tiles = preset.GetFootprint();
            List<PresetData> validPresets = new List<PresetData>();

            // Check every entry point
            foreach (Vector3 entryTilePosition in preset.entryPoints)
            {
                // Check every rotation of the room in respect to the entrypoint
                for (int i = 0; i < 6; i++)
                {
                    // Create an array to store all the tile gridPositions
                    Dictionary<Tile, Vector3> presetTiles = new Dictionary<Tile, Vector3>();
                    List<Vector3> globalExitPositions = new List<Vector3>();

                    // Loop through each tile in the preset for the given rotation
                    for (int j = 0; j < tiles.Count; j++)
                    {
                        // Get the local position of the tile rotated around the origin
                        Vector3 rotPosition = CubeCoord.GetRotatedPosition(tiles[j].gridPosition - entryTilePosition, Vector3.zero, i * 60);

                        // Check if the global position of this tile is open and add it if it is
                        if (PositionOpen(rotPosition + origin, room))
                        {
                            // Create the new tile
                            Tile newTile = new Tile(rotPosition + origin, room);

                            // Denote this space as a potential exit point for the preset so that the generation can continue through it
                            if (preset.entryPoints.Contains(tiles[j].gridPosition) && tiles[j].gridPosition != entryTilePosition)
                                globalExitPositions.Add(rotPosition + origin);

                            // Check if this tile is not an exit/entry tile, if so lock it since it is within a prefab
                            if (tiles[j].gridPosition != entryTilePosition && !preset.entryPoints.Contains(tiles[j].gridPosition))
                                newTile.modificationLocked = true;
                            newTile.presetContained = preset.isContained;

                            // Need a way to send transform of tile to the builder, maybe through coordinates
                            if (tiles[j].isEmpty)
                                newTile.SetType(TileType.EMPTY);
                            else if (tiles[j].tileObject == null)
                                newTile.SetType(TileType.NORMAL);
                            else
                                newTile.SetType(TileType.CUSTOM);

                            presetTiles.Add(newTile, tiles[j].gridPosition);
                        }
                        else
                            // If this position is not open, break the loop since this preset can't be used
                            break;
                    }

                    // Check to see if all tiles cleared, if so, add this to the list of valid presets
                    if (presetTiles.Count == tiles.Count)
                    {
                        validPresets.Add(new PresetData(entryTilePosition, origin, i * 60, presetIndex, presetTiles, globalExitPositions, preset));
                    }
                }
            }

            return validPresets;
        }

        // These functions are made to run after the map footprint is fully generated to give it other features
        // Would it be more effecient to do this during generation, probably, does this allow it to have a more wholistic view of the map, yes, so shut up
        void SetRoomCeilings(Room room)
        {
            List<Tile> tiles = room.GetTiles();
            foreach (Tile tile in tiles)
            {
                // Get the potential tile directly above this tile
                Tile aboveTile = genMap.GetTileAtLocation(tile.gridPosition + Vector3.up);

                // Check to make sure that the tile above is not just empty (in which case there should not be a ceiling)
                if (aboveTile == null || aboveTile.type != TileType.EMPTY)
                {
                    // Create the ceiling and assign it to the tile
                    Ceiling ceil = new Ceiling();
                    ceil.SetMaterialIndex(roomThemes[room.themeIndex].GetRandomCeilingMaterial());
                    ceil.SetHasCollider(aboveTile == null);
                    tile.SetCeiling(ceil);
                }
            }
        }
        void SetRoomWalls(Room room)
        {
            // Run through all tiles and place the necessary walls
            // This works since this is the minimum walls a preset could have as well
            List<Tile> roomTiles = room.GetTiles();

            // Used to determine what other rooms this room borders to determine it's distance
            int minDistance = int.MaxValue;

            // This is for normal tiles
            foreach (Tile tile in room.GetTiles())
            {
                // Don't evaluate this tile if it is in a contained preset
                if (tile.presetContained)
                    continue;

                Vector3 tilePosition = tile.gridPosition;
                Vector3[] nList = genMap.neighbors;

                for (int i = 0; i < nList.Length; i++)
                {
                    // Global neighbor shows tiles placed on the global map while local Neighbor
                    Tile neighbor = genMap.GetTileAtLocation(tilePosition + nList[i]);

                    // Check if there is a tile and if it is in another room
                    if (neighbor != null && neighbor.room != room)
                    {
                        // Document this room if it has a shorter distance than the current, and if so, use it
                        if (room.index == 0)
                            minDistance = -1;
                        else if (neighbor.room.roomDistance < minDistance)
                            minDistance = neighbor.room.roomDistance;

                        // These walls should be stored as candidates for doors between rooms
                        Wall neighborWall = neighbor.walls[(i + 3) % 6];

                        // Create a pair of rooms and add it to the potential doors array if not already present
                        // Order the rooms so that the room with the lowest index is always first. This allows hashcode and equals to work
                        RoomPair connectedRooms;
                        if (room.index < neighbor.room.index)
                            connectedRooms = new RoomPair(room, neighbor.room);
                        else
                            connectedRooms = new RoomPair(neighbor.room, room);

                        // Add the new room pair if it is not already present
                        if (!potentialDoors.ContainsKey(connectedRooms))
                            potentialDoors.Add(connectedRooms, new List<Wall>());

                        if (neighborWall == null)
                        {
                            // If there is currently no wall between this tile and the neighbor, create on
                            Wall newWall = new Wall(WallType.NORMAL, tile);
                            tile.AddWall(i, newWall);
                            neighbor.AddWall((i + 3) % 6, newWall);

                            // Assign a random material from the list to the wall
                            newWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());
                            newWall.SetMaterialIndex(neighbor, roomThemes[neighbor.room.themeIndex].GetRandomWallMaterial());

                            // Add this wall to the list of potential doors
                            if (!tile.modificationLocked && !neighbor.modificationLocked && !neighbor.presetContained)
                                potentialDoors[connectedRooms].Add(newWall);
                        }
                        else
                        {
                            // If the neighbor already has a wall in this position, add it to this tile as well and connected them
                            tile.AddWall(i, neighborWall);

                            // Assign the wall a random material index
                            neighborWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());

                            // Add this wall to the list of potential doors
                            if (!tile.modificationLocked && !neighbor.modificationLocked && !neighbor.presetContained)
                                potentialDoors[connectedRooms].Add(neighborWall);
                        }
                    }
                    else if (neighbor == null)
                    {
                        Wall newWall = new Wall(WallType.NORMAL, tile);
                        tile.AddWall(i, newWall);

                        // Assign a random material from the list to the wall
                        newWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());
                    }
                }
            }

            // Go through all presets within the room
            foreach (PresetData pData in room.presets)
            {
                // If the room should be contained, add new walls
                if (roomPresets[pData.index].isContained)
                {
                    // Apply walls to each of the tiles within this preset
                    foreach (Tile tile in pData.tiles.Keys)
                    {
                        Vector3 tilePosition = tile.gridPosition;
                        Vector3[] nList = genMap.neighbors;

                        // Go through each neighbor
                        for (int i = 0; i < nList.Length; i++)
                        {
                            // Global neighbor shows tiles placed on the global map while local Neighbor
                            Tile neighbor = genMap.GetTileAtLocation(tilePosition + nList[i]);

                            // Check if there is a neighbor and whether it is within this prefab or not
                            if (neighbor != null && !pData.tiles.Keys.ToList().Contains(neighbor))
                            {
                                Room neighborRoom = neighbor.room;

                                // These walls should be stored as candidates for doors between rooms
                                Wall neighborWall = neighbor.walls[(i + 3) % 6];

                                if (neighborWall == null)
                                {
                                    // If there is currently no wall between this tile and the neighbor, create on
                                    Wall newWall = new Wall(WallType.NORMAL, tile);
                                    tile.AddWall(i, newWall);
                                    neighbor.AddWall((i + 3) % 6, newWall);

                                    // Assign a random material from the list to the wall
                                    newWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());
                                    newWall.SetMaterialIndex(neighbor, roomThemes[neighbor.room.themeIndex].GetRandomWallMaterial());

                                    // Check for several conditions
                                    // Is the tile NOT contained within a preset
                                    // Is the tile either the entry point OR a potential exit point
                                    if (!neighbor.presetContained && (pData.globalOrigin == tile.gridPosition || pData.exitPositions.Contains(tile.gridPosition)))
                                    {
                                        // Add the entry to the dictionary if it doesn't already exist
                                        if (!containedPresetWalls.ContainsKey(tile))
                                            containedPresetWalls.Add(tile, new List<Wall>());

                                        // Add this tile as a door candidate
                                        containedPresetWalls[tile].Add(newWall);
                                    }
                                }
                            }
                            else if (neighbor == null)
                            {
                                // If there is no neighbor on the map, place a wall
                                Wall newWall = new Wall(WallType.NORMAL, tile);
                                tile.AddWall(i, newWall);

                                // Assign a random material from the list to the wall
                                newWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());
                            }
                        }
                    }
                }
            }

            // Set this room's distance to 1 away from the minDistance
            room.roomDistance = minDistance + 1;
        }
        void SetDoors()
        {
            // Run through all the potential doors to choose random ones
            foreach (List<Wall> doorSpots in potentialDoors.Values)
            {
                // Door spots can sometimes yield no doors
                if (doorSpots.Count > 0)
                {
                    Wall newDoor = doorSpots[UnityEngine.Random.Range(0, doorSpots.Count)];
                    newDoor.SetType(WallType.DOOR);

                    foreach (Tile t in newDoor.GetConnectedTiles())
                    {
                        newDoor.SetMaterialIndex(t, roomThemes[t.room.themeIndex].GetRandomDoorMaterial());
                    }
                }
            }

            // Go through all preset exits and add doors
            foreach (List<Wall> walls in containedPresetWalls.Values)
            {
                Wall newDoor = walls[UnityEngine.Random.Range(0, walls.Count)];
                newDoor.SetType(WallType.DOOR);

                foreach (Tile t in newDoor.GetConnectedTiles())
                {
                    newDoor.SetMaterialIndex(t, roomThemes[t.room.themeIndex].GetRandomDoorMaterial());
                }
            }
        }
        void SetTileElements()
        {
            // Go through each room to determine where to put the tile elements
            foreach(Room r in genMap.GetRooms())
            {
                // Initialize lists/dictionaries to hold valid placements for each element placement type
                List<Tile> groundTiles = new List<Tile>();
                List<Tile> ceilingTiles = new List<Tile>();
                Dictionary<Tile, List<int>> wallTiles = new Dictionary<Tile, List<int>>();

                // Run through all tiles in the room and add to their respective lists
                foreach(Tile t in r.GetTiles())
                {
                    // Skip tiles that are modification locked as they cannot be changed
                    if (t.modificationLocked)
                        continue;

                    // Ground tiles need to be on a tile with a floor
                    if(t.type != TileType.EMPTY)
                        groundTiles.Add(t);
                    
                    // Ceiling tiles can almost exist on anything, but obviously need a ceiling
                    if (t.ceiling != null)
                        ceilingTiles.Add(t);

                    // The wall list will have references by tile, but also contain each wall within that tile
                    List<int> wallCheck = new List<int>();
                    for(int i = 0; i < t.walls.Length; i++)
                    {
                        // If there is a wall on the index, add it to the list
                        if (t.walls[i] != null && t.walls[i].type != WallType.DOOR && t.walls[i].type != WallType.CUSTOM)
                        {
                            wallCheck.Add(i);
                        }
                    }
                    if (wallCheck.Count > 0)
                        wallTiles[t] = wallCheck;
                }

                // Choose what elements to place
                for(int i = 0;i < 5; i++)
                {
                    PlaceTileElement(spawner);
                    PlaceTileElement(decoElements[0]);
                }

                // Used due to frequent reference above
                void PlaceTileElement(TileElement tElement)
                {
                    Tile t = null;

                    // Use different logic based on the type of element
                    switch (tElement.placeType)
                    {
                        case TileElement.PlacementType.GROUND:
                            t = groundTiles[UnityEngine.Random.Range(0, groundTiles.Count)];
                            t.elements[6] = tElement;
                            groundTiles.Remove(t);
                            break;
                        case TileElement.PlacementType.WALL:
                            // Get a random tile that will have a wall taken from it
                            Tile[] tKeys = wallTiles.Keys.ToArray();
                            t = tKeys[UnityEngine.Random.Range(0, wallTiles.Keys.Count)];

                            // Get a random wall on tile
                            List<int> walls = wallTiles[t];
                            int randWall = walls[UnityEngine.Random.Range(0, walls.Count)];

                            // Set the corresponding variables and remove this wall from eligibility
                            t.elements[randWall] = tElement;
                            wallTiles[t].Remove(randWall);

                            // Remove the tile if it has no more walls to place on
                            if(wallTiles[t].Count == 0)
                            {
                                wallTiles.Remove(t);
                            }
                            break;
                        case TileElement.PlacementType.CEILING:
                            t = ceilingTiles[UnityEngine.Random.Range(0, ceilingTiles.Count)];
                            t.elements[7] = tElement;
                            ceilingTiles.Remove(t);
                            break;
                    }
                }
            }
        }

        // These functions just help with a bunch of stuff for tile checking
        bool PositionOpen(Vector3 pos, Room room = null)
        {
            /*
             * Check for 3 conditions
             * - Is this position within the bounds of the map
             * - Is this position occupied on the current version map
             * - (Optional) Is this position occupied in the currently generating room
            */

            if (room == null)
                return VectorInMap(pos) && genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] == null;
            return VectorInMap(pos) && genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] == null && room.GetTileAtLocation(pos) == null;
        }
        bool VectorInMap(Vector3 pos)
        {
            return
            (
                pos.x >= 0 && pos.x < genMap.mapBounds.x &&
                pos.y >= 0 && pos.y < genMap.mapBounds.y &&
                pos.z >= 0 && pos.z < genMap.mapBounds.z
            );
        }
    }
    private struct RoomPair
    {
        private Room room1;
        private Room room2;

        public RoomPair(Room room1, Room room2)
        {
            this.room1 = room1;
            this.room2 = room2;
        }

        public override bool Equals(object obj)
        {
            RoomPair other = (RoomPair)obj;
            return (room1.index == other.room2.index && room2.index == other.room1.index) || (room1.index == other.room1.index && room2.index == other.room2.index);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(room1, room2);
        }
    }

    public void BuildMap(Map map)
    {
        // Get some of the properties of this map for use later
        float tileRadius = Map.TILE_RADIUS;
        float tileSideDistance = Map.TILE_SIDE_DISTANCE;
        float wallWidthOffset = (Map.WALL_THICKNESS / 2 / Mathf.Sqrt(3)) * 2;

        // Create an empty gameobject to store all of the instantiate things
        GameObject mapParent = new GameObject("Map");
        
        // Store the necessary nav mesh builder for use after the map is finished being made
        NavMeshSurface navMesh = mapParent.AddComponent<NavMeshSurface>();
        navMesh.collectObjects = CollectObjects.Children;

        // Used to keep track of all spawners that are placed so that they can be initialized after the nav mesh is done
        List<EnemySpawner> spawners = new List<EnemySpawner>();
        
        // This stores walls that have already been placed since the walls link between multiple tiles
        List<Wall> placedWalls = new List<Wall>();

        // Loop through all rooms in the map
        List<Room> rooms = map.GetRooms();
        for( int i = 0; i < rooms.Count; i++)
        {
            // Create the object that will house all the instantiated objects
            GameObject roomParent = new GameObject("Room " + i);
            rooms[i].obj = roomParent;
            roomParent.transform.parent = mapParent.transform;

            // Get the list of tiles from this room
            List<Tile> tiles = rooms[i].GetTiles();

            // Spawn the presets within the given room
            for (int j = 0; j < rooms[i].presets.Count; j++)
            {
                // Instantiate the prefab that needs to be created
                PresetData data = rooms[i].presets[j];
                GameObject presetParent = Instantiate(roomPresets[data.index].obj, roomParent.transform);
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
                BuildTileElements(tiles[j]);
            }

            // Generate the wall and tile mesh for this room
            GenerateFloorMesh(rooms[i]);
            GenerateWallMesh(rooms[i]);
            GenerateCeilingMesh(rooms[i]);
        }

        mapParent.transform.localScale *= MAP_SCALE;

        // Create the nav mesh for the map
        navMesh.BuildNavMesh();

        // Activate all spawners
        foreach(EnemySpawner s in spawners)
        {
            s.Initialize();
        }

        // Open the first room
        map.rooms[0].SetOpenStatus(true);

        void BuildTile(Tile tile, GameObject roomParent)
        {
            // Check the type of the given tile
            if(tile.type == TileType.NORMAL)
            {
                // Create a tile from the given prefab
                GameObject tileObject = Instantiate(floorPrefabNormal);
                tileObject.transform.parent = roomParent.transform;
                Vector3 tileGridPosition = tile.gridPosition;

                // Set the transform of the tile to match the map size
                tileObject.transform.position = new Vector3
                (
                    tileGridPosition.x * (tileRadius * 1.5f),
                    tileGridPosition.y * Map.FLOOR_HEIGHT,
                    tileGridPosition.z * tileSideDistance
                );
                tileObject.transform.rotation = Quaternion.identity;

                // Set the size of all the colliders in the prefab
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
                // Create an empty gameobject to store the walls
                GameObject tileObject = new GameObject();
                tileObject.transform.parent = roomParent.transform;
                Vector3 tileGridPosition = tile.gridPosition;

                // Set the transform as necessary
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

            // Add the ceiling for this tile
            if(tile.ceiling != null)
            {
                Ceiling ceiling = tile.ceiling;

                // Add a ceiling collider if there is no tile above this one
                if (ceiling.hasCollider)
                {
                    // Create a tile from the given prefab
                    GameObject ceilingObject = Instantiate(floorPrefabNormal);
                    ceilingObject.transform.parent = tile.obj.transform;

                    // Set the transform of the ceiling to match the tile
                    ceilingObject.transform.position = tile.obj.transform.position + Vector3.up * Map.FLOOR_HEIGHT;
                    ceilingObject.transform.rotation = tile.obj.transform.rotation;

                    // Set the size of all the colliders in the prefab
                    foreach (Collider c in ceilingObject.GetComponentsInChildren<Collider>())
                    {
                        c.transform.localScale = new Vector3
                        (
                            c.transform.localScale.x * Map.TILE_RADIUS,
                            c.transform.localScale.y * Map.FLOOR_THICKNESS,
                            c.transform.localScale.z * Map.TILE_RADIUS
                        );
                    }
                    ceilingObject.name = "Ceiling " + tile.gridPosition.ToString();
                }
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

                    // Make sure that there is actually a wall at this position
                    if(wall != null)
                    {
                        if (wall.type == WallType.NORMAL)
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
                        else if (wall.type == WallType.DOOR)
                        {
                            // Create the wall object
                            GameObject doorObject = Instantiate(doorPrefab, tileObject.transform);
                            doorObject.name = "Door " + k;

                            // This size mod will adjust the size of the walls on the preset to account for the change in size of the preset
                            float sizeMod = tile.type == TileType.CUSTOM ? Map.TILE_RADIUS : 1;

                            // Set the transform of the wall
                            doorObject.transform.localScale = new Vector3(Map.TILE_RADIUS + wallWidthOffset * Map.TILE_RADIUS, Map.FLOOR_HEIGHT, Map.WALL_THICKNESS * Map.TILE_RADIUS) / sizeMod;
                            doorObject.transform.position = new Vector3
                            (
                                map.hexagonExteriorSidePositions[k].x + tile.obj.transform.position.x,
                                tile.obj.transform.position.y + Map.FLOOR_HEIGHT / 2,
                                map.hexagonExteriorSidePositions[k].y + tile.obj.transform.position.z
                            );
                            doorObject.transform.rotation = Quaternion.Euler(new Vector3(0, k * 60, 0));

                            // Set the values for this door
                            DoorController doorCont = doorObject.GetComponent<DoorController>();
                            Material[] mats = new Material[2];

                            // Get the materials for this door
                            for (int i = 0; i < mats.Length; i++)
                            {
                                Tile t = wall.connectedTiles[i];
                                mats[i] = roomThemes[t.room.themeIndex].GetDoorMaterialArray()[wall.materialIndexes[t]];
                            }

                            // Set the Door's materials
                            doorCont.SetMaterials(mats[0], mats[1]);
                            // Send over the room with the lowest distance
                            doorCont.Initialize(Mathf.Min(wall.connectedTiles[0].room.roomDistance, wall.connectedTiles[1].room.roomDistance), wall);

                            // Assign this object to the wall data type
                            tile.walls[k].obj = doorObject;

                            // Add this wall to the list of placed walls
                            placedWalls.Add(wall);
                        }
                    }
                }
            }
        }
        void BuildTileElements(Tile tile)
        {
            // Find the elements that are present on the given tile
            TileElement[] elements = tile.elements;

            // Run through each potential tile element to determine if any are present
            for (int k = 0;k < elements.Length; k++)
            {
                if (elements[k] != null)
                {
                    GameObject eObj = null;

                    // Inidicates that it is on a wall
                    switch (k)
                    {
                        // Any of the walls
                        case <= 5:
                            eObj = Instantiate(elements[k].prefab, tile.obj.transform);

                            // This size mod will adjust the size of the walls on the preset to account for the change in size of the preset
                            float sizeMod = tile.type == TileType.CUSTOM ? Map.TILE_RADIUS : 1;

                            // Set the transform of the wall
                            //eObj.transform.localScale = new Vector3(Map.TILE_RADIUS + wallWidthOffset * Map.TILE_RADIUS, Map.FLOOR_HEIGHT, Map.WALL_THICKNESS * Map.TILE_RADIUS) / sizeMod;
                            eObj.transform.position = new Vector3
                            (
                                tile.obj.transform.position.x + map.hexagonExteriorSidePositions[k].x - (map.hexagonExteriorSidePositions[k].x * Map.WALL_THICKNESS),
                                tile.obj.transform.position.y + Map.FLOOR_THICKNESS,
                                tile.obj.transform.position.z + map.hexagonExteriorSidePositions[k].y - (map.hexagonExteriorSidePositions[k].y * Map.WALL_THICKNESS)
                            );
                            eObj.transform.rotation = Quaternion.Euler(new Vector3(0, (k + 3) * 60, 0));
                            break;
                        // Floor
                        case 6:
                            eObj = Instantiate(elements[k].prefab, tile.obj.transform);
                            eObj.transform.position = new Vector3
                            (
                                tile.obj.transform.position.x,
                                tile.obj.transform.position.y + Map.FLOOR_THICKNESS,
                                tile.obj.transform.position.z
                            );
                            eObj.transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 359), 0));
                            break;
                        // Ceiling
                        case 7:
                            eObj = Instantiate(elements[k].prefab, tile.obj.transform);
                            eObj.transform.position = new Vector3
                            (
                                tile.obj.transform.position.x,
                                tile.obj.transform.position.y + Map.FLOOR_HEIGHT,
                                tile.obj.transform.position.z
                            );
                            eObj.transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 359), 0));
                            break;
                    }

                    // Extra functionality that may be required for some Tile Elements (spawners etc
                    if (elements[k].id == spawner.id)
                    {
                        // Add this to the list of spawners to be initialized
                        spawners.Add(eObj.GetComponent<EnemySpawner>());
                        tile.room.AddSpawner(eObj.GetComponent<EnemySpawner>());
                    }
                }
            }
        }

        // Generates combined meshes for each respective part of the map
        Mesh GenerateWallMesh(Room room)
        {
            // Create a 2D list of combine instances that
            List<CombineInstance>[] materialGroups = new List<CombineInstance>[roomThemes[room.themeIndex].GetWallMaterialArray().Length];

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

                    newInstance.mesh = wallMesh;

                    // Set the "transform" of the mesh which is basically just the transform of the wall
                    Vector3 scale = new Vector3((1 + wallWidthOffset) * tileRadius, Map.FLOOR_HEIGHT, Map.WALL_THICKNESS * Map.TILE_RADIUS / 2);
                    Vector2 normalized = map.hexagonExteriorSidePositions[i].normalized;
                    Vector3 pos = new Vector3
                    (
                        wallObject.transform.position.x - (normalized.x * (Map.WALL_THICKNESS / (4 / Map.TILE_RADIUS))),
                        wallObject.transform.position.y,
                        wallObject.transform.position.z - (normalized.y * (Map.WALL_THICKNESS / (4 / Map.TILE_RADIUS)))
                    );
                    Quaternion rot = Quaternion.Euler(new Vector3(0, i * 60 + 180, 0));

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
            MeshFilter filter = testObj.AddComponent<MeshFilter>();
            MeshRenderer rend = testObj.AddComponent<MeshRenderer>();

            filter.mesh = combinedMesh;
            rend.materials = roomThemes[room.themeIndex].GetWallMaterialArray();
            rend.receiveShadows = false;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return combinedMesh;
        }
        Mesh GenerateFloorMesh(Room room)
        {
            // Create a 2D list of combine instances that
            List<CombineInstance>[] materialGroups = new List<CombineInstance>[roomThemes[room.themeIndex].GetFloorMaterialArray().Length];

            // Loop through all tiles within the given room
            foreach (Tile tile in room.GetTiles())
            {
                // Exclude non normal tiles from generation
                if (tile.type != TileType.NORMAL)
                    continue;

                CombineInstance newInstance = new CombineInstance();

                GameObject tileObject = tile.obj;
                newInstance.mesh = hexMesh;

                // Get the transform of the tile
                Vector3 scale = new Vector3(tileObject.transform.localScale.x * Map.TILE_RADIUS, tileObject.transform.localScale.y / 2 * (Map.FLOOR_THICKNESS / 2), tileObject.transform.localScale.z * Map.TILE_RADIUS);
                Vector3 pos = tileObject.transform.position + (Map.FLOOR_THICKNESS / 4 * Vector3.up);
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
            GameObject testObj = new GameObject("Static Floor Mesh");
            testObj.transform.parent = room.obj.transform;
            MeshFilter filter = testObj.AddComponent<MeshFilter>();
            MeshRenderer rend = testObj.AddComponent<MeshRenderer>();

            filter.mesh = combinedMesh;
            rend.materials = roomThemes[room.themeIndex].GetFloorMaterialArray();
            rend.receiveShadows = false;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return combinedMesh;
        }
        Mesh GenerateCeilingMesh(Room room)
        {
            // Create a 2D list of combine instances that
            List<CombineInstance>[] materialGroups = new List<CombineInstance>[roomThemes[room.themeIndex].GetCeilingMaterialArray().Length];

            // Loop through all tiles within the given room
            foreach (Tile tile in room.GetTiles())
            {
                Ceiling ceil = tile.ceiling;

                if (ceil == null)
                    continue;

                CombineInstance newInstance = new CombineInstance();

                GameObject tileObject = tile.obj;
                newInstance.mesh = hexMesh;

                // Get the transform of the tile
                Vector3 scale = new Vector3(tileObject.transform.localScale.x * Map.TILE_RADIUS, tileObject.transform.localScale.y / 2 * (Map.FLOOR_THICKNESS / 2), tileObject.transform.localScale.z * Map.TILE_RADIUS);
                Vector3 pos = tileObject.transform.position + ((Map.FLOOR_HEIGHT - Map.FLOOR_THICKNESS / 4) * Vector3.up);
                //Quaternion rot = tileObject.transform.rotation;
                Quaternion rot = Quaternion.Euler
                    (
                        tileObject.transform.rotation.eulerAngles.x + 180,
                        tileObject.transform.rotation.eulerAngles.y,
                        tileObject.transform.rotation.eulerAngles.z
                    );

                newInstance.transform = Matrix4x4.TRS(pos, rot, scale);

                // Add this mesh to the appropriate group for submesh creation
                if (materialGroups[ceil.materialIndex] == null)
                    materialGroups[ceil.materialIndex] = new List<CombineInstance>();
                materialGroups[ceil.materialIndex].Add(newInstance);
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
            GameObject testObj = new GameObject("Static Ceiling Mesh");
            testObj.transform.parent = room.obj.transform;
            MeshFilter filter = testObj.AddComponent<MeshFilter>();
            MeshRenderer rend = testObj.AddComponent<MeshRenderer>();

            filter.mesh = combinedMesh;
            rend.materials = roomThemes[room.themeIndex].GetCeilingMaterialArray();
            rend.receiveShadows = false;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return combinedMesh;
        }
    }

    private void OnValidate()
    {
        // Run through the room themes and calculate their totals for weighting
        foreach(RoomTheme r in roomThemes)
        {
            r.CalculateMaterialWeightTotals();
        }
    }
}