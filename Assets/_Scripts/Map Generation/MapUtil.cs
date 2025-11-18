using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static MapPreset;
using static UnityEditor.PlayerSettings;

namespace MapUtil
{
    public enum TileType { NORMAL, CUSTOM, EMPTY}
    public enum WallType { NORMAL, CUSTOM, EMPTY, DOOR }

    public class Map
    {
        public List<Room> rooms { get; private set; } = new List<Room>();
        public List<Room> bonusRooms { get; private set; } = new List<Room>();
        public Tile[,,] tiles;

        public Vector3 mapBounds { get; private set; } = new Vector3(50, 1, 50);

        public Vector3 originTile = new Vector3(0, 0, 0);

        // These are the parameters for what a normal map's proportions look like
        public const float TILE_RADIUS = 4f;
        public const float FLOOR_HEIGHT = 4;
        // Relative to 1, treat as a percentage of the entire platform
        public const float WALL_THICKNESS = 0.05f;
        // I believe this is a global measurement
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
        private Dictionary<Tile, Room> doors = new Dictionary<Tile, Room>();

        public GameObject obj;

        public int themeIndex = 0;
        public int index = -1;

        public List<PresetData> presets = new List<PresetData>();

        public Room()
        {

        }

        public List<Tile> GetTiles()
        {
            return new List<Tile>(tiles.Values.ToList());
        }
        public Dictionary<Tile, Room> GetDoors()
        {
            return doors;
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
        public void AddDoor(Tile tile, Room room)
        {
            doors.Add(tile, room);
        }
    }

    public class Tile
    {
        public Vector3 gridPosition = Vector3.zero;
        public Room room;
        public Wall[] walls = new Wall[6];
        public Ceiling ceiling = null;

        public bool hasSpawner = false;

        public GameObject obj = null;

        public TileType type { get; private set; } = TileType.NORMAL;
        
        public int materialIndex = 0;

        /// <summary>
        /// Tiles that are modification locked will not be able to have items placed on them and all walls will be either preset or normal
        /// </summary>
        public bool modificationLocked = false;
        public bool presetContained = false;

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

        public void SetCeiling(Ceiling ceiling)
        {
            this.ceiling = ceiling;
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
        public void SetType(WallType type)
        {
            this.type = type;
        }

        public List<Tile> GetConnectedTiles()
        {
            return connectedTiles;
        }
    }   
    public class Ceiling
    {
        // Use this class to store the elements of the ceiling
        // This can be lights or various other elements

        public int materialIndex { get; private set; } = -1;
        public bool hasCollider { get; private set; } = false;

        public void SetMaterialIndex(int index)
        {
            materialIndex = index;
        }
        public void SetHasCollider(bool hasCollider)
        {
            this.hasCollider = hasCollider;
        }
    }

    public static class MapGenerator
    {
        // While this is not technically null, the graph that generates the points will never have to use this position
        private static readonly Vector3 NULL_VECTOR = new Vector3(-1, -1, -1);
        private const float prefabPlaceChance = 0.01f;

