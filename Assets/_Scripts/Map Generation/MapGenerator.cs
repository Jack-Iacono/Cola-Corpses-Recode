using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject floorObject;
    public GameObject wallObject;

    private Vector2 mapDim = new Vector2(50, 50);
    private int tileCount = 250;

    // These offsets are used to set the positions of the floors
    private const float HEX_RADIUS = 1f;
    // Need this function to get a specific value for the length on a hexagon
    private float hexShortLength = Mathf.Sqrt((HEX_RADIUS * HEX_RADIUS) - (HEX_RADIUS / 2) * (HEX_RADIUS / 2));
    private readonly Vector2 NULL_VECTOR = new Vector2(-1, -1);

    // These two lists contain which cells will be neighbors for a cell depending on their x position
    // This is needed since the rows are not linear and offset between eachother
    private readonly Vector2[] evenNeighbors = new Vector2[]
    {
        new Vector2(0,1),
        new Vector2(0,-1),
        new Vector2(1, 0),
        new Vector2(-1,0),
        new Vector2(1,-1),
        new Vector2(-1,-1)
    };
    private readonly Vector2[] oddNeighbors = new Vector2[]
    {
        new Vector2(0,1),
        new Vector2(0,-1),
        new Vector2(1, 0),
        new Vector2(-1,0),
        new Vector2(1,1),
        new Vector2(-1,1)
    };

    // Initialized later due to initializationg of hexShortLength
    // Contains the points for the middle of each side as well as for each vertex
    private Vector2[] hexagonVertexes = new Vector2[] { };
    private Vector2[] hexagonExteriorSidePositions = new Vector2[] { };
    private Vector2[] hexagonInteriorSidePositions = new Vector2[] { };

    // Start is called before the first frame update
    void Start()
    {
        // Vertexes labelled clockwise starting at the 2 o'clock position
        hexagonVertexes = new Vector2[]
        {
            new Vector2(HEX_RADIUS / 2, hexShortLength),
            new Vector2(HEX_RADIUS, 0),
            new Vector2(HEX_RADIUS / 2, -hexShortLength),
            new Vector2(-HEX_RADIUS / 2,-hexShortLength),
            new Vector2(-HEX_RADIUS, 0),
            new Vector2(-HEX_RADIUS / 2,hexShortLength),
        };
        // Sides are labelled clockwise starting at the top side (12 o'clock)
        hexagonExteriorSidePositions = new Vector2[]
        {
            new Vector2(0,hexShortLength),
            new Vector2((hexagonVertexes[0].x + hexagonVertexes[1].x) / 2, (hexagonVertexes[0].y + hexagonVertexes[1].y) / 2),
            new Vector2((hexagonVertexes[1].x + hexagonVertexes[2].x) / 2, (hexagonVertexes[1].y + hexagonVertexes[2].y) / 2),
            new Vector2(0,-hexShortLength),
            new Vector2((hexagonVertexes[3].x + hexagonVertexes[4].x) / 2, (hexagonVertexes[3].y + hexagonVertexes[4].y) / 2),
            new Vector2((hexagonVertexes[4].x + hexagonVertexes[5].x) / 2, (hexagonVertexes[4].y + hexagonVertexes[5].y) / 2)
        };
        // Sides labelled clockwise starting at the 2 o'clock position
        hexagonInteriorSidePositions = new Vector2[]
        {
            new Vector2(hexagonVertexes[0].x/ 2, hexagonVertexes[0].y / 2),
            new Vector2(HEX_RADIUS / 2, 0),
            new Vector2(hexagonVertexes[2].x/ 2, hexagonVertexes[2].y / 2),
            new Vector2(hexagonVertexes[3].x/ 2, hexagonVertexes[3].y / 2),
            new Vector2(-HEX_RADIUS / 2, 0),
            new Vector2(hexagonVertexes[5].x/ 2, hexagonVertexes[5].y / 2)
        };

        GenerateRoom();
    }

    private void GenerateRoom()
    {
        Vector2 currentPosition = new Vector2(25,25);
        List<Vector2> visitedNodes = new List<Vector2>() { currentPosition };

        PlaceTile(currentPosition);

        for(int i = 0; i < tileCount; i++)
        {
            Vector2 n = GetNextNode(currentPosition, visitedNodes);
            if (n != NULL_VECTOR)
            {
                currentPosition = n;
                visitedNodes.Add(n);

                PlaceTile(n);
            }
            else
            {
                Debug.Log("Stuck");
                break;
            }
        }
    }
    
    private Vector2 GetNextNode(Vector2 pos, List<Vector2> visited)
    {
        Vector2 next  = GetRandomValidNeighbor(pos, visited);
        if (next ==  NULL_VECTOR)
            next = GetBacktrackNeighbor(visited);
        return next;
    }
    private Vector2 GetRandomValidNeighbor(Vector2 pos, List<Vector2> visited)
    {
        List<Vector2> validPositions = new List<Vector2>();

        Vector2[] nList = pos.x % 2 == 0 ? evenNeighbors : oddNeighbors;

        foreach(Vector2 offset in nList)
        {
            Vector2 neighbor = offset + pos;
            if(VectorInMap(neighbor) && !visited.Contains(neighbor)) 
                validPositions.Add(neighbor);
        }

        if(validPositions.Count > 0)
            return validPositions[Random.Range(0,validPositions.Count)];

        return NULL_VECTOR;
    }
    private Vector2 GetBacktrackNeighbor(List<Vector2> visited)
    {
        // Backtrack through the list to find the first node that has a valid neighbor
        for(int i = visited.Count - 1; i > 0; i--)
        {
            Vector2 checkNode = GetRandomValidNeighbor(visited[i], visited);
            if (checkNode != NULL_VECTOR) 
            {
                return checkNode;
            }
        }

        return NULL_VECTOR;
    }
    private bool VectorInMap(Vector2 pos)
    {
        return (pos.x >= 0 && pos.x < mapDim.x && pos.y >= 0 && pos.y < mapDim.y);
    }

    private void PlaceTile(Vector2 graphPoint)
    {
        GameObject floor = Instantiate(floorObject);
        Vector3 tilePosition = new Vector3
        (
            graphPoint.x * (HEX_RADIUS * 1.5f),
            0,
            graphPoint.y * (hexShortLength * 2) + (graphPoint.x % 2 * hexShortLength * Mathf.Sign(graphPoint.y))
        );
        floor.transform.position = tilePosition;
        floor.transform.rotation = Quaternion.identity;
        floor.name = graphPoint.ToString();

        floor.GetComponentInChildren<MeshRenderer>().material.color = Random.ColorHSV();

        foreach(Vector2 midPoint in hexagonExteriorSidePositions)
        {
            GameObject wall = Instantiate(wallObject);
            wall.transform.parent = floor.transform;
            wall.transform.localScale = new Vector3(HEX_RADIUS, 1, 0.1f);
            wall.transform.localPosition = new Vector3
            (
                midPoint.x,
                0,
                midPoint.y
            );
            wall.transform.LookAt(tilePosition);
        }
    }
}
