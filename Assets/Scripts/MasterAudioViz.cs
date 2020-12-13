using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterAudioViz : MonoBehaviour
{

    public GameObject masterBusControllerGO;

    private MasterBusController _masterBusController;

    private List<GameObject> leftCylinders;
    [SerializeField] private GameObject leftVisualsGO;

    private List<GameObject> rightCylinders;
    [SerializeField] private GameObject rightVisualsGO;

    public int numberOfMeterObjects;
    private int _meterBandsSize;
    [SerializeField] private float heightMul = 0.1f;
    private Vector3 startScale;

    public bool showVisualization = true;

    public int startBinNumber=0;
    public int endBinNumber=0;

    public float[] leftAvgDbs, rightAvgDbs;

    private bool mainTrackActive = false;
    
    // Start is called before the first frame update
    void Start()
    {
        numberOfMeterObjects = leftVisualsGO.transform.childCount;
        _masterBusController = masterBusControllerGO.GetComponent<MasterBusController>();

        leftCylinders = new List<GameObject>(numberOfMeterObjects);
        rightCylinders = new List<GameObject>(numberOfMeterObjects);
        leftAvgDbs = new float[numberOfMeterObjects];
        rightAvgDbs = new float[numberOfMeterObjects];
        foreach(Transform child in leftVisualsGO.transform)
        {
            leftCylinders.Add(child.gameObject);
        }
        
        foreach (Transform child in rightVisualsGO.transform)
        {
            rightCylinders.Add(child.gameObject);
        }

        endBinNumber = endBinNumber != 0 ? endBinNumber : _masterBusController.WindowSize;
        _meterBandsSize = (endBinNumber - startBinNumber) / numberOfMeterObjects;

        startScale = leftCylinders[0].transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (showVisualization)
        {
            for (int i = 0; i < numberOfMeterObjects; i++)
            {
                var (leftAvgAmplitude, rightAvgAmplitude) = GetMeterBandAverage(i);
                leftAvgDbs[i] = leftAvgAmplitude;
                rightAvgDbs[i] = rightAvgAmplitude;
                leftCylinders[i].transform.localScale = new Vector3(startScale.x,
                    startScale.y + leftAvgAmplitude * heightMul, startScale.z);
                rightCylinders[i].transform.localScale = new Vector3(startScale.x,
                    startScale.y + rightAvgAmplitude * heightMul, startScale.z);
            }
        }
    }

    private (float, float) GetMeterBandAverage(int bandNumber)
    {
        if (mainTrackActive)
        {
            // Returns average in dB
            float lsum = 0, rsum = 0;
            for (int i = startBinNumber + _meterBandsSize * bandNumber;
                i < startBinNumber + _meterBandsSize * bandNumber + _meterBandsSize; ++i)
            {
                lsum += General_Utilities.lin2dB(_masterBusController.leftChannelSpectrumData[i]);
                rsum += General_Utilities.lin2dB(_masterBusController.rightChannelSpectrumData[i]);
            }
            float leftAvg = lsum / _meterBandsSize;
            float rightAvg = rsum / _meterBandsSize;
            return (leftAvg, rightAvg);  
        }
        else
        {
            return (0f, 0f);
        }
    }


    public void ToggleMainTrackActive()
    {
        mainTrackActive = !mainTrackActive;
    }
}
