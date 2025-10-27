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

    public delegate void ImpactDelegate(IDamageable hit);
    public delegate void SplashDelegate(IDamageable[] hits);
    public delegate void ExpireDelegate(ProjectileController sender);

    public event ImpactDelegate OnImpactCollide;
    public event SplashDelegate OnSplashCollide;
    public event ExpireDelegate OnExpire;

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

    public virtual void Activate()
    {
        currentLifetime = lifetime;
        isAlive = true;
        gameObject.SetActive(true);
    }
    public virtual void Deactivate()
    {
        isAlive = false;
        gameObject.SetActive(false);
        OnExpire?.Invoke(this);
    }

    protected virtual void IncrementLifetime()
    {
        if(currentLifetime > 0)
            currentLifetime -= Time.deltaTime;
        else if(isAlive)
            Deactivate();
    }
    protected virtual void UpdateAction() { }

    protected void ImpactCollide(IDamageable hit)
    {
        OnImpactCollide?.Invoke(hit);
    }
    protected void SplashCollide(IDamageable[] hits)
    {
        OnSplashCollide?.Invoke(hits);
    }

    private void OnDestroy()
    {
        ProjectileInstances.Remove(gameObject);
    }
}
