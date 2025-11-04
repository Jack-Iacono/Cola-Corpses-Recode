using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public static Dictionary<GameObject, IInteractable> ObjectReference = new Dictionary<GameObject, IInteractable>();

    public abstract void OnInteract();

    public static void Register(GameObject gameObject, IInteractable interctable)
    {
        if(!ObjectReference.ContainsKey(gameObject))
            ObjectReference.Add(gameObject, interctable);
    }
    public static void Unregister(GameObject gameObject)
    {
        if(ObjectReference.ContainsKey(gameObject))
            ObjectReference.Remove(gameObject);
    }
}
