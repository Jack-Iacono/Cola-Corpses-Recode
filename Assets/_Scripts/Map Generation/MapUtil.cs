using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEngine;

using static MapPreset;
using static UnityEditor.PlayerSettings;

namespace MapUtil
{
    public enum TileType { NORMAL, PRESET, EMPTY }
    public enum WallType { NORMAL, DOOR, HALF, PRESET }

    public class Map
    {
        public List<Room> rooms { get; private set; } = new List<Room>();
        public List<Room> bonusRooms { get; private set; } = new List<Room>();
        public Tile[,,] tiles;

        public Vector3 mapBounds { get; private set; } = new Vector3(50, 1, 50);

        // These are the parameters for what a normal map's proportions look like
        public const float TILE_RADIUS = 2f;
        public const float FLOOR_HEIGHT = 3;
        public const float WALL_THICKNESS = 0.05f;
        public const float FLOOR_THICKNESS = 0.05f;

        // The length from the center of the hexagon to the middle of any side
        public static readonly float TILE_SIDE_DISTANCE = Mathf.Sqrt((TILE_RADIUS * TILE_RADIUS) - ((TILE_RADIUS/2) * (TILE_RADIUS/2)));

        // These two lists contain which cells will be neighbors for a cell depending on their x position
        // This is needed since the rows are not linear and offset between eachother
        public readonly Vector3[] neighbors = new Vector3[]
        {
            new Vector3(0,0,2),
            new Vector3(1,0, 1),
            new Vector3(1,0,-1),
            new Vector3(0,0,-2),
            new Vector3(-1,0,-1),
            new Vector3(-1,0,1)
        };

