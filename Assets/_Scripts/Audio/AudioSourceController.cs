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
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
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
