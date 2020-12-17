﻿//--------------------------------------------------------------------
//
// This is a Unity behaviour script that demonstrates how to use
// timeline markers in your game code. 
//
// Timeline markers can be implicit - such as beats and bars. Or they 
// can be explicity placed by sound designers, in which case they have 
// a sound designer specified name attached to them.
//
// Timeline markers can be useful for syncing game events to sound
// events.
//
// The script starts a piece of music and then displays on the screen
// the current bar and the last marker encountered.
//
// This document assumes familiarity with Unity scripting. See
// https://unity3d.com/learn/tutorials/topics/scripting for resources
// on learning Unity scripting. 
//
//--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FMOD.Studio;
using UnityEngine;

public class TimelineController : MonoBehaviour
{

    private MasterBusController _masterBusController;
    public List<TrackFMODManager> _trackFmodManagers;
    public TrackFMODManager masterReferenceManager;
    
    [Header("--------- Timeline Data-----------")]
    public int songBPM = 135;
    public int totalNumberOfBars = 33;
    public int beatsPerBar = 4;
    public int currentTimeInMS = 0;
    public int totalTimeInMS = 0;
    public int currentBar = 0;
    public int currentBeat = 0;
    public int timelineLagTolerance = 50; // In milli seconds


    // 
    [Range(0, 60000)] public int timelinePositionSetterValue;

    // Moved the whole thing to Awake since other scripts need to access totalTimeInMS in Start()
    private void Awake()
    {
        _masterBusController = GetComponent<MasterBusController>();
        // calculate total time in milli-seconds
        totalTimeInMS = (int)((float)(totalNumberOfBars * beatsPerBar * 60 * 1000) / (float)songBPM);
    }

    void Start()
    {
        _trackFmodManagers = GetComponentsInChildren<TrackFMODManager>().ToList();
        masterReferenceManager = _trackFmodManagers.Find(x => x.isMasterTimelineInstance == true);
    }

    private void Update()
    {
        currentBeat = (currentTimeInMS * songBPM) / 60000;
        currentBar = currentBeat / beatsPerBar + 1;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SetTimelinePositionForAll(timelinePositionSetterValue);
        }
    }
    
    public void SetTimelinePositionForAll(int positionInMs)
    {
        foreach (var trackFmodManager in _masterBusController.allTrackFmodManagers)
        {
            //trackFmodManager.trackEventInstance.setPaused(true);
            trackFmodManager.trackEventInstance.setTimelinePosition(positionInMs);
            //trackFmodManager.trackEventInstance.setPaused(false);
        }
    }


    /*private void CheckTimelineLagConsistency()
    {
        Debug.Log("Checking lag..");
        // Go through all the tracks timeline positions and update all if any of them are more than tolerance level
        if (_trackFmodManagers.Any(x => Mathf.Abs(currentTimeInMS - x.currentTrackTimelinePos) > timelineLagTolerance))
        {
            Debug.Log("Lag found! Trying to set back all to current time...");
            foreach (var tfManager in _trackFmodManagers)
            {
                tfManager.trackEventInstance.setTimelinePosition(currentTimeInMS);
            }
        }
    }*/
}
