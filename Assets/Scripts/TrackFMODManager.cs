using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using Debug = UnityEngine.Debug;


public enum REVERBPRESETS
{
    CONCERTHALL,
    CAVE,
    UNDERWATER
}
public class TrackFMODManager : MonoBehaviour
{

    [FMODUnity.EventRef]
    public string trackReference = null;

    public FMOD.ChannelGroup _trackChannelGroup;
    public FMOD.Studio.EventInstance trackEventInstance;
    private FMOD.Studio.EVENT_CALLBACK _trackLoadEventCallback;
    public static bool loadedEvent = false;

    private float targetPanningValue;
    private float targetVolumeLevel;
    private float currentPanningValue = 0f;
    private float currentVolumeLevel = 0.5f;
    public bool isMute = false;
    public float isMuteVolume = 0.0001f;
    
    [Header("---------DSP Metering Attributes------------")]
    public string dbValue = "";
    public float dbFloatValue = 0f;
    FMOD.DSP meteringdDsp = new FMOD.DSP();
    FMOD.DSP_METERING_INFO meterInfo = new FMOD.DSP_METERING_INFO();
    
    [Header("----------Reverb DSP Properties--------------")]
    public REVERBPRESETS _currentReverbpreset;
    public float initialReverbWetLevel;
    public float initialReverbDryLevel;
    [HideInInspector] public float reverbWetLevel;
    [HideInInspector] public float reverbDryLevel;
    public bool reverbApplied = false;
    private float currentWetLevel;
    private float currentDryLevel;
    FMOD.DSP reverbDSP = new DSP();


    [Header("----------EQ DSP Properties-----------------")]
    //public float eqHighLevel
    public bool eqApplied = false;
    DSP eqDSP = new DSP();
    [Range(-80, 10)] public float eqLowGain;
    [Range(-80, 10)] public float eqMidGain;
    [Range(-80, 10)] public float eqHighGain;
    [HideInInspector] public float eqLowCrossover;
    [HideInInspector] public float eqHighCrossover;
    private float currentLowGain;
    private float currentMidGain;
    private float currentHighGain;
    private float currentLowCrossover;
    private float currentHighCorssover;

    private List<FMOD.DSP> currentDSPChain;
    
    private string _objectName;

    private TrackOrb torb;

    private Timeline _timelineReference;

    [Header("----------Timeline Properties--------------")]
    public bool isMasterTimelineInstance = false;
    public int timelineLagTolerance = 50; // In milli seconds
    private bool lagResetCooldown = false;
    public int currentTrackTimelinePos;

    private TimelineController _timelineController;
    private TrackFFTContainer _trackFFTContainer;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _objectName = gameObject.name;
        _trackLoadEventCallback = new EVENT_CALLBACK(TrackLoadEventCallback);
        trackEventInstance = FMODUnity.RuntimeManager.CreateInstance(trackReference);
        trackEventInstance.setCallback(_trackLoadEventCallback, EVENT_CALLBACK_TYPE.CREATED);

        _trackChannelGroup.setPan(currentPanningValue);
        _trackChannelGroup.setVolume(targetVolumeLevel);
        
        _timelineController = GetComponentInParent<TimelineController>();
        
        torb = GetComponent<TrackOrb>();

