using UnityEngine;

public class BasicInteractable : MonoBehaviour, IInteractable
{
    protected virtual void Awake()
    {
        IInteractable.Register(gameObject, this);
    }

    public void OnInteract()
    {
        Debug.Log("Interact");
    }

    protected virtual void OnDestroy()
    {
        IInteractable.Unregister(gameObject);   
    }
}
