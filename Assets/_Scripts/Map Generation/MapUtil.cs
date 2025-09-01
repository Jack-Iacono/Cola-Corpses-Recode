using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using static MapPreset;
using static UnityEditor.PlayerSettings;

namespace MapUtil
{
    public enum TileType { NORMAL, STAIR, HOLE, PRESET }
    public enum WallType { NORMAL, DOOR, HALF, PRESET }

    public class Map
    {
        public List<Room> rooms { get; private set; } = new List<Room>();
        public List<Room> bonusRooms { get; private set; } = new List<Room>();
        public Tile[,,] tiles;

        public Vector3 mapBounds { get; private set; } = new Vector3(50, 1, 50);
        public float tileRadius { get; private set; } = 1;
        public float floorHeight { get; private set; } = 10;
        
        // The length from the center of the hexagon to the mid point of any side
        public float tileSideDistance { get; private set; } = 0.86602f;

        // These two lists contain which cells will be neighbors for a cell depending on their x position
        // This is needed since the rows are not linear and offset between eachother
        public readonly Vector3[] neighbors = new Vector3[]
        {
            new Vector3(0,0,2),
            new Vector3(1,0, 1),
            new Vector3(1,0,-1),
            new Vector3(0,0,-2),
            new Vector3(-1,0,-1),
            new Vector3(-1,0,1),
            new Vector3(0,1,0),
            new Vector3(0,-1,0)
        };

