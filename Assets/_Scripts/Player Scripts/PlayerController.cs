using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 10f;
    private CharacterController charCont;
    private float health = 100;

    // Start is called before the first frame update
    void Start()
    {
        charCont = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Warps the player to the specified location
    /// </summary>
    /// <param name="location">The location to warp to (global position)</param>
    public void Warp(Vector3 location)
    {
        // Check to see if the player does have a character controller
        if (charCont == null)
        {
            charCont.enabled = false;
            transform.position = location;
            charCont.enabled = true;
        }
        else
            transform.position = location;
    }
}
