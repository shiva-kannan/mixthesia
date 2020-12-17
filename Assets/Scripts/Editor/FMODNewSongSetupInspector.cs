﻿using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

[CustomEditor(typeof(FMODNewSongSetup))]
public class FMODNewSongSetupInspector : Editor
{
    public int selectedReverbTrack;
    public bool useEventAsTrackName;
    public GameObject trackOrbPrefab;
    
    public override void OnInspectorGUI () {
        base.DrawDefaultInspector();

        FMODNewSongSetup fmodNewSongSetup = (FMODNewSongSetup) target;
        
        GUILayout.Space(20f); //2
        GUILayout.Label("Setup Options", EditorStyles.boldLabel); //3
        
        GUILayout.Space(10f);
        selectedReverbTrack = EditorGUILayout.Popup("Pick Track for Reverb: ", selectedReverbTrack,
            fmodNewSongSetup.songTackReferences.ToArray()); 

        GUILayout.Space(10f);

        useEventAsTrackName = GUILayout.Toggle(useEventAsTrackName, "  Use event name as track name");

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20f);

        //trackOrbPrefab = EditorGUILayout.ObjectField("Song Prefab", trackOrbPrefab, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button($"Create Scenes for {fmodNewSongSetup.songName}"))
        {
            // Setup the new scenes with the available information
            Populate(fmodNewSongSetup.songTackReferences, selectedReverbTrack, fmodNewSongSetup.songBPM, fmodNewSongSetup.songBeatPerMeausure, fmodNewSongSetup.totalNumberofMeasures);
        }
        GUILayout.Space(20f);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build for Quest"))
        {
            // Trigger build for Quest
        }

