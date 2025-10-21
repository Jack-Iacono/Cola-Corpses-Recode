using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileController : MonoBehaviour
{
    public static Dictionary<GameObject, ProjectileController> ProjectileInstances = new Dictionary<GameObject, ProjectileController>();

    public string description;

    public float lifetime = 0;
    protected float currentLifetime = 0;
    protected bool isAlive = false;

    protected Weapon weapon;

    protected virtual void Awake()
    {
        ProjectileInstances.Add(gameObject, this);
    }

    protected virtual void Update()
    {
        // Check if the game is paused
        if (!GameController.isPaused)
        {
            IncrementLifetime();
            UpdateAction();
        }
    }

    public virtual void Activate(Weapon sourceWeapon)
    {
        currentLifetime = lifetime;
        isAlive = true;
        gameObject.SetActive(true);
        this.weapon = sourceWeapon;
    }
    public virtual void Deactivate()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }

    protected virtual void IncrementLifetime()
    {
        if(currentLifetime > 0)
            currentLifetime -= Time.deltaTime;
        else if(isAlive)
            Deactivate();
    }
    protected virtual void UpdateAction() { }

    private void OnDestroy()
    {
        ProjectileInstances.Remove(gameObject);
    }
}
