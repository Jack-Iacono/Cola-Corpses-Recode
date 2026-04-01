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

        // This has 8, one for each wall plus one for the middle ground and ceiling
        public TileElement[] elements = new TileElement[8];

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

    [Serializable]
    public class RoomTheme
    {
        [SerializeField]
        private List<MaterialInfo> floorMaterials = new List<MaterialInfo>();
        [SerializeField]
        private List<MaterialInfo> wallMaterials = new List<MaterialInfo>();
        [SerializeField]
        private List<MaterialInfo> ceilingMaterials = new List<MaterialInfo>();
        [SerializeField]
        private List<MaterialInfo> doorMaterials = new List<MaterialInfo>();

        private int floorTotal = 0;
        private int wallTotal = 0;
        private int ceilingTotal = 0;
        private int doorTotal = 0;

        /// <summary>
        /// Get a random floor material from this room theme
        /// </summary>
        /// <returns>A random floor material index</returns>
        public int GetRandomFloorMaterial()
        {
            return GetRandomMaterial(floorMaterials, floorTotal);
        }
        /// <summary>
        /// Get a random wall material from this room theme
        /// </summary>
        /// <returns>A random wall material index</returns>
        public int GetRandomWallMaterial()
        {
            return GetRandomMaterial(wallMaterials, wallTotal);
        }
        /// <summary>
        /// Get a random ceiling material from this room theme
        /// </summary>
        /// <returns>A random ceiling material index</returns>
        public int GetRandomCeilingMaterial()
        {
            return GetRandomMaterial(ceilingMaterials, ceilingTotal);
        }
        /// <summary>
        /// Get a random door material from this room theme
        /// </summary>
        /// <returns>A random door material index</returns>
        public int GetRandomDoorMaterial()
        {
            return GetRandomMaterial(doorMaterials, doorTotal);
        }
        private int GetRandomMaterial(List<MaterialInfo> list, int total)
        {
            // Generate a random number for deciding which material to choose
            int rand = UnityEngine.Random.Range(0, total);
            int currentTotal = 0;

            // Run through list until desired material is chosen
            for (int i = 0; i < list.Count; i++)
            {
                if (rand < list[i].weight + currentTotal)
                    return i;
                currentTotal += list[i].weight;
            }

            return -1;
        }

        public Material[] GetFloorMaterialArray()
        {
            return GetMaterialArray(floorMaterials);
        }
        public Material[] GetWallMaterialArray()
        {
            return GetMaterialArray(wallMaterials);
        }
        public Material[] GetCeilingMaterialArray()
        {
            return GetMaterialArray(ceilingMaterials);
        }
        public Material[] GetDoorMaterialArray()
        {
            return GetMaterialArray(doorMaterials);
        }
        private Material[] GetMaterialArray(List<MaterialInfo> list)
        {
            Material[] result = new Material[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = list[i].material;
            }
            return result;
        }

        /// <summary>
        /// Go through Floor, Wall and Ceiling materials and calculate the total weights that they need to be drawn from
        /// </summary>
        public void CalculateMaterialWeightTotals()
        {
            floorTotal = 0;
            wallTotal = 0;
            ceilingTotal = 0;

            for (int i = 0; i < floorMaterials.Count; i++)
            {
                floorTotal += floorMaterials[i].weight;
            }
            for (int i = 0; i < wallMaterials.Count; i++)
            {
                wallTotal += wallMaterials[i].weight;
            }
            for (int i = 0; i < ceilingMaterials.Count; i++)
            {
                ceilingTotal += ceilingMaterials[i].weight;
            }
        }

        [Serializable]
        public class MaterialInfo
        {
            public Material material;
            public int weight;
        }
    }
    [Serializable]
    public class TileElement
    {
        public enum PlacementType { GROUND, WALL, CEILING }

        [SerializeField] public GameObject prefab;
        [NonSerialized] public int id = -1;
        [SerializeField] public PlacementType placeType = PlacementType.GROUND;
        [SerializeField] public bool bigObject = false;
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

