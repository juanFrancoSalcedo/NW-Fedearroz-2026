using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class NfcCaller : MonoBehaviour
{
    [SerializeField] private NfcHandler handler;
    [SerializeField] private int id;
    [SerializeField] private List<VideoPlayer> videos = new List<VideoPlayer>();

    private void OnEnable() => Subscribe();
    private void OnDisable() => Unsubscribe();

    public void Subscribe()
    {
        Unsubscribe();
        foreach (var video in videos)
        {
            if (video != null)
                video.loopPointReached += HandleVideoEnd;
        }
    }

    public void Unsubscribe()
    {
        foreach (var video in videos)
        {
            if (video != null)
                video.loopPointReached -= HandleVideoEnd;
        }
    }

    public void CallState()
    {
        if (handler != null)
            handler.SetState(id);
    }

    public void CallDefaultState()
    {
        if (handler != null)
            handler.SetDefaultState();
    }

    private void HandleVideoEnd(VideoPlayer source) => CallDefaultState();
}