        public static Map Generate(Vector3 bounds, int roomCount, Vector2 roomTileRange, List<MapPreset> roomPresets, List<RoomTheme> roomThemes)
        {
            // The map will store all the placed tiles and rooms
            Map genMap = new Map(bounds);

            // Dead zones and roomGenPaths allow the generator to correctly generate rooms with full exploration
            List<Vector3> deadZones = new List<Vector3>();
            List<List<Vector3>> roomGenPaths = new List<List<Vector3>>();

            // Store the potential doors that are found from generating the map
            Dictionary<RoomPair, List<Wall>> potentialDoors = new Dictionary<RoomPair, List <Wall>>();

            // Store the doors that lead out of contained presets
            // This is a little more complicated since each preset needs to store multiple exit tiles and their doors
            Dictionary<Tile, List<Wall>> containedPresetWalls = new Dictionary<Tile, List<Wall>>();

            // Orgainze the preset indices based on their function
            List<int> stairPresetIndices = new List<int>();
            List<int> normalPresetIndices = new List<int>();

            for(int i = 0; i < roomPresets.Count; i++)
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
                if(newRoom != null)
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
            foreach(Room room in genMap.GetRooms())
            {
                SetRoomWalls(room);
                SetRoomCeilings(room);
            }
            // Place all the doors on the map
            SetDoors();

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

                    if(UnityEngine.Random.Range(0,1f) < prefabPlaceChance)
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
                                Debug.Log(tile.gridPosition + ": " + tile.modificationLocked.ToString());
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
                    }
                    else
                    {
                        // Add this tile to the path of generated tiles
                        genPath.Add(currentLocation);

                        // create a new tile at this location
                        Tile newTile = new Tile(currentLocation, newRoom);

                        // Decide if this tile should have a spawner or not
                        // This is very basic for now, but should work
                        if (UnityEngine.Random.Range(0, 1f) < 0.1f)
                            newTile.hasSpawner = true;

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
                        return null;

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
                    if(neighbor != NULL_VECTOR)
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
                List<PresetData> validPresets = presetIndex != -1 ? GetValidPresetRotations(presetIndex, origin, room) : GetValidPresets(origin,room);

                // Check if there are no presets to choose from
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
                foreach (Vector3 entryTilePosition in preset.entryPoints)
                {
                    // Check every rotation of the room in respect to the entrypoint
                    for (int i = 0; i < 6; i++)
                    {
                        // Create an array to store all the tile gridPositions
                        Dictionary<Tile,Vector3> presetTiles = new Dictionary<Tile, Vector3>();
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
                            validPresets.Add(new PresetData(entryTilePosition, origin, i*60, presetIndex, presetTiles, globalExitPositions, preset));
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
                    if(aboveTile == null || aboveTile.type != TileType.EMPTY)
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

                // This is for normal tiles
                foreach(Tile tile in room.GetTiles())
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
                                if(!tile.modificationLocked && !neighbor.modificationLocked && !neighbor.presetContained)
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
                        else if(neighbor == null)
                        {
                            Wall newWall = new Wall(WallType.NORMAL, tile);
                            tile.AddWall(i, newWall);

                            // Assign a random material from the list to the wall
                            newWall.SetMaterialIndex(tile, roomThemes[room.themeIndex].GetRandomWallMaterial());
                        }
                    }
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
                                            if(!containedPresetWalls.ContainsKey(tile))
                                                containedPresetWalls.Add(tile, new List<Wall>());

                                            // Add this tile as a door candidate
                                            containedPresetWalls[tile].Add(newWall);
                                        }
                                    }
                                }
                                else if(neighbor == null)
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
            }
            void SetDoors()
            {
                // Run through all the potential doors to choose random ones
                foreach(List<Wall> doorSpots in potentialDoors.Values)
                {
                    // Door spots can sometimes yield no doors
                    if(doorSpots.Count > 0)
                    {
                        Wall newDoor = doorSpots[UnityEngine.Random.Range(0, doorSpots.Count)];
                        newDoor.SetType(WallType.DOOR);

                        foreach(Tile t in newDoor.GetConnectedTiles())
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
    }

    public class PresetData
    {
        public int rotation;
        public int index;

        public bool isStair = false;
        public bool isContained = false;

        public Vector3 localOrigin;
        public Vector3 globalOrigin;

        // This is the tiles connnected to the local position of the tile within the preset
        public Dictionary<Tile, Vector3> tiles = new Dictionary<Tile, Vector3>(); 
        // Exit points are global
        public List<Vector3> exitPositions = new List<Vector3>();

        public PresetData(Vector3 localOrigin, Vector3 globalOrigin,  int rotation, int index, Dictionary<Tile, Vector3> tiles, List<Vector3> exitPositions, MapPreset preset)
        {
            this.localOrigin = localOrigin;
            this.globalOrigin = globalOrigin;
            this.rotation = rotation;
            this.index = index;
            this.tiles = tiles;
            this.exitPositions = exitPositions;

            // Store the properties from the preset here as well for easier access later
            this.isStair = preset.isStair;
            this.isContained = preset.isContained;
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

        public override bool Equals(object obj)
        {
            PresetData other = obj as PresetData;
            return tiles.Equals(other.tiles) && index == other.index;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(tiles,index);
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
        /// <returns>The Vector3 representing the rotated pos variable</returns>
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

