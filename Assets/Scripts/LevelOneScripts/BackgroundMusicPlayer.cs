using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 1.0f;
    [Range(-3f, 3f)] public float pitch = 1.0f;
    public bool loop = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ApplySettings();
    }

    void Start()
    {
        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.Play();
        }
    }

    void Update()
    {
        // This allows real-time updates in Play mode when values are changed in the Inspector
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = loop;
    }

    void ApplySettings()
    {
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = loop;
    }
}
