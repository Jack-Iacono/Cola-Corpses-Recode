using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    // I don't use parent objects to contain the pooled object for now since I don't see a reason to do so other than organization in the editor

    // Dictionary to store references to the pooled items
    private static Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

    /// <summary>
    /// Create a pool for the given GameObject
    /// </summary>
    /// <param name="obj">The GameObject that you want to create a pool for</param>
    /// <param name="count">The amount of that GameObject to generate in the pool</param>
    public static void PoolObject(GameObject obj, int count)
    {
        // If there is already a pool for this GameObject, do nothing
        if (pooledObjects.ContainsKey(obj))
            return;

        // Make a new dictionary entry for this GameObject's pool
        pooledObjects.Add(obj, new List<GameObject>());

        // Create the desired amount of the GameObject
        for(int i = 0; i < count; i++)
        {
            pooledObjects[obj].Add(CreateObject(obj));
        }
    }
    /// <summary>
    /// Adds a specific amount of items to an already existing pool
    /// </summary>
    /// <param name="obj">The GameObject to add</param>
    /// <param name="count">The amount that is needed</param>
    public static void AddToPool(GameObject obj, int count = 1)
    {
        // If there is no pool for this GameObject yet, just make it and end there
        if(!pooledObjects.ContainsKey(obj))
        {
            PoolObject(obj, count);
            return;
        }

        // Add the new items to the pooledItems dictionary
        for(int i = 0;i < count; i++)
        {
            pooledObjects[obj].Add(CreateObject(obj));
        }
    }
    /// <summary>
    /// Creates a new GameObject to be pooled
    /// </summary>
    /// <param name="obj">The GameObject to create</param>
    /// <returns>The newly created GameObject</returns>
    private static GameObject CreateObject(GameObject obj)
    {
        // Use this method in case I need to add messaging or events for spawning later
        GameObject newObject = GameObject.Instantiate(obj);
        newObject.name = obj.name;
        newObject.SetActive(false);
        return newObject;
    }

    /// <summary>
    /// Gets an GameObject from the pool for that GameObject
    /// </summary>
    /// <param name="obj">The GameObject needed from a pool</param>
    /// <returns>The GameObject from the pool</returns>
    public static GameObject GetObject(GameObject obj)
    {
        // If the pool does not exist, make it here
        if(!pooledObjects.ContainsKey(obj))
            PoolObject(obj, 5);

        // Walk through objects from the beginning to find one that is not in use currently
        for(int i = 0; i < pooledObjects[obj].Count; i++)
        {
            // If the GameObject is not being used, return that
            if(!pooledObjects[obj][i].activeInHierarchy)
                return pooledObjects[obj][i];
        }



        return null;
    }

    /// <summary>
    /// Removes a given GameObject pool
    /// </summary>
    /// <param name="obj">The GameObject that should have it's pool removed</param>
    public static void RemovePool(GameObject obj)
    {
        if(pooledObjects.ContainsKey(obj))
            pooledObjects.Remove(obj);
    }
}
