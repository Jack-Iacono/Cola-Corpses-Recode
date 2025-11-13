using System;
using UnityEngine;
using static AudioManager;

[Serializable]
[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 1)]
public class AudioData : ScriptableObject
{
    [SerializeField]
    public AudioClip audioClip;
    [NonSerialized]
    public float clipLength;

    public bool playOnAwake = false;
    public bool loop = false;

    [Range(0, 256)]
    public int priority = 128;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    [Range(-1, 1)]
    public float stereoPan = 0;
    [Range(0, 1)]
    public float spatialBlend = 1;
    [Range(0, 1.1f)]
    public float reverbZoneMix = 1;

    [Header("3D Sound Settings")]
    [Range(0, 5)]
    public float dopplerLevel = 1;
    [Range(0, 360)]
    public float spread = 0;

    [Header("Rolloff")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    public float minDistance = 0;
    public float maxDistance = 20;
    public AnimationCurve rollOffCurve = new AnimationCurve();

#if UNITY_EDITOR
    private void OnValidate()
    {
        clipLength = audioClip.length;
    }
#endif
}