        if (GUILayout.Button("Build for Rift S"))
        {
            // Trigger build for Rift S
        }
        GUILayout.EndHorizontal();
    }


    public void Populate(List<string> songTrackReferences, int reverbTrackIdx, int songBPM, int songBeatPerMeausure, int totalNumberofMeasures)
    {
        //Find the track orb prefab:
        trackOrbPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefab/Track Obj.prefab", typeof(GameObject));

        //List of properties to change:

        //----------AllTracks Obj----------
        //TimelineController.cs: Song BPM, Total Number of Bars, Beats Per Bar

        //----------Individual Track Obj----------
        //Track Obj name
        //TrackOrb.cs: tField, aCube, interactionPlanePosReference, isSelectLocked, isBornSelectd, isAlwaysSelected, isByPassPosition,
        //isInAudioCube, ColorPool,
        //TrackAutomation.cs: isEnabled
        //TrackFMODManager.cs: TrackReference, isMasterTimelineInstance
        //Child0: icon sprite
        //Child1: instru. name

        //Assuming Trackfieldautomation, Reverb, and EQ scenes are at 1, 2, and 3:
        for (int i = 1; i < EditorBuildSettings.scenes.Length; i++)
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[i].path, OpenSceneMode.Single);

            //Find AllTracks Obj: 
            GameObject allTracksObj = GameObject.FindGameObjectWithTag("AllTracks");
            if (!allTracksObj)
            {
                Debug.LogError("Missing AllTracks obj in this scene: " + i);
                continue;
            }

            General_ColorPool colorPoolRef = allTracksObj.GetComponent<General_ColorPool>();
            TimelineController timelineControllerRef = allTracksObj.GetComponent<TimelineController>();

            timelineControllerRef.songBPM = songBPM;
            timelineControllerRef.totalNumberOfBars = totalNumberofMeasures;
            timelineControllerRef.beatsPerBar = songBeatPerMeausure;

            //Get rid of existing tracks:
            for (int k = allTracksObj.transform.childCount - 1; k >= 0; k--)
            {
                DestroyImmediate(allTracksObj.transform.GetChild(k).gameObject);
            }

            //Create new tracks:
            if (i == 2)
            { //if it's in reverb:
                string trackEvent = songTrackReferences[reverbTrackIdx];
                GameObject newTrackObj = Instantiate(trackOrbPrefab);

                //Parse Out Track Name:
                string trackName = "";
                for (int h = trackEvent.Length - 1; h >= 0; h--)
                {
                    char currentChar = trackEvent[h];
                    if (currentChar == '/')
                    {
                        break;
                    }

                    trackName = currentChar + trackName;
                }

                newTrackObj.name = trackName;

                //Deal with TrackOrb.cs related stuff:
                TrackOrb trackOrbRef = newTrackObj.GetComponent<TrackOrb>();

                trackOrbRef.tField = allTracksObj.transform.parent.GetComponent<TrackField>();
                trackOrbRef.aCube = allTracksObj.transform.parent.GetComponent<AudioCube>();
                trackOrbRef.colorPool = colorPoolRef;

                //Deal with TrackAutomation.cs related stuff:
                TrackAutomation trackAutoRef = newTrackObj.GetComponent<TrackAutomation>();

                //Deal with TrackFMODManager.cs related stuff:
                TrackFMODManager trackFMODRef = newTrackObj.GetComponent<TrackFMODManager>();
                trackFMODRef.trackReference = trackEvent;
                trackFMODRef.isMasterTimelineInstance = true;
    
                //Skip the icons for now


                //Change instru name:
                newTrackObj.transform.GetChild(1).GetComponent<TextMeshPro>().text = trackName;

                trackOrbRef.isSelectLocked = false;
                trackOrbRef.isBornSelected = true;
                trackOrbRef.isAlwaysSelected = true;
                trackOrbRef.isInAudioCube = false;
                trackOrbRef.isByPassPosition = true;

                trackAutoRef.isEnabled = false;           
               

                newTrackObj.transform.position = allTracksObj.transform.position;
                newTrackObj.transform.parent = allTracksObj.transform;

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();

                continue;
            }


            for (int j = 0; j < songTrackReferences.Count; j++)
            {
                string trackEvent = songTrackReferences[j];
                GameObject newTrackObj = Instantiate(trackOrbPrefab);

                //Parse Out Track Name:
                string trackName = "";
                for (int h = trackEvent.Length - 1; h >= 0; h--)
                {
                    char currentChar = trackEvent[h];
                    if (currentChar == '/')
                    {
                        break;
                    }

                    trackName = currentChar + trackName;
                }

                newTrackObj.name = trackName;

                //Deal with TrackOrb.cs related stuff:
                TrackOrb trackOrbRef = newTrackObj.GetComponent<TrackOrb>();

                trackOrbRef.tField = allTracksObj.transform.parent.GetComponent<TrackField>();
                trackOrbRef.aCube = allTracksObj.transform.parent.GetComponent<AudioCube>();
                trackOrbRef.colorPool = colorPoolRef;

                //Deal with TrackAutomation.cs related stuff:
                TrackAutomation trackAutoRef = newTrackObj.GetComponent<TrackAutomation>();

                //Deal with TrackFMODManager.cs related stuff:
                TrackFMODManager trackFMODRef = newTrackObj.GetComponent<TrackFMODManager>();
                trackFMODRef.trackReference = trackEvent;
                if (j == 0)
                {
                    trackFMODRef.isMasterTimelineInstance = true;
                }
                else
                {
                    trackFMODRef.isMasterTimelineInstance = false;
                }

                //Skip the icons for now


                //Change instru name:
                newTrackObj.transform.GetChild(1).GetComponent<TextMeshPro>().text = trackName;

                if (i == 3) //if it's in EQ
                {
                    trackOrbRef.isSelectLocked = false;
                    trackOrbRef.interactionPlane_posReference = GameObject.FindGameObjectWithTag("AudioCubePlane").transform;
                    trackOrbRef.isByPassPosition = true;
                    trackOrbRef.isInAudioCube = true;

                    trackAutoRef.isEnabled = false;
                }
                else if (i == 1) //if it's in TrackFieldAutomation
                {
                    trackOrbRef.isSelectLocked = true;
                    trackOrbRef.isByPassPosition = false;
                    trackOrbRef.isInAudioCube = false;

                    trackAutoRef.isEnabled = true;
                }

                newTrackObj.transform.position = allTracksObj.transform.position;
                newTrackObj.transform.parent = allTracksObj.transform;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveOpenScenes();
        }
    }
}