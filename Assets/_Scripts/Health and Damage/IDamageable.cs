using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public enum DamageType { NEUTRAL, CONTACT, SPLASH }

    public static Dictionary<GameObject, IDamageable> Instances = new Dictionary<GameObject, IDamageable>();

    public void DamageContact(float damage, Dictionary<ModifierUtil.Trait, int> traits = null);
    public void DamageSplash(float damage, Dictionary<ModifierUtil.Trait, int> traits = null);

    public static void Register(GameObject obj, IDamageable damageable)
    {
        Instances.Add(obj, damageable);
    }
    public static void Unregister(GameObject obj)
    {
        if(Instances.ContainsKey(obj))
            Instances.Remove(obj);
    }
}
