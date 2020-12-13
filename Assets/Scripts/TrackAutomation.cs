using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrackAutomation : MonoBehaviour
{
    private int totalTime;
    private float totalTimeInS;

    private TimelineController _timelineController;
    private TrackOrb _trackOrbReference;

    // Automation control variables
    public bool isRecording = false;
    public bool isThereAutomation = false;
    private float currentTimeInS;
    private bool isKeyframeInvoking = false;
    [SerializeField] private float invokeRepeatTime;
    [SerializeField] private bool isEnabled;

    public float startTimeOfCurve = Single.MaxValue;
    public float endTimeOfCurve = -1f;
    
    public AnimationCurve posXCurve = new AnimationCurve();
    public AnimationCurve posYCurve = new AnimationCurve();
    public AnimationCurve posZCurve = new AnimationCurve();

    [SerializeField] ParticleSystem starDust;
    
    // Start is called before the first frame update
    void Start()
    {
        _timelineController = GetComponentInParent<TimelineController>();
        _trackOrbReference = GetComponent<TrackOrb>();

        totalTime = _timelineController.totalTimeInMS;
        totalTimeInS = General_Utilities.milliS2Seconds(totalTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isThereAutomation)
        {
            _trackOrbReference.LerpToOGColor();
        }
        else
        {
            _trackOrbReference.LerpToAutomationColor();

            //Play Animation clip with the Animation component
            if (!isRecording)
            {
                // Update only if the current time falls in the range of startOfCurve and endOfCurve time
                if (currentTimeInS >= startTimeOfCurve && currentTimeInS <= endTimeOfCurve)
                {
                    // Use the curve information to update the position of the orbs
                    transform.position = new Vector3(posXCurve.Evaluate(currentTimeInS), posYCurve.Evaluate(currentTimeInS), posZCurve.Evaluate(currentTimeInS));   
                }
            }
        }

        currentTimeInS = General_Utilities.milliS2Seconds(_timelineController.currentTimeInMS);

        if (isRecording && isEnabled)
        {
            isThereAutomation = true;
            // Call a function to set key frame at regular intervals
            if (!isKeyframeInvoking)
            {
                isKeyframeInvoking = true;
                if (currentTimeInS < startTimeOfCurve)
                {
                    startTimeOfCurve = currentTimeInS;
                }
                InvokeRepeating("SetKeyFrameCurrent", 0f, invokeRepeatTime);
            }

            if (!starDust.isPlaying)
            {
                var main = starDust.main;
                main.startColor = _trackOrbReference.GetAutomationColor();
                starDust.Play();
            }
        }
        else
        {
            // Stop the invoke call
            if (isKeyframeInvoking)
            {
               CancelInvoke("SetKeyFrameCurrent");
               isKeyframeInvoking = false;
            }

            starDust.Stop();
        }
    }

    private void SetKeyFrameCurrent()
    {
        //Debug.Log($"Adding new keyframe at time: {currentTimeInS}");
        if (currentTimeInS > endTimeOfCurve)
        {
            endTimeOfCurve = currentTimeInS;
        }
        posXCurve.AddKey(new Keyframe(currentTimeInS, transform.position.x));
        posYCurve.AddKey(new Keyframe(currentTimeInS, transform.position.y));
        posZCurve.AddKey(new Keyframe(currentTimeInS, transform.position.z));
    }

    public float GetCurrentTimeInS()
    {
        return currentTimeInS;
    }

    public void EraseAutomationData()
    {
        // Make the anim curves flat
        posXCurve = new AnimationCurve();
        posYCurve = new AnimationCurve();
        posZCurve = new AnimationCurve();
        
        // Set all the booleans back to default
        isRecording = false;
        isThereAutomation = false;
        isKeyframeInvoking = false;
        
        // Clear the drawing
        GetComponentInChildren<TrackFieldAxisDrawer>().EraseAutomationPoints();
    }
}