        // Initialized later due to initializationg of hexShortLength
        // Contains the points for the middle of each side as well as for each vertex
        public Vector2[] hexagonVertexes { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonExteriorSidePositions { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonInteriorSidePositions { get; private set; } = new Vector2[] { };

        public Map(Vector3 bounds)
        {
            mapBounds = bounds;

            // The length from the center of the hexagon to the mid point of any side
            //TILE_SIDE_DISTANCE = Mathf.Sqrt((TILE_RADIUS * TILE_RADIUS) - (TILE_RADIUS / 2) * (TILE_RADIUS / 2));

            // Initialize the array for storing the tiles being used
            tiles = new Tile[(int)mapBounds.x, (int)mapBounds.y, (int)mapBounds.z];

            // Vertexes labelled clockwise starting at the 2 o'clock position
            hexagonVertexes = new Vector2[]
            {
                new Vector2(TILE_RADIUS / 2, TILE_SIDE_DISTANCE),
                new Vector2(TILE_RADIUS, 0),
                new Vector2(TILE_RADIUS / 2, -TILE_SIDE_DISTANCE),
                new Vector2(-TILE_RADIUS / 2,-TILE_SIDE_DISTANCE),
                new Vector2(-TILE_RADIUS, 0),
                new Vector2(-TILE_RADIUS / 2,TILE_SIDE_DISTANCE),
            };
            // Sides are labelled clockwise starting at the top side (12 o'clock)
            hexagonExteriorSidePositions = new Vector2[]
            {
                new Vector2(0,TILE_SIDE_DISTANCE),
                new Vector2((hexagonVertexes[0].x + hexagonVertexes[1].x) / 2, (hexagonVertexes[0].y + hexagonVertexes[1].y) / 2),
                new Vector2((hexagonVertexes[1].x + hexagonVertexes[2].x) / 2, (hexagonVertexes[1].y + hexagonVertexes[2].y) / 2),
                new Vector2(0,-TILE_SIDE_DISTANCE),
                new Vector2((hexagonVertexes[3].x + hexagonVertexes[4].x) / 2, (hexagonVertexes[3].y + hexagonVertexes[4].y) / 2),
                new Vector2((hexagonVertexes[4].x + hexagonVertexes[5].x) / 2, (hexagonVertexes[4].y + hexagonVertexes[5].y) / 2)
            };
            // Sides labelled clockwise starting at the 2 o'clock position
            hexagonInteriorSidePositions = new Vector2[]
            {
                new Vector2(hexagonVertexes[0].x/ 2, hexagonVertexes[0].y / 2),
                new Vector2(TILE_RADIUS / 2, 0),
                new Vector2(hexagonVertexes[2].x/ 2, hexagonVertexes[2].y / 2),
                new Vector2(hexagonVertexes[3].x/ 2, hexagonVertexes[3].y / 2),
                new Vector2(-TILE_RADIUS / 2, 0),
                new Vector2(hexagonVertexes[5].x/ 2, hexagonVertexes[5].y / 2)
            };
        }

        public List<Room> GetRooms()
        {
            return rooms;
        }
        public List<Room> GetBonusRooms() 
        {
            return bonusRooms; 
        }

        public Tile GetTileAtLocation(Vector3 pos)
        {
            return VectorInBounds(pos) ? tiles[(int)pos.x, (int)pos.y, (int)pos.z] : null;
        }
        public bool VectorInBounds(Vector3 pos)
        {
            return
            (
                pos.x >= 0 && pos.x < mapBounds.x &&
                pos.y >= 0 && pos.y < mapBounds.y &&
                pos.z >= 0 && pos.z < mapBounds.z
            );
        }
    }

    public class Room
    {
        private Dictionary<Vector3, Tile> tiles = new Dictionary<Vector3, Tile>();
        private List<Tile> doorTiles = new List<Tile>();

        public GameObject obj;

        public int themeIndex = 0;

        public List<PresetData> presets = new List<PresetData>();

        public Room()
        {

        }

        public List<Tile> GetTiles()
        {
            return new List<Tile>(tiles.Values.ToList());
        }

        public Tile GetTileAtLocation(Vector3 pos)
        {
            if(tiles.ContainsKey(pos))
                return tiles[pos];
            else
                return null;
        }

        public void AddTile(Tile tile)
        {
            tiles.Add(tile.gridPosition, tile);
        }
    }

    public class Tile
    {
        public Vector3 gridPosition = Vector3.zero;
        public Room room;
        public Wall[] walls = new Wall[6];

        public GameObject obj = null;

        public TileType type { get; private set; } = TileType.NORMAL;

        public int materialIndex = 0;

        public Tile(Vector3 gridPosition, Room room)
        {
            this.gridPosition = gridPosition;
            this.room = room;
        }

        public Wall[] GetWalls()
        {
            return walls;
        }

        public void AddWall(int index, Wall wall)
        {
            walls[index] = wall;
            if (!walls[index].GetConnectedTiles().Contains(this))
                walls[index].AddConnectedTile(this);
        }
        public void RemoveWall(int index)
        {
            if (walls[index] != null)
            {
                walls[index].RemoveConnectedTile(this);
                walls[index] = null;
            }
        }

        public void SetType(TileType type)
        {
            this.type = type;
        }

        public void SetMaterialIndex(int index)
        {
            materialIndex = index;
        }

        public override bool Equals(object obj)
        {
            Tile t = obj as Tile;
            return t.gridPosition == gridPosition;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(gridPosition.GetHashCode());
        }
    }
    public class Wall
    {
        public WallType type {  get; private set; }
        public GameObject obj = null;

        public Dictionary<Tile, int> materialIndexes = new Dictionary<Tile, int>();

        public List<Tile> connectedTiles { get; private set; } = new List<Tile>();

        public Wall(WallType type, Tile connectedTile)
        {
            this.type = type;
            this.connectedTiles.Add(connectedTile);
        }

        public void AddConnectedTile(Tile tile)
        {
            if (!connectedTiles.Contains(tile))
            {
                connectedTiles.Add(tile);
            }
        }
        public void RemoveConnectedTile(Tile tile)
        {
            if(connectedTiles.Count > 1 && connectedTiles.Contains(tile))
            {
                connectedTiles.Remove(tile);
                materialIndexes.Remove(tile);
            }
        }

        public void SetMaterialIndex(Tile tile, int index)
        {
            if (materialIndexes.ContainsKey(tile))
                materialIndexes[tile] = index;
            else
                materialIndexes.Add(tile, index);
        }

        public List<Tile> GetConnectedTiles()
        {
            return connectedTiles;
        }
    }   

    public static class MapGenerator
    {
        // While this is not technically null, the graph that generates the points will never have to use this position
        private static readonly Vector3 NULL_VECTOR = new Vector3(-1, -1, -1);
        private const float floorChangeChance = 0.01f;

        public static Map Generate(Vector3 bounds, int roomCount, Vector2 roomTileRange, List<MapPreset> roomPresets, List<RoomTheme> roomThemes)
        {
            // The map will store all the placed tiles and rooms
            Map genMap = new Map(bounds);

            // Dead zones and roomGenPaths allow the generator to correctly generate rooms with full exploration
            List<Vector3> deadZones = new List<Vector3>();
            List<List<Vector3>> roomGenPaths = new List<List<Vector3>>();

            // Use this list to determine the indices of all presets that can be used as stairs
            List<int> stairPresetIndices = new List<int>();
            for(int i = 0; i < roomPresets.Count; i++)
            {
                if (roomPresets[i].isStair)
                    stairPresetIndices.Add(i);
            }

            // Get the mid point of the map bounds
            Vector3 currentLocation = new Vector3(Mathf.FloorToInt(genMap.mapBounds.x / 2), Mathf.FloorToInt(genMap.mapBounds.y / 2), Mathf.FloorToInt(genMap.mapBounds.z / 2));

            // Create the desired amount of rooms
            for (int i = 0; i < roomCount; i++)
            {
                // Create a new room to hold the tiles, but don't add it to the map yet in case it doesn't generate fully
                Room newRoom = GenerateNormalRoom(i);
                if(newRoom != null)
                {
                    // Add each tile to the tile map for later access and comparison
                    foreach (Tile t in newRoom.GetTiles())
                    {
                        Vector3 pos = t.gridPosition;
                        // Change this to uninclude presets later
                        t.SetMaterialIndex(UnityEngine.Random.Range(0, roomThemes[newRoom.themeIndex].floorMaterials.Count));
                        genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] = t;
                    }
                }
            }

            return genMap;

            // The below functions are used purely for organizational purposes
            Room GenerateNormalRoom(int roomIndex)
            {
                // Create the new Room to hold the data needed
                Room newRoom = new Room();

                // Assign a theme to the room randomly from the list of themes
                newRoom.themeIndex = UnityEngine.Random.Range(0, roomThemes.Count);

                // Get a temporary clone of the map tiles for modification here
                List<Vector3> genPath = new List<Vector3>();

                // The amount of tiles that should be generated for this room
                int roomTileGoal = UnityEngine.Random.Range((int)roomTileRange.x, (int)roomTileRange.y);

                // Generate the tiles in the given room
                for (int j = 0; j < roomTileGoal; j++)
                {
                    // Add this tile to the path of generated tiles
                    genPath.Add(currentLocation);
                    Vector3 nextTilePosition = NULL_VECTOR;

                    if(UnityEngine.Random.Range(0,1f) < floorChangeChance)
                    {
                        // Get a random stair preset
                        //PresetData preset = GetPresetFrom(stairPresetIndices, currentLocation, genPath, newRoom);
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

                            // If the room had an exit point, get a random one
                            if (preset.exitPositions.Count > 0)
                                genPath.Add(preset.exitPositions[UnityEngine.Random.Range(0, preset.exitPositions.Count)]);
                        }
                    }
                    else
                    {
                        // create a new tile at this location
                        Tile newTile = new Tile(currentLocation, newRoom);
                        newRoom.AddTile(newTile);
                    }

                    // Get next tile in the map
                    nextTilePosition = GetNextTile(genPath.Last(), ref genPath, newRoom);
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
                        return null;

                    // Loop through the whole map to find a new starting location
                    Vector3 nextLocation = BacktrackMap();
                    
                    // This will run if no previous rooms have any open tiles at all, in which case the map is dead and done
                    if (nextLocation == NULL_VECTOR)
                        return null;

                    // Set the current location and generate a room at the new location
                    currentLocation = nextLocation;
                    return GenerateNormalRoom(roomIndex);
                }
                else
                {
                    // Since the room was built correctly, generate the walls
                    SetRoomWalls(newRoom);
                }

                // Add the current room to the map since it generated correctly
                genMap.rooms.Add(newRoom);
                currentLocation = BacktrackRoom(ref genPath, newRoom);
                roomGenPaths.Add(genPath);

                return newRoom;
            }

            Vector3 GetNextTile(Vector3 current, ref List<Vector3> roomGenPath, Room room = null)
            {
                Vector3 nextTilePosition = GetRandomValidNeighbor(roomGenPath.Last(), room);
                if (nextTilePosition == NULL_VECTOR)
                {
                    // Remove this tile from the path since it is invlaid due to not having a neighbor
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
                if (validVerticalPosition.Count > 0 && (UnityEngine.Random.Range(0, 1f) < floorChangeChance || validHorizontalPositions.Count == 0))
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
                    if(neighbor != NULL_VECTOR)
                        return neighbor;
                }

                // THERE IS NOTHING AVAILABLE
                return NULL_VECTOR;
            }

            PresetData GetPreset(int presetIndex, Vector3 origin, List<Vector3> path, Room room)
            {
                // Get the list of valid presets for the location
                // if the user enters -1 for the preset, a random preset will be chosen
                List<PresetData> validPresets = presetIndex != -1 ? GetValidPresetRotations(presetIndex, origin, room) : GetValidPresets(origin,room);

                // Check if there are presets to choose from
                if (validPresets == null || validPresets.Count == 0)
                {
                    Vector3 nextNeighbor = BacktrackRoom(ref path, room);
                    if (nextNeighbor != NULL_VECTOR)
                    {
                        path.RemoveAt(path.Count - 1);
                        return GetPreset(presetIndex, nextNeighbor, path, room);
                    }
                    else
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
                while(presets.Count > 0)
                {
                    // Get a random preset index and remove it from the list
                    int index = UnityEngine.Random.Range(0, presets.Count);
                    presets.RemoveAt(index);

                    // Get the valid rotations of this preset at the position
                    List<PresetData> validPresets = GetValidPresetRotations(index, presetStart, room);

                    // Check to see if this preset has any valid rotations
                    if(validPresets.Count > 0)
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
                foreach (Vector3 pivotTile in preset.entryPoints)
                {
                    // Check every rotation of the room in respect to the entrypoint
                    for (int i = 0; i < 6; i++)
                    {
                        // Create an array to store all the tile gridPositions
                        Dictionary<Tile,Vector3> presetTiles = new Dictionary<Tile, Vector3>();
                        List<Vector3> exitPositions = new List<Vector3>();

                        // Loop through each tile in the preset for the given rotation
                        for (int j = 0; j < tiles.Count; j++)
                        {
                            // Get the local position of the tile rotated around the origin
                            Vector3 rotPosition = CubeCoord.GetRotatedPosition(tiles[j].gridPosition - pivotTile, Vector3.zero, i * 60);

                            // Denote this space as a potential exit point for the preset so that the generation can continue through it
                            if (preset.entryPoints.Contains(tiles[j].gridPosition) && tiles[j].gridPosition != pivotTile)
                            {
                                exitPositions.Add(rotPosition + origin);
                            }

                            // Check if the global position of this tile is open and add it if it is
                            if (PositionOpen(rotPosition + origin, room))
                            {
                                // Create the new tile
                                Tile newTile = new Tile(rotPosition + origin, room);

                                // Need a way to send transform of tile to the builder, maybe through coordinates
                                if (tiles[j].isEmpty)
                                    newTile.SetType(TileType.EMPTY);
                                else if (tiles[j].tileObject == null)
                                    newTile.SetType(TileType.NORMAL);
                                else
                                    newTile.SetType(TileType.PRESET);


                                presetTiles.Add(newTile, tiles[j].gridPosition);
                            }
                            else
                                // If this position is not open, break the loop since this preset can't be used
                                break;
                        }

                        // Check to see if all tiles cleared, if so, add this to the list of valid presets
                        if (presetTiles.Count == tiles.Count)
                        {
                            validPresets.Add(new PresetData(pivotTile, origin, i*60, presetIndex, presetTiles, exitPositions));
                        }
                    }
                }

                return validPresets;
            }

            void SetRoomWalls(Room room)
            {
                // Run through all tiles and place the necessary walls
                // This works since this is the minimum walls a preset could have as well
                foreach(Tile t in room.GetTiles())
                {
                    SetNormalTileWalls(t, room);
                }

                // Go through all presets within the room
                foreach(PresetData pData in room.presets)
                {
                    // If the room should be contained, add new walls
                    if (roomPresets[pData.index].isContained)
                    {
                        // Apply walls to each of the tiles within this preset
                        foreach (Tile tile in pData.tiles.Keys)
                        {
                            SetIsolatedTileWalls(tile, pData, room);
                        }
                    }
                }
            }
            void SetNormalTileWalls(Tile tile, Room room)
            {
                Vector3 tilePosition = tile.gridPosition;
                Vector3[] nList = genMap.neighbors;

                // Cuts out the last two neighbors (the vertical neighbors)
                for (int i = 0; i < nList.Length; i++)
                {
                    // Global neighbor shows tiles placed on the global map while local Neighbor
                    Tile globalNeighbor = genMap.GetTileAtLocation(tilePosition + nList[i]);
                    Tile localNeighbor = room.GetTileAtLocation(tilePosition + nList[i]);

                    // If there is a neighbor in another room
                    if (globalNeighbor != null)
                    {
                        // These walls should be stored as candidates for doors between rooms
                        Wall neighborWall = globalNeighbor.walls[(i + 3) % 6];
                        if (neighborWall == null)
                        {
                            // If there is currently no wall between this tile and the neighbor, create on
                            Wall newWall = new Wall(WallType.NORMAL, tile);
                            tile.AddWall(i, newWall);
                            globalNeighbor.AddWall((i + 3) % 6, newWall);

                            // Assign a random material from the list to the wall
                            newWall.SetMaterialIndex(tile, UnityEngine.Random.Range(0, roomThemes[room.themeIndex].wallMaterials.Count));
                            newWall.SetMaterialIndex(globalNeighbor, UnityEngine.Random.Range(0, roomThemes[globalNeighbor.room.themeIndex].wallMaterials.Count));
                        }
                        else
                        {
                            // If the neighbor already has a wall in this position, add it to this tile as well and connected them
                            tile.AddWall(i, neighborWall);

                            // Assign the wall a random material index
                            neighborWall.SetMaterialIndex(tile, UnityEngine.Random.Range(0, roomThemes[room.themeIndex].wallMaterials.Count));
                        }
                    }
                    else if (localNeighbor == null)
                    {
                        Wall newWall = new Wall(WallType.NORMAL, tile);
                        tile.AddWall(i, newWall);

                        // Assign a random material from the list to the wall
                        newWall.SetMaterialIndex(tile, UnityEngine.Random.Range(0, roomThemes[room.themeIndex].wallMaterials.Count));
                    }
                    else
                    {
                        localNeighbor.RemoveWall((i + 3) % 6);
                    }
                }
            }
            void SetIsolatedTileWalls(Tile tile, PresetData pData, Room room)
            {
                Vector3 tilePosition = tile.gridPosition;
                Vector3[] nList = genMap.neighbors;

                // Cuts out the last two neighbors (the vertical neighbors)
                for (int i = 0; i < nList.Length; i++)
                {
                    // Global neighbor shows tiles placed on the global map while local Neighbor
                    Tile localNeighbor = room.GetTileAtLocation(tilePosition + nList[i]);

                    if(localNeighbor != null && !pData.tiles.Keys.ToList().Contains(localNeighbor))
                    {
                        // If there is currently no wall between this tile and the neighbor, create on
                        Wall newWall = new Wall(WallType.NORMAL, tile);
                        tile.AddWall(i, newWall);
                        localNeighbor.AddWall((i + 3) % 6, newWall);

                        // Assign a random material from the list to the wall
                        newWall.SetMaterialIndex(tile, UnityEngine.Random.Range(0, roomThemes[room.themeIndex].wallMaterials.Count));
                        newWall.SetMaterialIndex(localNeighbor, UnityEngine.Random.Range(0, roomThemes[localNeighbor.room.themeIndex].wallMaterials.Count));
                    }
                }
            }

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

    }

    public class PresetData
    {
        public int rotation;
        public int index;

        public Vector3 localOrigin;
        public Vector3 globalOrigin;

        // This is the tiles connnected to the local position of the tile within the preset
        public Dictionary<Tile, Vector3> tiles = new Dictionary<Tile, Vector3>(); 
        // Exit points are global
        public List<Vector3> exitPositions = new List<Vector3>();

        public PresetData(Vector3 localOrigin, Vector3 globalOrigin,  int rotation, int index, Dictionary<Tile, Vector3> tiles, List<Vector3> exitPositions)
        {
            this.localOrigin = localOrigin;
            this.globalOrigin = globalOrigin;
            this.rotation = rotation;
            this.index = index;
            this.tiles = tiles;
            this.exitPositions = exitPositions;
        }

        public void AddTile(Tile tile, Vector3 localPos)
        {
            tiles.Add(tile, localPos);
        }

        public override string ToString()
        {
            string temp = string.Empty;
            foreach (Tile tile in tiles.Keys)
            {
                temp += tile.gridPosition + "\n";
            }
            return temp;
        }
    }
    public class CubeCoord
    {
        public int q;
        public int r;
        public int s;
        public int h;

        public CubeCoord(int q, int r, int s, int h)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            this.h = h;
        }

        public static Vector3 ToVector3(CubeCoord c)
        {
            int x = c.q;
            int y = 2 * c.r + c.q;
            return new Vector3(x, c.h, y);
        }
        public static CubeCoord FromVector3(Vector3 pos)
        {
            int q = (int)pos.x;
            int r = (int)((pos.z - pos.x) / 2);
            return new CubeCoord(q, r, -q - r, (int)pos.y);
        }

        public static CubeCoord Add(CubeCoord a, CubeCoord b)
        {
            return new CubeCoord(a.q + b.q, a.r + b.r, a.s + b.s, a.h + b.h);
        }
        public static CubeCoord Subtract(CubeCoord a, CubeCoord b)
        {
            return new CubeCoord(a.q - b.q, a.r - b.r, a.s - b.s, a.h - b.h);
        }

        public static float Distance(CubeCoord a, CubeCoord b)
        {
            CubeCoord vec = Subtract(a, b);
            return (Mathf.Abs(vec.q) + Mathf.Abs(vec.r) + Mathf.Abs(vec.s)) / 2;
        }

        /// <summary>
        /// Returns the rotated position of the given Vector3
        /// </summary>
        /// <param name="pos">The Vector3 that will be rotated</param>
        /// <param name="origin">The Vector3 that pos should be rotated around</param>
        /// <param name="rot">The rotation amount (Use increments of 60 DEGREES)</param>
        /// <returns></returns>
        public static Vector3 GetRotatedPosition(Vector3 pos, Vector3 origin, int rot)
        {
            // Convert the vector 3 into a cube coordinate
            CubeCoord cubePos = FromVector3(pos);
            CubeCoord cubeOrigin = FromVector3(origin);

            // The amount of times this should be rotated in increments of 60
            int rotIncrements = Mathf.FloorToInt((360 + (rot % 360)) % 360 / 60);

            // Subtract the origin from the position to get the local position
            CubeCoord rotatedPos = Subtract(cubePos, cubeOrigin);
            for (int i = 0; i < rotIncrements; i++)
            {
                rotatedPos = new CubeCoord
                    (
                        -rotatedPos.s,
                        -rotatedPos.q,
                        -rotatedPos.r,
                        rotatedPos.h
                    );
            }
            return ToVector3(Add(cubeOrigin, rotatedPos));
        }

        public override string ToString()
        {
            return q.ToString() + "," + r.ToString() + "," + s.ToString() + "," + h.ToString();
        }
    }
}

