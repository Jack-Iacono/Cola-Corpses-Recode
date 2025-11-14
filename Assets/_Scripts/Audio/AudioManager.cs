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
    private static Dictionary<SoundType, List<AudioSourceController>> audioSourceReference = new Dictionary<SoundType, List<AudioSourceController>>();

    private void Awake()
    {
        // Singleton
        if(Instance != null)
            Destroy(Instance);
        else
        {
            Instance = this;

            // Create the audio sources for each sound type
            foreach (SoundList s in sounds)
            {
                // Initialize the dictionary
                audioSourceReference.Add(s.type, new List<AudioSourceController>());

                // Run through the sources needed for that type and create them
                for (int i = 0; i < s.sourceCount; i++)
                {
                    GameObject g = Instantiate(audioSourceObject, transform);
                    g.name = s.name + " Audio Source";
                    g.SetActive(false);
                    audioSourceReference[s.type].Add(g.GetComponent<AudioSourceController>());
                }
            }
        }
    }

    public static void Play(SoundType type, Vector3 pos = default)
    {
        Play(GetAudioData(type), type, pos);
    }
    public static void Play(AudioData audioData, SoundType type, Vector3 pos = default)
    {
        AudioSourceController source = null;

        int lowestTimeIndex = 0;
        float lowestTime = float.PositiveInfinity;

        // Increment through all sources of the chosen type
        for(int i = 0; i < audioSourceReference[type].Count; i++)
        {
            // Check if the current source is playing or not, if not, check the time remaining on it for use as backup
            if (!audioSourceReference[type][i].isPlaying)
            {
                source = audioSourceReference[type][i];
                break;
            }
            else
            {
                // Tracks the source with the least time remaining and will use that if no inactive source is found
                float timeRemaining = audioSourceReference[type][i].GetTimeRemaining();
                if(timeRemaining < lowestTime)
                {
                    lowestTime = timeRemaining;
                    lowestTimeIndex = i;
                }
            }
        }

        // If no source was found, use the one with the lowest time left for it's playing
        if(source == null)
            source = audioSourceReference[type][lowestTimeIndex];

        if(pos != Vector3.zero)
            source.transform.position = pos;
        source.Play(audioData);
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

    // Enables adding of audio sources from other scripts
    public static void AddAudioSources(SoundType type, int count, Transform trans = null)
    {
        // If this sound has not been added to the dictionary yet, somehow, add it
        if (!audioSourceReference.ContainsKey(type))
            audioSourceReference.Add(type, new List<AudioSourceController>());

        // Run through the sources needed for that type and create them
        for (int i = 0; i < count; i++)
        {
            GameObject g = Instantiate(Instance.audioSourceObject, trans == null ? Instance.transform : trans);
            g.name = type.ToString() + " Audio Source";
            g.SetActive(false);
            audioSourceReference[type].Add(g.GetComponent<AudioSourceController>());
        }
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
    [Tooltip("If you are going to attach sources to the transform of another object, put 0 here")]
    [SerializeField] public AudioData[] sounds;
    [SerializeField] public int sourceCount;
}
