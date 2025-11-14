using UnityEngine;
using static AudioManager;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    public bool isPlaying { get; private set; } = false;

    private AudioSource audioSource;
    private AudioData audioData;

    private Timer playTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playTimer = new Timer(null, null, Stop);
    }

    public void Play(AudioData data)
    {
        SetAudioSourceData(data);
        gameObject.SetActive(true);
        audioSource.Play();
        isPlaying = true;
        playTimer.Start(audioData.clipLength, 0);
    }
    public void Stop()
    {
        isPlaying = false;
        audioSource.Stop();
        gameObject.SetActive(false);
    }

    public float GetTimeRemaining()
    {
        return playTimer.GetRemainingTime();
    }

    private void Update()
    {
        playTimer.Update(Time.deltaTime);
    }

    public void SetAudioSourceData(AudioData sound)
    {
        audioData = sound;

        audioSource.clip = sound.audioClip;
        audioSource.playOnAwake = sound.playOnAwake;
        audioSource.loop = sound.loop;

        audioSource.priority = sound.priority;

        // Allows subtle variation in these sounds to keep them sounding nice
        audioSource.volume = Random.Range(sound.volume - sound.volumeVariance, sound.volume + sound.volumeVariance);
        audioSource.pitch = Random.Range(sound.pitch - sound.pitchVariance, sound.pitch + sound.pitchVariance);

        audioSource.panStereo = sound.stereoPan;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.reverbZoneMix = sound.reverbZoneMix;

        audioSource.dopplerLevel = sound.dopplerLevel;
        audioSource.spread = sound.spread;

        audioSource.rolloffMode = sound.rolloffMode;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        if (sound.rolloffMode == AudioRolloffMode.Custom)
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sound.rollOffCurve);
    }
}
