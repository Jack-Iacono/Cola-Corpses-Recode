using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject hexPart;

    private Vector2 mapDim = new Vector2(50, 50);
    private int tileCount = 250;

    // These offsets are used to set the positions of the floors
    private const float HEX_RADIUS = 1f;
    // Need this function to get a specific value for the length on a hexagon
    private float hexShortLength = Mathf.Sqrt((HEX_RADIUS * HEX_RADIUS) - (HEX_RADIUS / 2) * (HEX_RADIUS / 2));

    // These two lists contain which cells will be neighbors for a cell depending on their x position
    // This is needed since the rows are not linear and offset between eachother
    private Vector2[] evenNeighbors = new Vector2[]
    {
        new Vector2(0,1),
        new Vector2(0,-1),
        new Vector2(1, 0),
        new Vector2(-1,0),
        new Vector2(1,-1),
        new Vector2(-1,-1)
    };
    private Vector2[] oddNeighbors = new Vector2[]
    {
        new Vector2(0,1),
        new Vector2(0,-1),
        new Vector2(1, 0),
        new Vector2(-1,0),
        new Vector2(1,1),
        new Vector2(-1,1)
    };

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();   
    }

    private void GenerateMap()
    {
        Vector2 currentPosition = new Vector2(25,25);
        List<Vector2> visitedNodes = new List<Vector2>() { currentPosition };

        PlaceTile(currentPosition);

        for(int i = 0; i < tileCount; i++)
        {
            Vector2 n = GetRandomValidNeighbor(currentPosition, visitedNodes);
            if (n != currentPosition)
            {
                currentPosition = n;
                visitedNodes.Add(n);

                PlaceTile(n);
            }
            else
                break;
        }
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
        return pos;
    }
    private bool VectorInMap(Vector2 pos)
    {
        return (pos.x >= 0 && pos.x < mapDim.x && pos.y >= 0 && pos.y < mapDim.y);
    }

    private void PlaceTile(Vector2 pos)
    {
        GameObject floor = Instantiate(hexPart);
        floor.transform.position = new Vector3
            (
                pos.x * (HEX_RADIUS * 1.5f),
                0,
                pos.y * (hexShortLength * 2) + (pos.x % 2 * hexShortLength * Mathf.Sign(pos.y))
            );
        floor.transform.rotation = Quaternion.identity;
        floor.name = pos.ToString();

        MeshRenderer mesh = floor.GetComponentInChildren<MeshRenderer>();
        mesh.material.color = Random.ColorHSV();
    }
}
