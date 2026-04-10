using System.Collections.Generic;
using UnityEngine;

public abstract class MovementSystem : MonoBehaviour
{

    [Header("Movement Variables")]
    [SerializeField]
    protected float moveSpeed = 10;
    [SerializeField]
    protected float jumpHeight = 10;
    [SerializeField]
    [Tooltip("Negative values will pull player downward, Positive value will push them up")]
    protected float gravity = -0.98f;

    [Header("Acceleration Variables", order = 2)]
    [SerializeField]
    protected float groundAcceleration = 1;
    [SerializeField]
    protected float airAcceleration = 1;
    [SerializeField]
    protected float groundDeceleration = 1;
    [SerializeField]
    protected float airDeceleration = 1;

    // Used to keep track of the active movement speed modifiers
    public Dictionary<string, float> movementModifiers = new Dictionary<string, float>();
}
