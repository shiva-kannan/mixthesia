using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tool_Reverb : MonoBehaviour
{
    enum ReverbState
    {
        NORMAL,
        CHANGINGDRY,
        CHANGINGWET,
    }

    [Header("Sensitivity Related")]
    [SerializeField] float triggerThreshold;
    [SerializeField] float resizeSensitivity;
    [SerializeField] float moveSensitivity;
    [SerializeField] Vector2 wetLevelRangedB;
    [SerializeField] Vector2 dryLevelRangedB;

    [HideInInspector] public Transform copyDisplay;
    [HideInInspector] public TrackFMODManager trackMng;

    private ReverbState currentState = ReverbState.NORMAL;

    private Transform reverbDisplay;
    private float ogScaleZ;
    private float capScaleZ;
    [HideInInspector] public float ogCopyZ;

    private bool isLeftTriggerDown = false;
    private bool isRightTriggerDown = false;

    private Transform handL;
    private Transform handR;
    private float wetInitialDistant = 0f;
    private float dryInitialZ = 0f;
    private Transform dryAffectiveHand = null;

    private Vector2 wetLevelRangeLin;
    private Vector2 dryLevelRangeLin;

    private ChurchPopulator _churchPopulator;

    // Start is called before the first frame update
    void Start()
    {
        reverbDisplay = GameObject.FindGameObjectWithTag("ReverbDisplay").transform;
        ogScaleZ = transform.localScale.z;
        capScaleZ = ogScaleZ * 4f;

        handL = GameObject.FindGameObjectWithTag("HandL").transform;
        handR = GameObject.FindGameObjectWithTag("HandR").transform;

        //Convert db range to linear range:
        wetLevelRangeLin = new Vector2(General_Utilities.dB2lin(wetLevelRangedB.x), General_Utilities.dB2lin(wetLevelRangedB.y));
        dryLevelRangeLin = new Vector2(General_Utilities.dB2lin(dryLevelRangedB.x), General_Utilities.dB2lin(dryLevelRangedB.y));

        _churchPopulator = GetComponent<ChurchPopulator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (reverbDisplay != null)
        {
            transform.position = Vector3.Lerp(transform.position, reverbDisplay.position, 7f * Time.deltaTime);
        }

        //Get trigger input:
        float triggerValL = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        float triggerValR = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (triggerValL >= triggerThreshold && !isLeftTriggerDown)
        {
            isLeftTriggerDown = true;
        }
        else if (triggerValL < triggerThreshold && isLeftTriggerDown)
        {
            isLeftTriggerDown = false;
        }

        if (triggerValR >= triggerThreshold && !isRightTriggerDown)
        {
            isRightTriggerDown = true;
        }
        else if (triggerValR < triggerThreshold && isRightTriggerDown)
        {
            isRightTriggerDown = false;
        }


        //Go to different states based on trigger states:
        if (isLeftTriggerDown && isRightTriggerDown)
        {
            if (currentState != ReverbState.CHANGINGWET)
            {
                wetInitialDistant = Vector3.Distance(handL.position, handR.position);
                currentState = ReverbState.CHANGINGWET;
            }
        }
        else
        {
            if (isLeftTriggerDown && !isRightTriggerDown)
            {
                if (currentState != ReverbState.CHANGINGDRY)
                {
                    dryAffectiveHand = handL;
                    dryInitialZ = dryAffectiveHand.position.z;
                    currentState = ReverbState.CHANGINGDRY;
                }
            }else if (!isLeftTriggerDown && isRightTriggerDown)
            {
                if (currentState != ReverbState.CHANGINGDRY)
                {
                    dryAffectiveHand = handR;
                    dryInitialZ = dryAffectiveHand.position.z;
                    currentState = ReverbState.CHANGINGDRY;
                }
            }
            else
            {
                currentState = ReverbState.NORMAL;
            }
        }


        //Change church size, orb pos, and reverb:
        if (currentState == ReverbState.CHANGINGWET)
        {
            float currentDist = Vector3.Distance(handL.position, handR.position);
            float expansion = currentDist - wetInitialDistant;

            float newZ = transform.localScale.z + expansion * resizeSensitivity;
            if (newZ > capScaleZ)
            {
                newZ = capScaleZ;
            }else if (newZ < ogScaleZ)
            {
                newZ = ogScaleZ;
            }

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newZ);

            // Change the wet value:
            float newWetLin = General_Utilities.Map(new Vector2(ogScaleZ, capScaleZ), wetLevelRangeLin, newZ);
            trackMng.reverbWetLevel = General_Utilities.lin2dB(newWetLin);


            wetInitialDistant = currentDist;

        }else if (currentState == ReverbState.CHANGINGDRY)
        {
            float currentHandZ = dryAffectiveHand.position.z;
            float expansion = currentHandZ - dryInitialZ;

            float newZ = copyDisplay.position.z + expansion * moveSensitivity;
            if (newZ > (transform.position.z + (transform.localScale.z / 2.0f)))
            {
                newZ = transform.position.z + (transform.localScale.z / 2.0f);
            }else if (newZ < ogCopyZ)
            {
                newZ = ogCopyZ;
            }

            copyDisplay.position = new Vector3(copyDisplay.position.x, copyDisplay.position.y, newZ);

            // Change the dry value:
            float newDryLin = General_Utilities.Map(new Vector2(transform.position.z + (transform.localScale.z / 2.0f), ogCopyZ), dryLevelRangeLin, newZ);
            trackMng.reverbDryLevel = General_Utilities.lin2dB(newDryLin);

            dryInitialZ = currentHandZ;
        }

    }

    private void OnDestroy()
    {
        // Since the church instances aren't parented, call the associated function to destroy the reverb related 
        // additional prefabs
        _churchPopulator.DestroyAllInstantiatedPrefabsOnDestroy();
    }
}