        // Initialized later due to initializationg of hexShortLength
        // Contains the points for the middle of each side as well as for each vertex
        public Vector2[] hexagonVertexes { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonExteriorSidePositions { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonInteriorSidePositions { get; private set; } = new Vector2[] { };

        public Map(Vector3 bounds, float tileRadius, float floorHeight)
        {
            mapBounds = bounds;
            this.tileRadius = tileRadius;
            this.floorHeight = floorHeight;

            // The length from the center of the hexagon to the mid point of any side
            tileSideDistance = Mathf.Sqrt((tileRadius * tileRadius) - (tileRadius / 2) * (tileRadius / 2));

            // Initialize the array for storing the tiles being used
            tiles = new Tile[(int)mapBounds.x, (int)mapBounds.y, (int)mapBounds.z];

            // Vertexes labelled clockwise starting at the 2 o'clock position
            hexagonVertexes = new Vector2[]
            {
                new Vector2(tileRadius / 2, tileSideDistance),
                new Vector2(tileRadius, 0),
                new Vector2(tileRadius / 2, -tileSideDistance),
                new Vector2(-tileRadius / 2,-tileSideDistance),
                new Vector2(-tileRadius, 0),
                new Vector2(-tileRadius / 2,tileSideDistance),
            };
            // Sides are labelled clockwise starting at the top side (12 o'clock)
            hexagonExteriorSidePositions = new Vector2[]
            {
                new Vector2(0,tileSideDistance),
                new Vector2((hexagonVertexes[0].x + hexagonVertexes[1].x) / 2, (hexagonVertexes[0].y + hexagonVertexes[1].y) / 2),
                new Vector2((hexagonVertexes[1].x + hexagonVertexes[2].x) / 2, (hexagonVertexes[1].y + hexagonVertexes[2].y) / 2),
                new Vector2(0,-tileSideDistance),
                new Vector2((hexagonVertexes[3].x + hexagonVertexes[4].x) / 2, (hexagonVertexes[3].y + hexagonVertexes[4].y) / 2),
                new Vector2((hexagonVertexes[4].x + hexagonVertexes[5].x) / 2, (hexagonVertexes[4].y + hexagonVertexes[5].y) / 2)
            };
            // Sides labelled clockwise starting at the 2 o'clock position
            hexagonInteriorSidePositions = new Vector2[]
            {
                new Vector2(hexagonVertexes[0].x/ 2, hexagonVertexes[0].y / 2),
                new Vector2(tileRadius / 2, 0),
                new Vector2(hexagonVertexes[2].x/ 2, hexagonVertexes[2].y / 2),
                new Vector2(hexagonVertexes[3].x/ 2, hexagonVertexes[3].y / 2),
                new Vector2(-tileRadius / 2, 0),
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

        public int prefabIndex = -1;
        public int prefabRotation = 0;

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
                walls[index] = null;
        }

        public void AssignPrefabIndex(int index, int rotation)
        {
            type = TileType.PRESET;
            this.prefabIndex = index;
            this.prefabRotation = rotation;
        }

        public void SetType(TileType type)
        {
            this.type = type;
        }
    }
    public class Wall
    {
        public WallType type {  get; private set; }
        public GameObject obj = null;

        public List<Tile> connectedTiles { get; private set; } = new List<Tile>();

        public Wall(WallType type, Tile connectedTile)
        {
            this.type = type;
        }

        public void AddConnectedTile(Tile tile)
        {
            connectedTiles.Add(tile);
        }
        public void RemoveConnectedTile(Tile tile)
        {
            if(connectedTiles.Count > 1 && connectedTiles.Contains(tile))
                connectedTiles.RemoveAt(0);
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

        public static Map Generate(Vector3 bounds, float tileRadius, float floorHeight, int roomCount, Vector2 roomTileRange, List<MapPreset> roomPresets)
        {
            // The map will store all the placed tiles and rooms
            Map genMap = new Map(bounds, tileRadius, floorHeight);

            // Dead zones and roomGenPaths allow the generator to correctly generate rooms with full exploration
            List<Vector3> deadZones = new List<Vector3>();
            List<List<Vector3>> roomGenPaths = new List<List<Vector3>>();

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
                        genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] = t;
                    }
                }

                Room bonusRoom = GeneratePresetRoom(roomPresets, i);
                if(bonusRoom != null)
                {
                    foreach (Tile t in bonusRoom.GetTiles())
                    {
                        Vector3 pos = t.gridPosition;
                        genMap.tiles[(int)pos.x, (int)pos.y, (int)pos.z] = t;
                    }
                }
            }

            return genMap;


            // The below functions are used purely for organizational purposes

            Room GeneratePresetRoom(List<MapPreset> presetList, int roomIndex)
            {
                // NEED TO ACCOUNT FOR ODD VERSUS EVEN COLUMN GENERATION
                int presetIndex = Random.Range(0, presetList.Count);
                MapPreset preset = presetList[presetIndex];

                List<Preset_Tile> tiles = preset.GetFootprint();
                Dictionary<int, Room> validRooms = new Dictionary<int, Room>();

                // Go through all rotations of the chosen preset
                for (int i = 0; i < 6; i++)
                {
                    // Initialize this room
                    Room newRoom = new Room();

                    // Loop through each tile in the preset for the given rotation
                    for(int j = 0; j < tiles.Count; j++)
                    {
                        Vector3 rotPosition = GetRotatedPosition(tiles[j].gridPosition + currentLocation, currentLocation, i * 60);

                        // Check if this rotation variant of the tile is open
                        if (PositionOpen(rotPosition))
                        {
                            Tile newTile = new Tile(rotPosition, newRoom);
                            newTile.SetType(TileType.PRESET);

                            newTile.obj = tiles[i].tileObject;
                            newRoom.AddTile(newTile);
                            SetTileWalls(newTile, newRoom);

                            // Check if the tile is the center point, if so, assign the preset to that so that the builder knows where to place the prefab
                            if (tiles[j].gridPosition == Vector3.zero)
                            {
                                newTile.AssignPrefabIndex(presetIndex, i*60);
                            }
                        }
                        else
                        {
                            // If this position is not open, break the loop since this preset can't be used
                            break;
                        }  
                    }

                    // Check to see if all tiles cleared
                    if (newRoom.GetTiles().Count == tiles.Count)
                    {
                        validRooms.Add(i*60, newRoom);
                    }
                }

                // Check to make sure that at least one valid room was found
                // If not, backtrack to find a possible position for it
                if (validRooms.Count == 0)
                {
                    // If this triggers, all possible neighbors are invalid and the map is basically filled
                    if (roomIndex - 1 < 0)
                        return null;

                    // Set the loop to redo the current room at a new point not within the deadzones
                    // Make this a loop that will go until it finds a neighbor

                    Vector3 nextLocation = NULL_VECTOR;
                    int nextRoomIndex = roomIndex;

                    // Start at the previous room and run through all previous rooms to find
                    for (int k = roomIndex - 1; k >= 0; k--)
                    {
                        List<Vector3> previousPath = roomGenPaths[k];
                        nextLocation = GetBacktrackNeighbor(ref previousPath);

                        // If the next location has been found, break the loop
                        if (nextLocation != NULL_VECTOR)
                        {
                            nextRoomIndex = k;
                            break;
                        }
                    }

                    // This will run if no previous rooms have any open tiles at all, in which case the map is dead and done
                    if (nextLocation == NULL_VECTOR)
                        return null;

                    // Set the current location and generate a room at the new location
                    currentLocation = nextLocation;
                    return GeneratePresetRoom(presetList, nextRoomIndex);
                }

                // Get a random rotation from the valid rotations
                Room nextRoom = validRooms[validRooms.Keys.ToList()[Random.Range(0, validRooms.Keys.ToList().Count)]];

                genMap.bonusRooms.Add(nextRoom);
                List<Vector3> roomPath = roomGenPaths[roomIndex];
                currentLocation = GetBacktrackNeighbor(ref roomPath, nextRoom);

                return nextRoom;
            }
            Room GenerateNormalRoom(int roomIndex)
            {
                // Create the new Room to hold the data needed
                Room newRoom = new Room();

                // Get a temporary clone of the map tiles for modification here
                List<Vector3> genPath = new List<Vector3>();

                // The amount of tiles that should be generated for this room
                int roomTileGoal = UnityEngine.Random.Range((int)roomTileRange.x, (int)roomTileRange.y);

                // Generate the tiles in the given room
                for (int j = 0; j < roomTileGoal; j++)
                {
                    genPath.Add(currentLocation);
                    Tile newTile = new Tile(currentLocation, newRoom);
                    newRoom.AddTile(newTile);
                    SetTileWalls(newTile, newRoom);

                    // Get next tile in the map
                    Vector3 nextTilePosition = GetRandomValidNeighbor(genPath.Last(), newRoom);
                    if (nextTilePosition == NULL_VECTOR)
                    {
                        genPath.RemoveAt(genPath.Count - 1);
                        nextTilePosition = GetBacktrackNeighbor(ref genPath, newRoom);
                        if (nextTilePosition == NULL_VECTOR)
                            break;
                    }

                    currentLocation = nextTilePosition;
                }

                // Check to see if the room fully generated
                if (newRoom.GetTiles().Count < roomTileGoal - 5)
                {
                    // Add the room tiles to the deadzone
                    foreach (Tile t in newRoom.GetTiles())
                    {
                        deadZones.Add(t.gridPosition);
                    }

                    // If this triggers, all possible neighbors are invalid and the map is basically filled
                    if (roomIndex - 1 < 0)
                        return null;

                    // Set the loop to redo the current room at a new point not within the deadzones
                    // Make this a loop that will go until it finds a neighbor

                    Vector3 nextLocation = NULL_VECTOR;
                    int nextRoomIndex = roomIndex;

                    // Start at the previous room and run through all previous rooms to find
                    for(int k = roomIndex - 1; k >= 0; k--)
                    {
                        List<Vector3> previousPath = roomGenPaths[k];
                        nextLocation = GetBacktrackNeighbor(ref previousPath);

                        // If the next location has been found, break the loop
                        if (nextLocation != NULL_VECTOR)
                        {
                            nextRoomIndex = k;
                            break;
                        }
                    }
                    
                    // This will run if no previous rooms have any open tiles at all, in which case the map is dead and done
                    if (nextLocation == NULL_VECTOR)
                        return null;

                    // Set the current location and generate a room at the new location
                    currentLocation = nextLocation;
                    return GenerateNormalRoom(roomIndex);
                }

                // Add the current room to the map since it generated correctly
                genMap.rooms.Add(newRoom);
                currentLocation = GetBacktrackNeighbor(ref genPath, newRoom);
                roomGenPaths.Add(genPath);

                return newRoom;
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
            Vector3 GetBacktrackNeighbor(ref List<Vector3> roomGenPath, Room room = null)
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

            void SetTileWalls(Tile tile, Room room)
            {
                Vector3 tilePosition = tile.gridPosition;
                Vector3[] nList = genMap.neighbors;

                // Cuts out the last two neighbors (the vertical neighbors)
                for (int i = 0; i < nList.Length - 2; i++)
                {
                    // Global neighbor shows tiles placed on the global map while local Neighbor
                    Tile globalNeighbor = genMap.GetTileAtLocation(tilePosition + nList[i]);
                    Tile localNeighbor = room.GetTileAtLocation(tilePosition + nList[i]);

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
                        }
                        else
                        {
                            // If the neighbor already has a wall in this position, add it to this tile as well and connected them
                            tile.AddWall(i, neighborWall);
                        }
                    }
                    else if (localNeighbor == null)
                    {
                        tile.walls[i] = new Wall(WallType.NORMAL, tile);
                    }
                    else
                    {
                        localNeighbor.RemoveWall((i + 3) % 6);
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
            CubeCoord cubePos = CubeCoord.FromVector3(pos);
            CubeCoord cubeOrigin = CubeCoord.FromVector3(origin);

            // The amount of times this should be rotated in increments of 60
            int rotIncrements = Mathf.FloorToInt(rot / 60);

            // Subtract the origin from the position to get the local position
            CubeCoord rotatedPos = CubeCoord.Subtract(cubePos, cubeOrigin);
            for(int i = 0; i < rotIncrements; i++)
            {
                rotatedPos = new CubeCoord
                    (
                        -rotatedPos.s,
                        -rotatedPos.q,
                        -rotatedPos.r,
                        rotatedPos.h
                    );
            }
            return CubeCoord.ToVector3(CubeCoord.Add(cubeOrigin, rotatedPos));
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

        public override string ToString()
        {
            return q.ToString() + "," + r.ToString() + "," + s.ToString() + "," + h.ToString();
        }
    }
}

