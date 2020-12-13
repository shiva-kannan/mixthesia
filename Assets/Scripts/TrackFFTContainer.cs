using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Specialized;
using System.Linq;

public class TrackFFTContainer : MonoBehaviour
{

    private TrackFMODManager _trackFmodManager;
    [SerializeField] public int fmodSampleRate = 48000;
    private int nyquistRate;
    private float hertzPerEntry;
    public int WindowSize = 16;
    public bool fft_read_enabled = false;
    public bool fft_dsp_added = false;
    [HideInInspector] public float[] leftChannelSpectrumData;
    [HideInInspector] public float[] rightChannelSpectrumData;
    [HideInInspector] public float[] bothChannelAvgSpectrumData;
    [HideInInspector] public float[] leftChannelSpectrumDb;
    [HideInInspector] public float[] rightChannelSpectrumDb;
    public List<float> stereoFrequencySortedMap;
    // not needed
    [HideInInspector] public float leftChannelMaxInLinear;
    [HideInInspector] public float rightChannelMaxInLinear;
    // ^ not needed
    [Range(0, 22000)] public float leftAverageEnergyFreq = 0f;
    [Range(0, 22000)] public float rightAverageEnergyFreq = 0f;
    [Range(0, 22000)] public float bothChannelAverageEnergyFreq = 0f;
    public float leftAvgOvertime = 0f;
    public float rightAvgOvertime = 0f;
    
    private int fft_enable_counter = 0;
     
    FMOD.DSP _track_fft;
    
    // Start is called before the first frame update
    void Start()
    {
        _trackFmodManager = GetComponent<TrackFMODManager>();
        nyquistRate = fmodSampleRate / 2;
        hertzPerEntry = nyquistRate / WindowSize;
        
        leftChannelSpectrumData = new float[WindowSize];
        rightChannelSpectrumData = new float[WindowSize];
        bothChannelAvgSpectrumData = new float[WindowSize];
        leftChannelSpectrumDb = new float[WindowSize];
        rightChannelSpectrumDb = new float[WindowSize];
        stereoFrequencySortedMap = new List<float>(WindowSize);
        
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _track_fft);
        _track_fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
        _track_fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, WindowSize * 2);

        leftChannelMaxInLinear = float.MinValue;
        rightChannelMaxInLinear = float.MinValue;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fft_read_enabled)
        {
            fft_enable_counter += 1;
            IntPtr unmanagedData;
            uint length;
            _track_fft.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out unmanagedData, out length);
            FMOD.DSP_PARAMETER_FFT fftData = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(FMOD.DSP_PARAMETER_FFT));
            var spectrum = fftData.spectrum;
            
            if (fftData.numchannels > 0)
            {
                var leftWeightedAverage = 0f;
                var leftArea = 0f;
                var rightWeightedAverage = 0f;
                var rightArea = 0f;
                Dictionary<float, float> frequencyMap = new Dictionary<float, float>(WindowSize);
                for (int i = 0; i < WindowSize; ++i)
                {
                    var frequencyBinValue = (float) (i + 0.5) * hertzPerEntry;
                    leftArea += hertzPerEntry * spectrum[0][i];
                    leftChannelSpectrumData[i] = spectrum[0][i];
                    leftWeightedAverage += spectrum[0][i] * frequencyBinValue * hertzPerEntry;
                    leftChannelSpectrumDb[i] = General_Utilities.lin2dB(spectrum[0][i]);
                    if (leftChannelSpectrumData[i] > leftChannelMaxInLinear)
                    {
                        leftChannelMaxInLinear = leftChannelSpectrumData[i];
                    }

                    rightArea += hertzPerEntry * spectrum[1][i];
                    rightChannelSpectrumData[i] = spectrum[1][i];
                    rightWeightedAverage += spectrum[1][i] * frequencyBinValue * hertzPerEntry;
                    rightChannelSpectrumDb[i] = General_Utilities.lin2dB(spectrum[1][i]);
                    if (rightChannelSpectrumData[i] > rightChannelMaxInLinear)
                    {
                        rightChannelMaxInLinear = rightChannelSpectrumData[i];
                    }

                    bothChannelAvgSpectrumData[i] = (leftChannelSpectrumData[i] + rightChannelSpectrumData[i]) / 2;

                    frequencyMap[frequencyBinValue] = bothChannelAvgSpectrumData[i];
                }

                frequencyMap.OrderByDescending(key => key.Value);
                var keys = from element in frequencyMap
                    orderby element.Value descending 
                    select element.Key;
                stereoFrequencySortedMap.Clear();
                foreach (var key in keys)
                {
                    stereoFrequencySortedMap.Add(key);
                }

                leftAverageEnergyFreq = leftWeightedAverage / leftArea;
                rightAverageEnergyFreq = rightWeightedAverage / rightArea;
                bothChannelAverageEnergyFreq = (leftAverageEnergyFreq + rightAverageEnergyFreq) / 2;
            }
        }
    }

    public void AddTrackFFTToChannelGroup()
    {
        fft_dsp_added = true;
        _trackFmodManager._trackChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _track_fft);
    }
}
