using System;
using System.Collections.Generic;
using UnityEngine;

public class TileElementManager : MonoBehaviour
{
    public static TileElementManager Instance { get; private set; }
    
    // Store important elements in variables for more concrete reference
    public TileElement spawner;

    // Store decorations in a more generic list for reference later
    public TileElement[] decoElements;

    // Use this to store all references for every tile element for building purposes
    private Dictionary<int, TileElement> tileElementReference = new Dictionary<int, TileElement>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            int currentID = 1;

            // Imporant Tile Elements
            spawner.id = currentID++;
            tileElementReference.Add(spawner.id, spawner);

            // Initialize the id values for each of the tile elements
            foreach(TileElement decoElement in decoElements)
            {
                decoElement.id = currentID++;
                tileElementReference[decoElement.id] = decoElement;
            }
        }
        else
            Destroy(this);
    }

    private void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
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
