using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_EQ_Filter : Clickable
{
    [SerializeField] private Material filterMat;
    [SerializeField] private Color normalOLC;
    [SerializeField] private Vector2 faceOPRange;
    [SerializeField] private Vector2 bandGainRange;
    [SerializeField] private float eqSensitivity;
    private bool isMatGenerated = false;
    private MeshRenderer myMR;

    [HideInInspector] public Transform highThreshold;
    [HideInInspector] public Transform lowThreshold;
    [HideInInspector] public TrackOrb _trackOrbReference;
    [HideInInspector] public TrackFMODManager _trackFMODReference;

    enum FilterType
    {
        LOW,
        MID,
        HIGH
    }

    private FilterType myType = FilterType.HIGH;

    // Start is called before the first frame update
    void Start()
    {
        myMR = GetComponent<MeshRenderer>();
        myMR.material = new Material(filterMat);
        isMatGenerated = true;
    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        base.AdditionalUpdate();

        //Position the filter and change opacity
        Vector2[] axisRanges = _trackOrbReference.AC_GetAxisRanges();
        Vector2 yRange = axisRanges[1];

        Vector3 targetPos;
        Vector3 targetScale;
        if (myType == FilterType.LOW)
        {
            float scaleY = lowThreshold.transform.position.y - yRange.x;
            float yPos = yRange.x + (scaleY / 2f);

            targetScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
            targetPos = new Vector3(_trackOrbReference.transform.position.x, yPos, _trackOrbReference.transform.position.z);

            float newOP = General_Utilities.Map(bandGainRange, faceOPRange, _trackFMODReference.eqLowGain);
            myMR.material.SetFloat(General_ShaderID.edgeHighFaceOpacity, newOP);
        }else if (myType == FilterType.MID)
        {
            float scaleY = highThreshold.transform.position.y - lowThreshold.transform.position.y;
            float yPos = lowThreshold.transform.position.y + (scaleY / 2f);

            targetScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
            targetPos = new Vector3(_trackOrbReference.transform.position.x, yPos, _trackOrbReference.transform.position.z);

            float newOP = General_Utilities.Map(bandGainRange, faceOPRange, _trackFMODReference.eqMidGain);
            myMR.material.SetFloat(General_ShaderID.edgeHighFaceOpacity, newOP);
        }
        else
        {
            float scaleY = yRange.y - highThreshold.transform.position.y;
            float yPos = highThreshold.transform.position.y + (scaleY / 2f);

            targetScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
            targetPos = new Vector3(_trackOrbReference.transform.position.x, yPos, _trackOrbReference.transform.position.z);

            float newOP = General_Utilities.Map(bandGainRange, faceOPRange, _trackFMODReference.eqHighGain);
            myMR.material.SetFloat(General_ShaderID.edgeHighFaceOpacity, newOP);
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, 7f * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 7f * Time.deltaTime);
    }

    public override void Normal()
    {
        base.Normal();
        if (!isMatGenerated)
        {
            myMR = GetComponent<MeshRenderer>();
            myMR.material = new Material(filterMat);
            isMatGenerated = true;
        }

        Color currentOLC = myMR.material.GetColor(General_ShaderID.edgeHighOutlineColor);
        Color targetOLC = Color.Lerp(currentOLC, normalOLC, 7f * Time.deltaTime);
        myMR.material.SetColor(General_ShaderID.edgeHighOutlineColor, targetOLC);
    }

    public override void Hover()
    {
        base.Hover();
        if (!isMatGenerated)
        {
            myMR = GetComponent<MeshRenderer>();
            myMR.material = new Material(filterMat);
            isMatGenerated = true;
        }

        Color currentOLC = myMR.material.GetColor(General_ShaderID.edgeHighOutlineColor);
        Color targetOLC = Color.Lerp(currentOLC, Color.yellow, 7f * Time.deltaTime);
        myMR.material.SetColor(General_ShaderID.edgeHighOutlineColor, targetOLC);

    }

    public override void ThumbstickPush(Vector2 thumbstickVal)
    {
        base.ThumbstickPush(thumbstickVal);
        float yVal = thumbstickVal.y;

        if (myType == FilterType.LOW)
        {
            float newGain = _trackFMODReference.eqLowGain + yVal * eqSensitivity;
            if (newGain > bandGainRange.y)
            {
                newGain = bandGainRange.y;
            }else if (newGain < bandGainRange.x)
            {
                newGain = bandGainRange.x;
            }
            _trackFMODReference.eqLowGain = newGain;
        }else if (myType == FilterType.MID)
        {
            float newGain = _trackFMODReference.eqMidGain + yVal * eqSensitivity;
            if (newGain > bandGainRange.y)
            {
                newGain = bandGainRange.y;
            }
            else if (newGain < bandGainRange.x)
            {
                newGain = bandGainRange.x;
            }
            _trackFMODReference.eqMidGain = newGain;
        }
        else
        {
            float newGain = _trackFMODReference.eqHighGain + yVal * eqSensitivity;
            if (newGain > bandGainRange.y)
            {
                newGain = bandGainRange.y;
            }
            else if (newGain < bandGainRange.x)
            {
                newGain = bandGainRange.x;
            }
            _trackFMODReference.eqHighGain = newGain;
        }

    }

    public override void OnTriggerDown()
    {
        base.OnTriggerDown();
        myState = ClickableState.NORMAL;
        myMR.material.SetColor(General_ShaderID.edgeHighOutlineColor, normalOLC);
        _trackOrbReference.OnTriggerDown();
    }

    public void SetType(int type)
    {
        if (type == 0)
        {
            myType = FilterType.LOW;
            return;
        }else if (type == 1)
        {
            myType = FilterType.MID;
            return;
        }
        else
        {
            myType = FilterType.HIGH;
            return;
        }
    }


}
