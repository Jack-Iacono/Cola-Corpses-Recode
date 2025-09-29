using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Flavor { }
public enum Attributes { }

public enum ActionState { DOWN, UP, HELD, NONE };

public class Weapon
{
    public string name;

    public float damage;
    public float radius;
    public float range;
    public float useTime;

    // Add other stats later, just need this skeleton class for now
    private WeaponAction primaryAction;
    private WeaponAction secondaryAction;

    public Weapon()
    {
        primaryAction = new WeaponAction_CanThrow(this);
        secondaryAction = new WeaponAction_CanThrow(this);
    }

    public void Update(float dt, ActionState primaryState, ActionState secondaryState)
    {
        primaryAction.Update(dt, primaryState);
        //secondaryAction.Update(dt, secondaryState);
    }
}
