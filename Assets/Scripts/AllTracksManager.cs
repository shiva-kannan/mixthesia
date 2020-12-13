using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AllTracksManager : MonoBehaviour
{
    private TrackFMODManager[] allTrackFmodManagers;

    private bool loadedAllEventInstances = false;

    private bool stop_start_tracker = false;
    // Start is called before the first frame update
    void Start()
    {
        // Get all the track manager reference
        allTrackFmodManagers = GetComponentsInChildren<TrackFMODManager>();
        
        // Start a co-routine that waits for sometime to get the channel groups of all tracks
        StartCoroutine(GetChannelGroupsInAllTracks());
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StopPlayAllTracksAtOnce();
        //}
    }

    private IEnumerator GetChannelGroupsInAllTracks()
    {
        yield return new WaitForSeconds(3);
        foreach (var trackFmodManager in allTrackFmodManagers)
        {
            trackFmodManager.SetChannelGroup();
        }

        StopPlayAllTracksAtOnce();
    }


    public void StopPlayAllTracksAtOnce()
    {
        stop_start_tracker = !stop_start_tracker;
        foreach (var track in allTrackFmodManagers)
        {
            if (stop_start_tracker)
            {
                track.trackEventInstance.start();
            }
            else
            {
                track.trackEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}
