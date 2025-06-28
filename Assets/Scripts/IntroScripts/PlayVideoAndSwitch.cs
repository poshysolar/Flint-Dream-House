using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class PlayVideoAndSwitch : MonoBehaviour
{
    [Header("Assign your video in the Inspector")]
    public VideoClip videoClip;

    public string nextSceneName = "LevelOne";

    void Start()
    {
        // Show the mouse cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Get or add the VideoPlayer component
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

        // Attach the main camera
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;

        videoPlayer.clip = videoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // Add or get an AudioSource to play sound from the video
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.EnableAudioTrack(0, true);

        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
