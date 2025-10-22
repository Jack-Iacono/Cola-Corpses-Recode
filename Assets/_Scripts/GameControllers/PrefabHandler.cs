using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabHandler : MonoBehaviour
{
    // This class will work as a universal storage for any prefabs that may be needed by any script
    // This allows for reuse of many prefabs across scripts without reassigning them to variables

    public static PrefabHandler Instance;

    [Header("Player / Enemies")]
    public GameObject player;
    public GameObject enemy;

    [Header("Projectiles")]
    public GameObject thrownCan;

    [Header("UI Elements")]
    public GameObject damagePopup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
