using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapUtil
{
    public class Map
    {
        private List<Room> rooms = new List<Room>();

        private Vector3 mapBounds = new Vector3(50, 1, 50);
        public float tileRadius { get; private set; } = 1;
        private int roomCount = 1;
        private Vector2 roomTileRange = new Vector2(50, 100);

        // The length from the center of the hexagon to the mid point of any side
        public float tileSideDistance { get; private set; } = 0.8666f;

        // These two lists contain which cells will be neighbors for a cell depending on their x position
        // This is needed since the rows are not linear and offset between eachother
        private readonly Vector3[] evenNeighbors = new Vector3[]
        {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0, 0),
        new Vector3(-1,0,0),
        new Vector3(1,0,-1),
        new Vector3(-1,0,-1)
        };
        private readonly Vector3[] oddNeighbors = new Vector3[]
        {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0, 0),
        new Vector3(-1,0,0),
        new Vector3(1,0,1),
        new Vector3(-1,0,1)
        };

        private readonly Vector3 NULL_VECTOR = new Vector3(-1,-1,-1);

        // Initialized later due to initializationg of hexShortLength
        // Contains the points for the middle of each side as well as for each vertex
        public Vector2[] hexagonVertexes { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonExteriorSidePositions { get; private set; } = new Vector2[] { };
        public Vector2[] hexagonInteriorSidePositions { get; private set; } = new Vector2[] { };

        public Map(Vector3 bounds, float tileRadius, int roomCount, Vector2 roomTileRange)
        {
            mapBounds = bounds;
            this.tileRadius = tileRadius;
            this.roomCount = roomCount;
            this.roomTileRange = roomTileRange;

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
            // Get the mid point of the map bounds
            Vector3 currentLocation = new Vector3(Mathf.FloorToInt(mapBounds.x/2), Mathf.FloorToInt(mapBounds.y/2), Mathf.FloorToInt(mapBounds.z / 2));

            // Create the desired amount of rooms
            for(int i = 0; i < roomCount; i++)
            {
                // Create the new room that this will be attached to
                Room newRoom = new Room();
                rooms.Add(newRoom);

                List<Vector3> roomGenPath = new List<Vector3>();
                roomGenPath.Add(currentLocation);

                // Generate the tiles in the given room
                for (int j = 0; j < UnityEngine.Random.Range(roomTileRange.x, roomTileRange.y); j++)
                {
                    Vector3 n = GetNextLocation(currentLocation, roomGenPath);
                    if (n != NULL_VECTOR)
                    {
                        Tile newTile = new Tile(currentLocation);
                        newRoom.AddTile(newTile);

                        currentLocation = n;
                        roomGenPath.Add(n);
                    }
                    else
                        break;
                }
            }
        }

        private Vector3 GetNextLocation(Vector3 pos, List<Vector3> roomGenPath)
        {
            Vector3 next = GetRandomValidNeighbor(pos, roomGenPath);
            if (next == NULL_VECTOR)
                next = GetBacktrackNeighbor(roomGenPath);
            return next;
        }
        private Vector3 GetRandomValidNeighbor(Vector3 pos, List<Vector3> visited)
        {
            List<Vector3> validPositions = new List<Vector3>();

            Vector3[] nList = pos.x % 2 == 0 ? evenNeighbors : oddNeighbors;

            foreach (Vector3 offset in nList)
            {
                Vector3 neighbor = offset + pos;
                if (VectorInMap(neighbor) && !visited.Contains(neighbor))
                    validPositions.Add(neighbor);
            }

            if (validPositions.Count > 0)
                return validPositions[UnityEngine.Random.Range(0, validPositions.Count)];

            return NULL_VECTOR;
        }
        private Vector3 GetBacktrackNeighbor(List<Vector3> visited)
        {
            // Backtrack through the list to find the first node that has a valid neighbor
            for (int i = visited.Count - 1; i > 0; i--)
            {
                Vector3 checkNode = GetRandomValidNeighbor(visited[i], visited);
                if (checkNode != NULL_VECTOR)
                {
                    return checkNode;
                }
            }

            return NULL_VECTOR;
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
            foreach (Room room in rooms)
            {
                Tile tile = room.GetTileAtLocation(pos);
                if(tile != null) 
                    return tile;
            }

            return null;
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
        private Wall[] walls = new Wall[6];
        
        public enum TileType { NORMAL, STAIR, HOLE }
        private TileType type;

        public Tile(Vector3 gridPosition)
        {
            this.gridPosition = gridPosition;
        }

        public Wall[] GetWalls()
        {
            return walls;
        }

        public void SetWall(int index, Wall wall)
        {
            walls[index] = wall;
        }
    }
    public class Wall
    {
        public enum WallType { NORMAL, DOOR, HALF }
        private WallType type;
    }
}

