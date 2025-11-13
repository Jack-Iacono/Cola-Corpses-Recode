using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public GameObject audioSourceObject;

    public enum SoundType
    {
        p_Jump,
        p_Hurt,
        w_CanThrow,
        w_CanExplode,
        w_Drink
    }

    // Sounds that will be used
    public SoundList[] sounds = new SoundList[0];

    // A reference to all created audio source objects
    private Dictionary<SoundType, List<AudioSourceController>> audioSourceReference = new Dictionary<SoundType, List<AudioSourceController>>();

    private void Awake()
    {
        // Singleton
        if(Instance != null)
            Destroy(Instance);
        Instance = this;

        // Create the audio sources for each sound type
        foreach(SoundList s in sounds)
        {
            // Initialize the dictionary
            audioSourceReference.Add(s.type, new List<AudioSourceController>());

            // Run through the sources needed for that type and create them
            for(int i = 0; i < s.sourceCount; i++)
            {
                GameObject g = Instantiate(audioSourceObject, transform);
                g.name = s.name + " Audio Source";
                g.SetActive(false);
                audioSourceReference[s.type].Add(g.GetComponent<AudioSourceController>());
            }
        }
    }

    public static void Play(SoundType type)
    {
        Play(GetAudioData(type), type);
    }
    public static void Play(AudioData audioData, SoundType type)
    {
        Instance.audioSourceReference[type][0].Play(audioData);
    }

    /// <summary>
    /// Returns a random audio data from given sound type collection
    /// </summary>
    /// <param name="type">The type of sound to select from</param>
    /// <returns>An AudioData containing the data for a sound within the requested type</returns>
    private static AudioData GetAudioData(SoundType type)
    {
        AudioData[] s = Instance.sounds[(int)type].sounds;
        return s[UnityEngine.Random.Range(0, s.Length)];
    }
    /// <summary>
    /// Returns a specific audio data via their indexes (This is mostly used for networking purposes)
    /// </summary>
    /// <param name="i">The index of the SoundType</param>
    /// <param name="j">The index of the AudioData</param>
    /// <returns>The AudioData with the given indexes</returns>
    public static AudioData GetAudioData(int i, int j)
    {
        return Instance.sounds[i].sounds[j];
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref sounds, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            sounds[i].name = names[i];
            sounds[i].type = (SoundType)i;
        }
    }
#endif

}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [HideInInspector] public SoundType type;
    [SerializeField] public AudioData[] sounds;
    [SerializeField] public int sourceCount;
}
