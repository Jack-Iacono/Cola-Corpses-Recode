using System;
using UnityEngine;
using UnityEngine.Events;

public class BasicInteractable : MonoBehaviour, IInteractable
{
    [Space(10)]
    [SerializeField]
    private ButtonClickedEvent interactMethod = new ButtonClickedEvent();

    protected virtual void Awake()
    {
        IInteractable.Register(gameObject, this);
    }

    public void OnInteract()
    {
        interactMethod?.Invoke();
    }

    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }
    public ButtonClickedEvent onClick
    {
        get { return interactMethod; }
        set { interactMethod = value; }
    }

    protected virtual void OnDestroy()
    {
        IInteractable.Unregister(gameObject);   
    }
}
