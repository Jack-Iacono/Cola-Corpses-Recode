using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;
using static UnityEditor.PlayerSettings;

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
    private static Dictionary<GameObject, Dictionary<SoundType, List<AudioSourceController>>> audioSourceReference = new Dictionary<GameObject, Dictionary<SoundType, List<AudioSourceController>>>();

    private void Awake()
    {
        // Singleton
        if(Instance != null)
            Destroy(Instance);
        else
        {
            Instance = this;

            // Start setting up the audio reference dictionary for the audio manager
            audioSourceReference.Add(gameObject, new Dictionary<SoundType, List<AudioSourceController>>());

            // Create the audio sources for each sound type
            foreach (SoundList s in sounds)
            {
                audioSourceReference[gameObject].Add(s.type, new List<AudioSourceController>());

                // Run through the sources needed for that type and create them
                for (int i = 0; i < s.sourceCount; i++)
                {
                    GameObject g = Instantiate(audioSourceObject, transform);
                    g.name = s.name + " Audio Source";
                    g.SetActive(false);
                    audioSourceReference[gameObject][s.type].Add(g.GetComponent<AudioSourceController>());
                }
            }
        }
    }

    /// <summary>
    /// Plays an audio clip at the desired position
    /// </summary>
    /// <param name="type">The type of audio to play</param>
    /// <param name="pos">The position to play the audio at</param>
    public static void Play(SoundType type, Vector3 pos)
    {
        // Send in the audio source for this sound in the local audio manager
        AudioSourceController source = GetAvailableAudioSourceController(audioSourceReference[Instance.gameObject][type]);
        source.transform.position = pos;
        source.Play(GetAudioData(type));
    }
    /// <summary>
    /// Plays an audio from the desired gameobject
    /// </summary>
    /// <param name="type">The type of audio to play</param>
    /// <param name="owner">The owner of the audio source controllers</param>
    public static void Play(SoundType type, GameObject owner)
    {
        AudioSourceController source = GetAvailableAudioSourceController(audioSourceReference[owner][type]);
        source.Play(GetAudioData(type));
    }

    /// <summary>
    /// Gets the best audio source to be used from the designated list
    /// </summary>
    /// <param name="sources">The list of Audio Source Controllers to take from</param>
    /// <returns>An AudioSourceController reference</returns>
    private static AudioSourceController GetAvailableAudioSourceController(List<AudioSourceController> sources)
    {
        // Set up to find the best source to use
        AudioSourceController source = null;

        int lowestTimeIndex = 0;
        float lowestTime = float.PositiveInfinity;

        // Increment through all sources of the chosen type
        for (int i = 0; i < sources.Count; i++)
        {
            // Check if the current source is playing or not, if not, check the time remaining on it for use as backup
            if (!sources[i].isPlaying)
            {
                source = sources[i];
                break;
            }
            else
            {
                // Tracks the source with the least time remaining and will use that if no inactive source is found
                float timeRemaining = sources[i].GetTimeRemaining();
                if (timeRemaining < lowestTime)
                {
                    lowestTime = timeRemaining;
                    lowestTimeIndex = i;
                }
            }
        }

        // If no source was found, use the one with the lowest time left for it's playing
        if (source == null)
            source = sources[lowestTimeIndex];

        return source;
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
    public static void AddAudioSources(SoundType type, int count, GameObject owner)
    {
        // If this sound has not been added to the dictionary yet, somehow, add it
        if (!audioSourceReference.ContainsKey(owner))
            audioSourceReference.Add(owner, new Dictionary<SoundType, List<AudioSourceController>>());

        // Check to see if this sound is already set up
        if (!audioSourceReference[owner].ContainsKey(type))
            audioSourceReference[owner].Add(type, new List<AudioSourceController>());

        // Run through the sources needed for that type and create them
        for (int i = 0; i < count; i++)
        {
            GameObject g = Instantiate(Instance.audioSourceObject, owner.transform);
            g.name = owner.name + ": " + type.ToString() + " Audio Source";
            g.SetActive(false);
            audioSourceReference[owner][type].Add(g.GetComponent<AudioSourceController>());
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
