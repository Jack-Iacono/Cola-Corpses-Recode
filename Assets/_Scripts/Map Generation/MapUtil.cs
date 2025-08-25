using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace MapUtil
{
    public class Map
    {
        private List<Room> rooms = new List<Room>();
        private Tile[,,] tiles;

        private Vector3 mapBounds = new Vector3(50, 1, 50);
        public float tileRadius { get; private set; } = 1;
        public float floorHeight { get; private set; } = 10;
        private int roomCount = 1;
        private Vector2 roomTileRange = new Vector2(50, 100);

        private readonly float floorChangeChance = 0.01f;

        // The length from the center of the hexagon to the mid point of any side
        public float tileSideDistance { get; private set; } = 0.8666f;

        // These two lists contain which cells will be neighbors for a cell depending on their x position
        // This is needed since the rows are not linear and offset between eachother
        private readonly Vector3[] evenNeighbors = new Vector3[]
        {
            new Vector3(0,0,1),
            new Vector3(1,0, 0),
            new Vector3(1,0,-1),
            new Vector3(0,0,-1),
            new Vector3(-1,0,-1),
            new Vector3(-1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,-1,0)
        };
        private readonly Vector3[] oddNeighbors = new Vector3[]
        {
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(1,0, 0),
            new Vector3(0,0,-1),
            new Vector3(-1,0,0),
            new Vector3(-1,0,1),
            new Vector3(0,1,0),
            new Vector3(0,-1,0)
        };

        // While this is not technically null, the graph that generates the points will never have to use this position
        private readonly Vector3 NULL_VECTOR = new Vector3(-1,-1,-1);

        // Initialized later due to initializationg of hexShortLength
        // Contains the points for the middle of each side as well as for each vertex
        public Vector2[] hexagonVertexes { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonExteriorSidePositions { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonInteriorSidePositions { get; private set; } = new Vector2[] { };

        public Map(Vector3 bounds, float tileRadius, float floorHeight, int roomCount, Vector2 roomTileRange)
        {
            mapBounds = bounds;
            this.tileRadius = tileRadius;
            this.roomCount = roomCount;
            this.roomTileRange = roomTileRange;
            this.floorHeight = floorHeight;

            // The length from the center of the hexagon to the mid point of any side
            tileSideDistance = Mathf.Sqrt((tileRadius * tileRadius) - (tileRadius / 2) * (tileRadius / 2));

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
            this.roomCount = roomCount;
            this.roomTileRange = roomTileRange;
        }

        public void GenerateMap()
        {
            // Initialze the array that will hold the array of all the tiles in the game
            tiles = new Tile[(int)mapBounds.x, (int)mapBounds.y, (int)mapBounds.z];
            List<Vector3> deadZones = new List<Vector3>();

            // Get the mid point of the map bounds
            Vector3 currentLocation = new Vector3(Mathf.FloorToInt(mapBounds.x/2), Mathf.FloorToInt(mapBounds.y/2), Mathf.FloorToInt(mapBounds.z / 2));
            
            // Store the paths from the previous rooms
            List<List<Vector3>> roomPaths = new List<List<Vector3>>();

            // Create the desired amount of rooms
            for(int i = 0; i < roomCount; i++)
            {
                // Create a new room to hold the tiles, but don't add it to the map yet in case it doesn't generate fully
                Room newRoom = new Room();

                // Get a temporary clone of the map tiles for modification here
                List<Vector3> genPath = new List<Vector3>();

                // Add the starting tile to the generation path and to the room
                genPath.Add(currentLocation);
                Tile startingTile = new Tile(currentLocation, newRoom);
                newRoom.AddTile(startingTile);

                SetTileWalls(startingTile, newRoom);

                // The amount of tiles that should be generated for this room
                int roomTileGoal = UnityEngine.Random.Range((int)roomTileRange.x, (int)roomTileRange.y);

                // If the current location is null, then stop generating as something is really wrong
                if (currentLocation == NULL_VECTOR)
                    break;

                // Generate the tiles in the given room
                for (int j = 0; j < roomTileGoal; j++)
                {
                    // Get next tile in the map
                    Vector3 tileGenPosition = GetRandomValidNeighbor(genPath.Last(), deadZones, newRoom);
                    if (tileGenPosition == NULL_VECTOR)
                    {
                        genPath.RemoveAt(genPath.Count - 1);
                        tileGenPosition = GetBacktrackNeighbor(ref genPath, deadZones, newRoom);
                        if (tileGenPosition == NULL_VECTOR)
                            break;
                    }

                    // Add this vector to the path as well as to the map of placed tiles
                    genPath.Add(tileGenPosition);

                    // Create the tile and add it to the room
                    Tile newTile = new Tile(tileGenPosition, newRoom);
                    newRoom.AddTile(newTile);

                    SetTileWalls(newTile, newRoom);
                }

                // Check to see if the room fully generated
                if(newRoom.GetTiles().Count < roomTileGoal - 5)
                {
                    // Add the room tiles to the deadzone
                    foreach(Tile t in newRoom.GetTiles())
                    {
                        deadZones.Add(t.gridPosition);
                    }

                    // Set the loop to redo the current room at a new point not within the deadzones
                    i--;
                    List<Vector3> previousPath = roomPaths[i];
                    currentLocation = GetBacktrackNeighbor(ref previousPath, deadZones);

                    // Failsafe for when the prior room also has no valid neighbors
                    if (currentLocation == NULL_VECTOR)
                        break;

                    continue;
                }

                // Add each tile to the tile map for later access and comparison
                foreach(Tile t in newRoom.GetTiles())
                {
                    Vector3 pos = t.gridPosition;
                    tiles[(int)pos.x, (int)pos.y, (int)pos.z] = t;
                }

                // Add the current room to the map since it generated correctly
                rooms.Add(newRoom);
                currentLocation = GetBacktrackNeighbor(ref genPath, deadZones, newRoom);
                roomPaths.Add(genPath);
            }
        }

        private Vector3 GetRandomValidNeighbor(Vector3 current, List<Vector3> deadZones, Room room = null)
        {
            List<Vector3> validHorizontalPositions = new List<Vector3>();
            List<Vector3> validVerticalPosition = new List<Vector3>();

            // Get the positions of the neighbors for the specific row
            Vector3[] nList = current.x % 2 == 0 ? evenNeighbors : oddNeighbors;

            // Go through all neighbors for the given cell
            for (int i = 0; i < nList.Length; i++)
            {
                Vector3 neighbor = nList[i] + current;
                if (PositionOpen(neighbor, room) && !deadZones.Contains(neighbor))
                {
                    if (nList[i].y == 0)
                        validHorizontalPositions.Add(neighbor);
                    else
                        validVerticalPosition.Add(neighbor);
                }
            }

            // If there is a vertical neighbor and either the cahnce to change floors happens, or there are no horizontal neighbors
            if (validVerticalPosition.Count > 0 && (UnityEngine.Random.Range(0, 1f) < floorChangeChance || validHorizontalPositions.Count == 0))
                return validVerticalPosition[UnityEngine.Random.Range(0, validVerticalPosition.Count)];
            else if(validHorizontalPositions.Count > 0)
                return validHorizontalPositions[UnityEngine.Random.Range(0, validHorizontalPositions.Count)];

            // If no valid neighbor was found, return a null
            return NULL_VECTOR;
        }
        private Vector3 GetBacktrackNeighbor(ref List<Vector3> roomGenPath, List<Vector3> deadZones, Room room = null)
        {
            // Run through the list, removing items as you go to find one that has an open tile
            for (int i = roomGenPath.Count - 1; i > 0; i--)
            {
                // Evaluate the position for neighbors and return if a neighbor with open spaces is found
                Vector3 neighborCheck = GetRandomValidNeighbor(roomGenPath[i], deadZones, room);
                if (neighborCheck != NULL_VECTOR)
                    return neighborCheck;
                else
                    roomGenPath.RemoveAt(i);
            }

            return NULL_VECTOR;
        }

        private void SetTileWalls(Tile tile, Room room)
        {
            Vector3 tilePosition = tile.gridPosition;
            Vector3[] nList = tile.gridPosition.x % 2 == 0 ? evenNeighbors : oddNeighbors;

            // Cuts out the last two neighbors (the vertical neighbors)
            for(int i = 0; i < nList.Length - 2; i++) 
            {
                // Global neighbor shows tiles placed on the global map while local Neighbor
                Tile globalNeighbor = GetTileAtLocation(tilePosition + nList[i]);
                Tile localNeighbor = room.GetTileAtLocation(tilePosition + nList[i]);

                if (globalNeighbor != null)
                {
                    // These walls should be stored as candidates for doors between rooms
                    Wall neighborWall = globalNeighbor.walls[(i + 3) % 6];
                    if (neighborWall == null)
                    {
                        // If there is currently no wall between this tile and the neighbor, create on
                        Wall newWall = new Wall(Wall.WallType.NORMAL, tile);
                        tile.AddWall(i, newWall);
                        globalNeighbor.AddWall((i + 3) % 6, newWall);
                    }
                    else
                    {
                        // If the neighbor already has a wall in this position, add it to this tile as well and connected them
                        tile.AddWall(i, neighborWall);
                    }
                }
                else if(localNeighbor == null)
                {
                    tile.walls[i] = new Wall(Wall.WallType.NORMAL, tile);
                }
                else
                {
                    localNeighbor.RemoveWall((i + 3) % 6);
                }
            }
        }

        private bool PositionOpen(Vector3 pos, Room room = null)
        {
            /*
             * Check for 3 conditions
             * - Is this position within the bounds of the map
             * - Is this position occupied on the current version map
             * - (Optional) Is this position occupied in the currently generating room
            */

            if (room == null)
                return VectorInMap(pos) && tiles[(int)pos.x, (int)pos.y, (int)pos.z] == null;
            return VectorInMap(pos) && tiles[(int)pos.x, (int)pos.y, (int)pos.z] == null && room.GetTileAtLocation(pos) == null;
        }
        private bool VectorInMap(Vector3 pos)
        {
            return 
            (
                pos.x >= 0 && pos.x < mapBounds.x && 
                pos.y >= 0 && pos.y < mapBounds.y && 
                pos.z >= 0 && pos.z < mapBounds.z
            );
        }

        public List<Room> GetRooms()
        {
            return rooms;
        }
        public Tile GetTileAtLocation(Vector3 pos)
        {
            return VectorInMap(pos) ? tiles[(int)pos.x, (int)pos.y, (int)pos.z] : null;
        }
        public float GetTileRadius()
        {
            return tileRadius;
        }
        public float GetTileSideDistance()
        {
            return tileSideDistance;
        }
    }

    public class Room
    {
        private Dictionary<Vector3, Tile> tiles = new Dictionary<Vector3, Tile>();
        private List<Tile> doorTiles = new List<Tile>();
        private List<Tile> stairTiles = new List<Tile>();

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
        
        public enum TileType { NORMAL, STAIR, HOLE }
        public TileType type { get; private set; } = TileType.NORMAL;

        public int stairDirection = 0;

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

        public void SetType(TileType type)
        {
            this.type = type;
        }
    }
    public class Wall
    {
        public enum WallType { NORMAL, DOOR, HALF }
        public WallType type {  get; private set; }

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
}

