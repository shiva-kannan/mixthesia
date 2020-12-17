﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FMODNewSongSetup))]
public class FMODNewSongSetupInspector : Editor
{
    public int selectedReverbTrack;
    public bool useEventAsTrackName;
        
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

        if (GUILayout.Button($"Create Scenes for {fmodNewSongSetup.songName}"))
        {
            // Setup the new scenes with the available information
            // Populate()
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

}