        _timelineReference = GameObject.Find("Timeline Head").GetComponent<Timeline>();
        _trackFFTContainer = GetComponent<TrackFFTContainer>();

    }

    // Update is called once per frame
    void Update()
    {
        targetPanningValue = torb.GetMappedPanning();
        targetVolumeLevel = torb.GetMappedVolume();

        currentPanningValue = targetPanningValue;
        _trackChannelGroup.setPan(targetPanningValue);



        currentVolumeLevel = targetVolumeLevel;
        if (!isMute)
        {
            _trackChannelGroup.setVolume(targetVolumeLevel);
        }
        else
        {
            _trackChannelGroup.setVolume(isMuteVolume);
        }
        
        // Update RMS value every frame
        UpdateRMSValue();
        
        // To test from editor
        if (currentDryLevel != reverbDryLevel)
        {
            reverbDSP.setParameterFloat((int) DSP_SFXREVERB.DRYLEVEL, reverbDryLevel);
            currentDryLevel = reverbDryLevel;
        }
        if (currentWetLevel != reverbWetLevel)
        {
            reverbDSP.setParameterFloat((int) DSP_SFXREVERB.WETLEVEL, reverbWetLevel);
            currentWetLevel = reverbWetLevel;
        }

        if (isMasterTimelineInstance)
        {
            trackEventInstance.getTimelinePosition(out _timelineController.currentTimeInMS);
        }

        trackEventInstance.getTimelinePosition(out currentTrackTimelinePos);

        UpdateEqParameters();

        // For testing 
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddEqDSP();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // Add the main FFT dsp for the track
            _trackFFTContainer.AddTrackFFTToChannelGroup();
        }
    }

    public void SetChannelGroup()
    {
        trackEventInstance.getChannelGroup(out _trackChannelGroup);
        _trackChannelGroup.getDSP(0, out meteringdDsp);
        meteringdDsp.setMeteringEnabled(false, true);
        // Add the main FFT and EQ dsp for the track by default
        // Order matters
        AddEqDSP();
        _trackFFTContainer.AddTrackFFTToChannelGroup();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT TrackLoadEventCallback(EVENT_CALLBACK_TYPE type, EventInstance instance,
        IntPtr parameterPtr)
    {
        Debug.Log($"Event created!");
        //TODO: Notify the master bus controller after it's loaded
        loadedEvent = true;
        return FMOD.RESULT.OK;
    }

    // Set the track to mute (disable) in our world
    public void EnableDisable(bool enable)
    {
        Debug.Log("Set Mute Called!");
        // Hacky AF!
        if (!enable)
        {
            _trackChannelGroup.setVolume(isMuteVolume);
        }
        else
        {
            _trackChannelGroup.setVolume(currentVolumeLevel);
        }
        isMute = !enable;
    }
    
    float GetRMS()
    {
        float rms = 0f;

        meteringdDsp.getMeteringInfo(IntPtr.Zero, out meterInfo);
        for (int i = 0; i < meterInfo.numchannels; i++)
        {
            rms += meterInfo.rmslevel[i] * meterInfo.rmslevel[i];
        }

        rms = Mathf.Sqrt(rms / (float)meterInfo.numchannels);

        float dB = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
        if (dB > 10.0f) dB = 10.0f;
        return dB;
    }
    
    private void UpdateRMSValue()
    {
        if (loadedEvent)
        {
            dbFloatValue = GetRMS();
            dbValue = "RMS: " + dbFloatValue.ToString("F2") + " dB";   
        }
    }


    public void AddReverbDSP()
    {
        if (!reverbApplied)
        {
            FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.SFXREVERB, out reverbDSP);
            REVERB_PROPERTIES concertHallDefault = ApplyReverbPreset(_currentReverbpreset);
        

            string[] pTypeNames = Enum.GetNames (typeof(DSP_SFXREVERB));
            foreach (var fieldInfo in concertHallDefault.GetType().GetFields())
            {
                var name = fieldInfo.Name;
                int index = Array.IndexOf(pTypeNames, name.ToUpper());
                reverbDSP.setParameterFloat(index, (float)fieldInfo.GetValue(concertHallDefault));
            }
            // Set dry and wet level to default
            reverbDSP.setParameterFloat((int) DSP_SFXREVERB.WETLEVEL, initialReverbWetLevel);
            reverbDSP.setParameterFloat((int)DSP_SFXREVERB.DRYLEVEL, initialReverbDryLevel);
            _trackChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, reverbDSP);

            currentDryLevel = initialReverbDryLevel;
            currentWetLevel = initialReverbWetLevel;
            reverbDryLevel = initialReverbDryLevel;
            reverbWetLevel = initialReverbWetLevel;

            reverbApplied = true;
        }
    }

    public REVERB_PROPERTIES ApplyReverbPreset(REVERBPRESETS reverbpreset)
    {
        MethodInfo[] reverbPresetMethods = typeof(PRESET).GetMethods();
        foreach (var mInfo in reverbPresetMethods)
        {
            if (mInfo.Name == Enum.GetName(typeof(REVERBPRESETS), (int)reverbpreset))
            {
                var returnValue = (REVERB_PROPERTIES) mInfo.Invoke(this, null);
                return returnValue;
            }
        }
        return new REVERB_PROPERTIES();
    }

    public int RemoveReverbDSP()
    {
        // Return 1 if result is okay otherwise 0
        var removalResult = _trackChannelGroup.removeDSP(reverbDSP);
        if (removalResult == RESULT.OK)
        {
            reverbApplied = false;
            return 1;
        }
        else
        {
            Debug.LogWarning(removalResult);
            return 0;
        }
    }

    public void AddEqDSP()
    {
        if (!eqApplied)
        {
            FMODUnity.RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.THREE_EQ, out eqDSP);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.LOWGAIN, out currentLowGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.LOWGAIN, out eqLowGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.MIDGAIN, out currentMidGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.MIDGAIN, out eqMidGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.HIGHGAIN, out currentHighGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.HIGHGAIN, out eqHighGain);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.LOWCROSSOVER, out currentLowCrossover);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.LOWCROSSOVER, out eqLowCrossover);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.HIGHCROSSOVER, out currentHighCorssover);
            eqDSP.getParameterFloat((int) DSP_THREE_EQ.HIGHCROSSOVER, out eqHighCrossover);
            
            _trackChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, eqDSP);

            eqApplied = true;
        }
    }

    public int RemoveEqDSP()
    {
        var removalResult = _trackChannelGroup.removeDSP(eqDSP);
        if (removalResult == RESULT.OK)
        {
            eqApplied = false;
            return 1;
        }
        else
        {
            Debug.LogWarning(removalResult);
            return 0;
        }
    }

    private void UpdateEqParameters()
    {
        if (eqApplied)
        {
            if (currentHighGain != eqHighGain)
            {
                eqDSP.setParameterFloat((int) DSP_THREE_EQ.HIGHGAIN, eqHighGain);
                Debug.Log("Setting high gain..");
                currentHighGain = eqHighGain;
            }
            if (currentMidGain != eqMidGain)
            {
                eqDSP.setParameterFloat((int) DSP_THREE_EQ.MIDGAIN, eqMidGain);
                currentMidGain = eqMidGain;
            }
            if (currentLowGain != eqLowGain)
            {
                eqDSP.setParameterFloat((int) DSP_THREE_EQ.LOWGAIN, eqLowGain);
                currentLowGain = eqLowGain;
            }

            if (currentLowCrossover != eqLowCrossover)
            {
                eqDSP.setParameterFloat((int) DSP_THREE_EQ.LOWCROSSOVER, eqLowCrossover);
                currentLowCrossover = eqLowCrossover;
            }

            if (currentHighCorssover != eqHighCrossover)
            {
                eqDSP.setParameterFloat((int) DSP_THREE_EQ.HIGHCROSSOVER, eqHighCrossover);
                currentHighCorssover = eqHighCrossover;
            }
        }
    }

    public void MakeEQBrighter()
    {
        if (isEqParamsBrightRange() || isEqParamsDefault())
        {
            eqLowGain = eqLowGain - 3.5f;
            eqMidGain = eqMidGain + 1.5f;
            eqHighGain = eqHighGain + 2.5f;
        }
        else
        {
            eqLowGain = eqLowGain + 2.5f;
            eqMidGain = eqMidGain + 2f;
            eqHighGain = eqHighGain + 5f;
        }
        
    }

    public void MakeEQMuddier()
    {
        if (isEqParamsBrightRange())
        {
            eqLowGain = eqLowGain + 3.5f;
            eqMidGain = eqMidGain - 1.5f;
            eqHighGain = eqHighGain - 2.5f;
        }
        else
        {
            eqLowGain = eqLowGain + 2.5f;
            eqMidGain = eqMidGain - 2f;
            eqHighGain = eqHighGain - 5f;
        }
        
    }

    private bool isEqParamsBrightRange()
    {
        if (eqHighGain > 0f && eqMidGain > 0f && eqLowGain < 0f)
        {
            return true;
        }
        return false;
    }
    
    private bool isEqParamsDefault()
    {
        if (eqHighGain == 0f && eqMidGain == 0f && eqLowGain == 0f)
        {
            return true;
        }
        return false;
    }

    public void ResetEqGainParameters()
    {
        eqLowGain = 0f;
        eqMidGain = 0f;
        eqHighGain = 0f;
    }
    
    private void OnDestroy()
    {
        // Gracefully quit
        trackEventInstance.setUserData(IntPtr.Zero);
        trackEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        trackEventInstance.release();
    }
}
