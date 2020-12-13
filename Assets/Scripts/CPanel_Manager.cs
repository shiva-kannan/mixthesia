using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPanel_Manager : MonoBehaviour
{
    enum SongState
    {
        PLAY,
        STOP,
        PAUSE
    }

    enum SongMuteUnmuteState
    {
        AllMuted,
        AllUnMuted
    }
    
    private SongState songState = SongState.STOP;
    private bool looping = false;
    [SerializeField] private SongMuteUnmuteState songMuteUnmuteState = SongMuteUnmuteState.AllUnMuted;

    [SerializeField] CPanel_Button playPauseButton;
    [SerializeField] MasterBusController masterControl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPause()
    {
        if (songState == SongState.STOP)
        {
            masterControl.StartAllTrakcsAtOnce();
            songState = SongState.PLAY;
        }else if (songState == SongState.PLAY)
        {
            masterControl.PauseResume(true);
            songState = SongState.PAUSE;
        }
        else if (songState == SongState.PAUSE)
        {
            masterControl.PauseResume(false);
            songState = SongState.PLAY;
        }
    }

    public void Stop()
    {
        masterControl.PauseResume(false);
        masterControl.StopAllTracksAtOnce();
        playPauseButton.isFlipped = false;
        songState = SongState.STOP;
    }

    public void Loop()
    {
        looping = !looping;
    }

    public void AllOn()
    {
        masterControl.EnableDisableAllTracks(true);
    }

    public void AllOff()
    {
        masterControl.EnableDisableAllTracks(false);
    }

    public void MuteUnmuteAll()
    {
        if (songMuteUnmuteState == SongMuteUnmuteState.AllUnMuted)
        {
            songMuteUnmuteState = SongMuteUnmuteState.AllMuted;
            masterControl.EnableDisableAllTracks(false);
        }
        else if (songMuteUnmuteState == SongMuteUnmuteState.AllMuted)
        {
            songMuteUnmuteState = SongMuteUnmuteState.AllUnMuted;
            masterControl.EnableDisableAllTracks(true);
        }
    }
}
