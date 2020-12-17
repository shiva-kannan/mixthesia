using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FMODNewSongPopulator : MonoBehaviour
{
    [SerializeField] GameObject trackOrbPrefab;

    public void Populate(List<string> songTackReferences, string reverbTrackReference, int songBPM, int songBeatPerMeausure, int totalNumberofMeasures)
    {
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
            foreach (Transform child in allTracksObj.transform)
            {
                Destroy(child.gameObject);
            }

            //Create new tracks:
            if (i == 2)
            { //if it's in reverb:
                continue;
            }


            for (int j = 0; j < songTackReferences.Count; j++)
            {
                string trackEvent = songTackReferences[i];
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
                trackOrbRef.

                if (i == 3) //if it's in EQ
                {
                    trackOrbRef.interactionPlane_posReference = GameObject.FindGameObjectWithTag("AudioCubePlane").transform;
                    trackOrbRef.isByPassPosition = true;
                    trackOrbRef.isInAudioCube = true;
                }else if (i == 1) //if it's in TrackFieldAutomation
                {
                    trackOrbRef.isByPassPosition = false;
                    trackOrbRef.isInAudioCube = false;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveOpenScenes();
        }
    }
}
