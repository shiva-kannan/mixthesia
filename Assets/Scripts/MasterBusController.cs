using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MasterBusController : MonoBehaviour
{
    
    public string masterBusName = "bus:/";

    [Range(0f, 5f)] public float adjustableVolumeLevel = 0.7f;
    FMOD.Studio.Bus masterBus;
    private float currentMasterVolume;
    
    
    [Header("Editor Level Track Controls")]
    public bool stop_start_tracks = false;
    // For editor testing
    private bool stop_start_current = false;

    public bool pause_resume_tracks = false;
    // For editor testing
    private bool pause_resume_current = false;
    
    public bool enable_disable_all_tracks = true;
    // For editor testing
    private bool enable_disable_current = true;

    public TrackFMODManager[] allTrackFmodManagers;
    public TrackOrb[] allTrackOrbManagers;
    public Tool_EQ_Simple[] allToolEqSimple;
 
    [Header("EQ Mode Info")] public bool isSimpleEqOn = false;
     
    [Header("Meter Info Data")]
    FMOD.DSP _master_fft;
    FMOD.DSP meteringdDsp = new FMOD.DSP();
    FMOD.DSP_METERING_INFO meterInfo = new FMOD.DSP_METERING_INFO();
    public string masterBusRMSValue = "";
    public float masterBusRMSFloatValue = 0f;
    
    [Header("FFT Related Data")]
    public int WindowSize = 512;
    public float[] leftChannelSpectrumData;
    public float[] rightChannelSpectrumData;
    public bool enable_fft_read = false;

    [Header("Track Start/Stop Event")]
    public UnityEvent stopTrackCalled;
    public UnityEvent startTrackCalled;

    // Start is called before the first frame update
    void Start()
    {
        // Get all the track manager reference
        allTrackFmodManagers = GetComponentsInChildren<TrackFMODManager>();
        allTrackOrbManagers = GetComponentsInChildren<TrackOrb>();
        allToolEqSimple = GetComponentsInChildren<Tool_EQ_Simple>();
        
        // Start a co-routine that waits for sometime to get the channel groups of all tracks
        StartCoroutine(GetChannelGroupsInAllTracks());
        
        // Get FFT from the master channel for some base visualizations
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _master_fft);
        _master_fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
        _master_fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, WindowSize * 2);
        
        leftChannelSpectrumData = new float[WindowSize];
        rightChannelSpectrumData = new float[WindowSize];

        FMOD.ChannelGroup masterChannelGroup;
        FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out masterChannelGroup);
        masterChannelGroup.getDSP(0, out meteringdDsp);
        meteringdDsp.setMeteringEnabled(false, true);
        masterChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _master_fft);
        
        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusName);
        masterBus.getVolume(out adjustableVolumeLevel);
        currentMasterVolume = adjustableVolumeLevel;

        //InvokeRepeating("DisplayRMS", 0.5f, 0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMasterVolume != adjustableVolumeLevel)
        {
            masterBus.setVolume(adjustableVolumeLevel);
            currentMasterVolume = adjustableVolumeLevel;
        }

        // For editor testing, can be removed later
        if (stop_start_current != stop_start_tracks)
        {
            if (stop_start_tracks)
            {
                StartAllTrakcsAtOnce();
            }
            else
            {
                StopAllTracksAtOnce();
            }
            stop_start_current = stop_start_tracks;
        }

        if (pause_resume_current != pause_resume_tracks)
        {
            PauseResume(pause_resume_tracks);
            pause_resume_current = pause_resume_tracks;
        }

        if (enable_disable_current != enable_disable_all_tracks)
        {
            EnableDisableAllTracks(enable_disable_all_tracks);
            enable_disable_current = enable_disable_all_tracks;
        }

        
        if (enable_fft_read)
        {
            IntPtr unmanagedData;
            uint length;
            _master_fft.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out unmanagedData, out length);
            FMOD.DSP_PARAMETER_FFT fftData = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(FMOD.DSP_PARAMETER_FFT));
            var spectrum = fftData.spectrum;

            if (fftData.numchannels > 0)
            {
                for (int i = 0; i < WindowSize; ++i)
                {
                    leftChannelSpectrumData[i] = spectrum[0][i];
                    rightChannelSpectrumData[i] = spectrum[1][i];
                }
            }
        }

        // Get metering info every frame
        UpdateRMS();
        
    }

    // Start = True , Stop = False
    public void StopAllTracksAtOnce()
    {
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        stopTrackCalled.Invoke();

        GetComponent<TimelineController>().SetTimelinePositionForAll(0);
    }

    public void StartAllTrakcsAtOnce()
    {
        foreach (var track in allTrackFmodManagers)
        {
            track.trackEventInstance.start();
        }
        startTrackCalled.Invoke();

        //Debug.Log("Start Called");
    }

    public void PauseResume(bool pause)
    {
        foreach (var track in allTrackFmodManagers)
        {
            track.trackEventInstance.setPaused(pause);
        }
    }

    // Used to enable disable all tracks at once
    public void EnableDisableAllTracks(bool enable)
    {
        foreach (var track in allTrackFmodManagers)
        {
            track.EnableDisable(enable);
        }
    }
    
    
    // TODO: Ideally call this after all event instances are created
    private IEnumerator GetChannelGroupsInAllTracks()
    {
        yield return new WaitForSeconds(3);
        foreach (var trackFmodManager in allTrackFmodManagers)
        {
            trackFmodManager.SetChannelGroup();
        }
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
    
    private void UpdateRMS()
    {
        masterBusRMSFloatValue = GetRMS();
        masterBusRMSValue = "RMS: " + masterBusRMSFloatValue.ToString("F2") + " dB";
    }


    public void TurnOnOffSimpleEqMode(TextMeshProUGUI eqModeTextMeshProUGUI)
    {
        foreach (var trackOrbs in allTrackOrbManagers)
        {
            trackOrbs.TurnEQ(false);
        }
        if (isSimpleEqOn)
        {
            eqModeTextMeshProUGUI.text = "Switch to Simple EQ";
            foreach (var trackSimple in allToolEqSimple)
            {
                trackSimple.Reset();
            }
        }
        else
        {
            eqModeTextMeshProUGUI.text = "Switch to 3-Band EQ";
            foreach (var fmodManager in allTrackFmodManagers)
            {
                fmodManager.ResetEqGainParameters();
            }
        }
        isSimpleEqOn = !isSimpleEqOn;
        foreach (var trackOrbs in allTrackOrbManagers)
        {
            trackOrbs.isSimpleEQ = isSimpleEqOn;
        }
    }

    public void TurnOffSimpleEqInAll()
    {
        foreach (var orbReference in allTrackOrbManagers)
        {
            orbReference.TurnOffSimpleEqSelection();
        }
    }
}
