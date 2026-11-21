using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class TrainVideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    void HandleStateChanged(GameState state)
    {
        if (videoPlayer == null) return;

        bool shouldMove = (state == GameState.Gameplay || state == GameState.WaitingWaveClear);

        if (shouldMove && !videoPlayer.isPlaying)
            videoPlayer.Play();
        else if (!shouldMove && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }
}