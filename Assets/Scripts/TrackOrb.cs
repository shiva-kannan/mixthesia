using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class TrackOrb : Clickable
{
 
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] public TrackField tField;
    [SerializeField] public AudioCube aCube;
    [SerializeField] public Transform interactionPlane_posReference;
    [SerializeField] private Vector2 interactionPlane_offsetRange;
    [SerializeField] private GameObject aCube_interactionPlane;
    [SerializeField] private GameObject eqThresholdPrefab;
    [SerializeField] private GameObject eqFilterPrefab;
    [SerializeField] private GameObject[] eqSimplPrefabs;
    [SerializeField] private SpriteRenderer instruIcon;
    

    [Header("----------Look Related----------")]
    [SerializeField] Material orbMat;
    [SerializeField] Material freqRectMat;
    [SerializeField] private GameObject freqVisL;
    [SerializeField] private GameObject freqVisR;
    [SerializeField] private Vector2 freqRectSizeRange;
    [SerializeField] private float freqRectDrawThreshold;
    [SerializeField] private Vector2 freqRectHeightRange;

    [Header("----------Behavior Related----------")]
    [SerializeField] public bool isSelectLocked;
    [SerializeField] public bool isBornSelected;
    [SerializeField] public bool isAlwaysSelected;
    [SerializeField] public bool isByPassPosition;
    public bool isInAudioCube;
    public bool isSimpleEQ;
    private bool isEQOn;

    [Header("")]
    [SerializeField] GameObject orbCopy;
    [SerializeField] public General_ColorPool colorPool;
    [SerializeField] Vector2 volToColorRange;
    [SerializeField] float volToSizeMulti;

    private Vector3 ogSize = Vector3.zero;
    private MeshRenderer myMR;
    private Color normalC;
    private Color automationC;
    private float sizeOffset = 0f;

    private GameObject interactionPlane;
    private GameObject[] freqVisualization = new GameObject[0];
    private GameObject[] eqThresholds = new GameObject[0];
    private GameObject[] eqFilters = new GameObject[0];
    private GameObject[] eqSimpOptions = new GameObject[0];

    private TrackFMODManager myFMManager;
    private TrackAutomation _trackAutomation;
    private TrackFFTContainer _trackFFT;
    private MasterBusController _masterBusController;

    // Start is called before the first frame update
    void Start()
    {
        ogSize = transform.localScale;

        normalC = colorPool.GetOnColor();
        automationC = colorPool.GetUniqueRandonColor();

        myMR = GetComponent<MeshRenderer>();
        myMR.material = new Material(orbMat);
        myMR.material.SetColor(General_ShaderID.rimMainColor, normalC);

        myFMManager = GetComponent<TrackFMODManager>();
        _trackAutomation = GetComponent<TrackAutomation>();
        _trackFFT = GetComponent<TrackFFTContainer>();
        _masterBusController = GetComponentInParent<MasterBusController>();

        StartCoroutine(PlaceOrb());


        if (isBornSelected)
        {
            OnTriggerDown();
        }

        instruIcon.color = automationC;

    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        if (myFMManager.isMute)
        {
            myMR.material.SetColor(General_ShaderID.rimMainColor, colorPool.GetOffColor());
            myMR.material.SetFloat(General_ShaderID.rimScrollSpd, 0f);

            sizeOffset = 0f;
        }
        else
        {
            float volumeFloat = General_Utilities.dB2lin(myFMManager.dbFloatValue);
            //Debug.Log("volumelin: " + volumeFloat);
            float colorRatio = General_Utilities.Map(volToColorRange, new Vector2(0f, 1f), volumeFloat);
            //Debug.Log("RATIO: " + colorRatio);
            
            //Color targetColor = Color.Lerp(normalC, colorPool.GetloudColor(), colorRatio);
            //Color c = Color.Lerp(myMR.material.GetColor(General_ShaderID.rimMainColor), targetColor, 3.5f * Time.deltaTime);

            //myMR.material.SetColor(General_ShaderID.rimMainColor, c);
            myMR.material.SetFloat(General_ShaderID.rimScrollSpd, 0.4f);
            sizeOffset = colorRatio * volToSizeMulti;


            float panningValue = GetMappedPanning();
        }

        if (isInAudioCube)
        {
            //Change Y-pos in Audio Cube
            if (!myFMManager.isMute && myFMManager.dbFloatValue > -72f)
            {
                float centerFreq = _trackFFT.bothChannelAverageEnergyFreq;
                //float targetPosY = General_Utilities.Map(new Vector2(20f, 20000f), aCube.GetAxisRanges()[1], centerFreq);
                float targetPosY = General_Utilities.GetMappedYForLogFrequency(centerFreq, aCube.GetAxisLength(),
                    aCube.GetAxisRanges()[1].x);
                if (!(targetPosY == Mathf.NegativeInfinity))
                {
                    Vector3 targetPos = new Vector3(transform.position.x, targetPosY, transform.position.z);
                    transform.position = Vector3.Lerp(transform.position, targetPos, 1f * Time.deltaTime);
                }
            }
            else
            {
                Vector3 targetPos = new Vector3(transform.position.x, aCube.GetAxisRanges()[1].x, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, 1f * Time.deltaTime);
            }

            //Draw freqency visualization
            int windowSize = _trackFFT.WindowSize;
            float rectHeight = aCube.GetAxisLength() / windowSize;
            if (freqVisualization.Length == 0)
            {
                freqVisualization = new GameObject[2 * windowSize];
                for (int i = 0; i < windowSize; i++)
                {
                    GameObject lRect = Instantiate(freqVisL);
                    GameObject rRect = Instantiate(freqVisR);

                    //Set the color:
                    MeshRenderer lMR = lRect.transform.GetChild(0).GetComponent<MeshRenderer>();
                    MeshRenderer rMR = rRect.transform.GetChild(0).GetComponent<MeshRenderer>();

                    lMR.material = new Material(freqRectMat);
                    rMR.material = new Material(freqRectMat);

                    /*
                    lMR.material.SetColor(General_ShaderID.edgeHighMainColor, automationC);
                    lMR.material.SetColor(General_ShaderID.edgeHighOutlineColor, automationC);
                    rMR.material.SetColor(General_ShaderID.edgeHighMainColor, automationC);
                    rMR.material.SetColor(General_ShaderID.edgeHighOutlineColor, automationC);*/
                    lMR.material.color = automationC;
                    rMR.material.color = automationC;

                    lRect.transform.localScale = Vector3.one * rectHeight;
                    rRect.transform.localScale = Vector3.one * rectHeight;

                    freqVisualization[2 * i] = lRect;
                    freqVisualization[2 * i + 1] = rRect;
                }
            }

            float[] lChannel = _trackFFT.leftChannelSpectrumData;
            float[] rChannel = _trackFFT.rightChannelSpectrumData;

            Vector2 yRange = aCube.GetAxisRanges()[1];
            float yCubeLength = aCube.GetAxisLength();
            float currentY = yRange.x + rectHeight / 2;
            float logMinFreq = Mathf.Log10(20);
            float samplerateLogDifference = Mathf.Log10((float)_trackFFT.fmodSampleRate / 2) - logMinFreq;
            float logarithmicY;
            float adjustedLogY;
            float currentRectangleHeight;
            float previousAdjustedLogY = yRange.x;
            for (int i = 0; i < windowSize; i++)
            {
                //Position the rect
                logarithmicY = Mathf.Log10((float)(i + 0.5) * (_trackFFT.fmodSampleRate) / (windowSize * 2));
                adjustedLogY = ((logarithmicY - logMinFreq) * yCubeLength / samplerateLogDifference) + yRange.x;
                currentRectangleHeight = (adjustedLogY - previousAdjustedLogY) / 2;

                //Test the height:
                if (currentRectangleHeight < freqRectHeightRange.x)
                {
                    currentRectangleHeight = freqRectHeightRange.x;
                }else if (currentRectangleHeight > freqRectHeightRange.y)
                {
                    currentRectangleHeight = freqRectHeightRange.y;
                }


                previousAdjustedLogY = adjustedLogY;
                freqVisualization[2 * i].transform.position = new Vector3(transform.position.x, adjustedLogY, transform.position.z);
                freqVisualization[2 * i + 1].transform.position = new Vector3(transform.position.x, adjustedLogY, transform.position.z);

                //Scale the rect
                float leftX = General_Utilities.Map(new Vector2(0f, 100f), freqRectSizeRange, lChannel[i] * 100f);
                float rightX = General_Utilities.Map(new Vector2(0f, 100f), freqRectSizeRange, rChannel[i] * 100f);
                Vector3 targetScaleL;
                Vector3 targetScaleR;
                if (leftX < freqRectDrawThreshold)
                {
                    leftX = 0f;
                    freqVisualization[2 * i].SetActive(false);
                }
                else
                {
                    freqVisualization[2 * i].SetActive(true);
                }
                targetScaleL = new Vector3(leftX, currentRectangleHeight, currentRectangleHeight);
                freqVisualization[2 * i].transform.localScale = Vector3.Lerp(freqVisualization[2 * i].transform.localScale, targetScaleL, 7f * Time.deltaTime);

                if (rightX < freqRectDrawThreshold)
                {
                    rightX = 0f;
                    freqVisualization[2 * i + 1].SetActive(false);
                }
                else
                {
                    freqVisualization[2 * i + 1].SetActive(true);
                }
                targetScaleR = new Vector3(rightX, currentRectangleHeight, currentRectangleHeight);
                freqVisualization[2 * i + 1].transform.localScale = Vector3.Lerp(freqVisualization[2 * i + 1].transform.localScale, targetScaleR, 7f * Time.deltaTime);

                currentY += rectHeight;
            }

        }
    }

    public override void Hover()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale - sizeOffset * Vector3.one, 2.0f * ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale + sizeOffset * Vector3.one;
        //transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);

        if (!interactionPlane)
        {
            interactionPlane = Instantiate(aCube_interactionPlane);

            float yOffset;
            float playerH = General_Utilities.GetPlayerHeight();
            Vector2 cubeHeightRange = aCube.GetAxisRanges()[1];
            float heightDiffMax = Mathf.Max(Mathf.Abs(cubeHeightRange.y - playerH), Mathf.Abs(playerH - cubeHeightRange.x));
            float offsetMag = General_Utilities.Map(new Vector2(heightDiffMax, 0f), interactionPlane_offsetRange, Mathf.Abs(transform.position.y - playerH));

            //if (transform.position.y > playerH)
            //{
            //    yOffset = -offsetMag;
            //}
            //else
            //{
            //    yOffset = offsetMag;
            //}
            yOffset = offsetMag;


            Vector3 pos = new Vector3(interactionPlane_posReference.position.x, transform.position.y + yOffset, interactionPlane_posReference.position.z);
            interactionPlane.transform.position = pos;
        }
    }

    public override void Normal()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale - sizeOffset * Vector3.one, ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale + sizeOffset * Vector3.one;
        //transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);

        if (interactionPlane)
        {
            Destroy(interactionPlane);
        }
    }


    private Vector3 ClipPositionInRange(Vector3 vectTotest)
    {
        if (isInAudioCube)
        {
            Vector2[] axisRanges = aCube.GetAxisRanges();
            Vector2 xrange = axisRanges[0];
            Vector2 zrange = axisRanges[2];

            if (vectTotest.x > xrange.y)
            {
                vectTotest.x = xrange.y;
            }else if (vectTotest.x < xrange.x)
            {
                vectTotest.x = xrange.x;
            }

            vectTotest.y = transform.position.y;

            if (vectTotest.z > zrange.y)
            {
                vectTotest.z = zrange.y;
            }
            else if (vectTotest.z < zrange.x)
            {
                vectTotest.z = zrange.x;
            }

            //Bypass Z:
            vectTotest.z = transform.position.z;

            return vectTotest;
        }
        else
        {
            Vector2 allowedAngle = tField.GetAngleRange();
            Vector2 allowedDist = tField.GetDistanceRange();
            Vector3 refPoint = tField.GetReferenceStart();

            Vector3 refToTarget = vectTotest - refPoint;
            float angle = General_Utilities.GetSignedAngle(tField.GetMidAxis(), refToTarget, "X");
            float distance = refToTarget.magnitude;

            //Test angle:
            if (angle > allowedAngle.y) // if it's beyond the right boundary
            {
                refToTarget = tField.GetRightAxis().normalized * distance;
                vectTotest = refPoint + refToTarget;
            }
            else if (angle < allowedAngle.x) // if it's beyond the left boundary
            {
                refToTarget = tField.GetLeftAxis().normalized * distance;
                vectTotest = refPoint + refToTarget;
            }

            //Test distance:
            if (distance > allowedDist.y)
            {
                refToTarget = refToTarget.normalized * allowedDist.y;
                vectTotest = refPoint + refToTarget;
            }
            else if (distance < allowedDist.x)
            {
                refToTarget = refToTarget.normalized * allowedDist.x;
                vectTotest = refPoint + refToTarget;
            }

            return vectTotest;
        }
    }

    public override void OnGrabbed(Vector3 grabToPosition)
    {
        // Change the position of the track orb to that of the plane
        Vector3 newPosition = new Vector3(grabToPosition.x, grabToPosition.y + yOffset, grabToPosition.z);
        newPosition = ClipPositionInRange(newPosition);
        transform.position = newPosition;
    }

    public float GetMappedVolume()
    {
        if (isByPassPosition)
        {
            return 2.5f;
        }

        if (isInAudioCube)
        {
            Vector2 zrange = aCube.GetAxisRanges()[2];
            float volume = General_Utilities.Map(zrange, new Vector2(1.0f, 0.1f), transform.position.z);
            return volume;

        }
        else
        {
            float distance = TF_GetCenterToPos().magnitude;
            float volume = General_Utilities.Map(tField.GetDistanceRange(), new Vector2(1.0f, 0.1f), distance);
            return volume;
        }
      
    }

    public float GetMappedPanning()
    {
        if (isByPassPosition)
        {
            return 0f;
        }

        if (isInAudioCube)
        {
            Vector2 xrange = aCube.GetAxisRanges()[0];
            float panning = General_Utilities.Map(xrange, new Vector2(-1f, 1f), transform.position.x);
            return panning;

        }
        else
        {
            float angle = General_Utilities.GetSignedAngle(tField.GetMidAxis(), TF_GetCenterToPos(), "X");
            float panning = General_Utilities.Map(tField.GetAngleRange(), new Vector2(-1f, 1f), angle);
            return panning;
        }
    }

    public Vector3 TF_GetCenterToPos()
    {
        return transform.position - tField.GetReferenceStart();
    }

    public override void OnButtonTwoDown()
    {
        bool toSet = myFMManager.isMute;
        myFMManager.EnableDisable(toSet);
    }

    public override void OnButtonOneDown()
    {
        _trackAutomation.EraseAutomationData();
    }

    public override void OnTriggerDown()
    {
        if (!isSelectLocked)
        {
            if (isInAudioCube)
            {
                if (isEQOn)
                {
                    TurnEQ(false);
                }
                else
                {
                    TurnEQ(true);
                }
            }
            else
            {
                //Instantiate the copy:
                GameObject clone = Instantiate(orbCopy) as GameObject;
                TrackOrbCopied cloneMngr = clone.GetComponent<TrackOrbCopied>();

                //Initialize the copy's parameters:
                cloneMngr.SetSizeAndPosition(ogSize, transform.position);
                cloneMngr.SetIconAndName(transform.GetChild(0).GetComponent<SpriteRenderer>().sprite, transform.GetChild(1).GetComponent<TextMeshPro>().text);
                cloneMngr.SetMainColorAndScrollSpd(normalC, 0.4f);
                cloneMngr.SetOrigin(this.transform);
                cloneMngr.isFixedOnDisplay = isAlwaysSelected;

                //Make the track field sink:
                tField.HideAndReveal(true);
            }
        }
    }

    public void TurnEQ(bool on)
    {
        if (isSimpleEQ && on)
        {
            // Switch off every simple eq selection
            _masterBusController.TurnOffSimpleEqInAll();
        }
        if (!isEQOn && on)
        {
            //Turn on EQ:
            if (isSimpleEQ)
            {
                Tool_EQ_Simple _simpEQReference = GetComponent<Tool_EQ_Simple>();
                if (eqSimpOptions.Length == 0)
                {
                    eqSimpOptions = new GameObject[2];

                    GameObject muddyObj = Instantiate(eqSimplPrefabs[0]);
                    muddyObj.transform.position = transform.position;
                    muddyObj.GetComponent<TrackTool>()._trackSimpEQReference = _simpEQReference;

                    GameObject brightObj = Instantiate(eqSimplPrefabs[1]);
                    brightObj.transform.position = transform.position;
                    brightObj.GetComponent<TrackTool>()._trackSimpEQReference = _simpEQReference;

                    muddyObj.SetActive(true);
                    brightObj.SetActive(true);

                    eqSimpOptions[0] = muddyObj;
                    eqSimpOptions[1] = brightObj;
                }
                else
                {
                    eqSimpOptions[0].transform.position = transform.position;
                    eqSimpOptions[0].SetActive(true);
                    eqSimpOptions[1].transform.position = transform.position;
                    eqSimpOptions[1].SetActive(true);
                }

                _simpEQReference._optionReferences = new GameObject[2];
                _simpEQReference._optionReferences[0] = eqSimpOptions[0];
                _simpEQReference._optionReferences[1] = eqSimpOptions[1];
                _simpEQReference.isOn = true;

            }
            else
            {
                if (eqThresholds.Length == 0)
                {
                    eqThresholds = new GameObject[2];

                    GameObject thresholdLow = Instantiate(eqThresholdPrefab);
                    Tool_EQ_Threshold low = thresholdLow.GetComponent<Tool_EQ_Threshold>();
                    thresholdLow.transform.position = transform.position;
                    low._trackFMODReference = myFMManager;
                    low._trackOrbReference = this;
                    low.isMidHigh = false;

                    GameObject thresholdHigh = Instantiate(eqThresholdPrefab);
                    Tool_EQ_Threshold high = thresholdHigh.GetComponent<Tool_EQ_Threshold>();
                    thresholdHigh.transform.position = transform.position;
                    high._trackFMODReference = myFMManager;
                    high._trackOrbReference = this;
                    high.isMidHigh = true;

                    thresholdLow.SetActive(true);
                    thresholdHigh.SetActive(true);

                    eqThresholds[0] = thresholdLow;
                    eqThresholds[1] = thresholdHigh;
                }
                else
                {
                    eqThresholds[0].transform.position = transform.position;
                    eqThresholds[0].SetActive(true);
                    eqThresholds[1].transform.position = transform.position;
                    eqThresholds[1].SetActive(true);
                }

                if (eqFilters.Length == 0)
                {
                    eqFilters = new GameObject[3];

                    GameObject lowFilter = Instantiate(eqFilterPrefab);
                    Tool_EQ_Filter low = lowFilter.GetComponent<Tool_EQ_Filter>();
                    lowFilter.transform.position = transform.position;
                    low.lowThreshold = eqThresholds[0].transform;
                    low.highThreshold = eqThresholds[1].transform;
                    low._trackFMODReference = myFMManager;
                    low._trackOrbReference = this;
                    low.SetType(0);

                    GameObject midFilter = Instantiate(eqFilterPrefab);
                    Tool_EQ_Filter mid = midFilter.GetComponent<Tool_EQ_Filter>();
                    midFilter.transform.position = transform.position;
                    mid.lowThreshold = eqThresholds[0].transform;
                    mid.highThreshold = eqThresholds[1].transform;
                    mid._trackFMODReference = myFMManager;
                    mid._trackOrbReference = this;
                    mid.SetType(1);

                    GameObject highFilter = Instantiate(eqFilterPrefab);
                    Tool_EQ_Filter high = highFilter.GetComponent<Tool_EQ_Filter>();
                    highFilter.transform.position = transform.position;
                    high.lowThreshold = eqThresholds[0].transform;
                    high.highThreshold = eqThresholds[1].transform;
                    high._trackFMODReference = myFMManager;
                    high._trackOrbReference = this;
                    high.SetType(2);

                    lowFilter.SetActive(true);
                    midFilter.SetActive(true);
                    highFilter.SetActive(true);

                    eqFilters[0] = lowFilter;
                    eqFilters[1] = midFilter;
                    eqFilters[2] = highFilter;
                }
                else
                {
                    eqFilters[0].transform.position = transform.position;
                    eqFilters[0].SetActive(true);
                    eqFilters[1].transform.position = transform.position;
                    eqFilters[1].SetActive(true);
                    eqFilters[2].transform.position = transform.position;
                    eqFilters[2].SetActive(true);
                }
            }

            isEQOn = true;
        }
        else if (isEQOn && !on)
        {
            //Turn off EQ:
            if (isSimpleEQ)
            {
                eqSimpOptions[0].SetActive(false);
                eqSimpOptions[1].SetActive(false);
                GetComponent<Tool_EQ_Simple>().isOn = false;
            }
            else
            {
                eqThresholds[0].SetActive(false);
                eqThresholds[1].SetActive(false);

                eqFilters[0].SetActive(false);
                eqFilters[1].SetActive(false);
                eqFilters[2].SetActive(false);
            }

            isEQOn = false;
        }
    }


    public void BringBackTF()
    {
        tField.HideAndReveal(false);
    }

    public Vector3 TF_GetOffsetToOrigin()
    {
        return tField.offsetToOrigin;
    }

    public Vector2[] AC_GetAxisRanges()
    {
        return aCube.GetAxisRanges();
    }


    public Color GetNormalColor()
    {
        return normalC;
    }

    public Color GetAutomationColor()
    {
        return automationC;
    }

    IEnumerator PlaceOrb()
    {
        yield return new WaitForSeconds(1f);
        if (isInAudioCube)
        {
            transform.position = aCube.GetRandomStartPos();
        }
        else
        { 
            transform.position = tField.GetRandomStartPos();
        }
    }

    private void LerpToColor(Color c)
    {
        Color targetC = Color.Lerp(myMR.material.GetColor(General_ShaderID.rimMainColor), c, 3.5f * Time.deltaTime);
        myMR.material.SetColor(General_ShaderID.rimMainColor, targetC);
    }

    public void LerpToAutomationColor()
    {
        LerpToColor(automationC);
    }

    public void LerpToOGColor()
    {
        LerpToColor(normalC);
    }

    public void TurnOffSimpleEqSelection()
    {
        if (eqSimpOptions.Length > 0)
        {
            eqSimpOptions[0].SetActive(false);
            eqSimpOptions[1].SetActive(false);
            GetComponent<Tool_EQ_Simple>().isOn = false;
        }
    }
}
