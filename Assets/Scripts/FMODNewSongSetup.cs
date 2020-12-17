using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODNewSongSetup : MonoBehaviour
{
    public string songName;
    public int numberOfStems;
    public int songBPM;
    public int songBeatPerMeausure;
    public int totalNumberofMeasures;
    
    [FMODUnity.EventRef] public List<string> songTackReferences;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
