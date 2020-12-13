using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tool_Reverb_Opacity : MonoBehaviour
{
    enum ReverbState
    {
        NORMAL,
        CHANGINGDRY,
        CHANGINGWET,
    }

    [SerializeField] MeshRenderer churchMesh;

    [Header("Sensitivity Related")]
    [SerializeField] float triggerThreshold;
    [SerializeField] float resizeSensitivity;
    [SerializeField] float wetChangeSensitivity;
    [SerializeField] float dryChangeSensitivity;
    [SerializeField] Vector2 wetLevelRangedB;
    [SerializeField] Vector2 dryLevelRangedB;
    [SerializeField] Vector2 overlayRangeWet;
    [SerializeField] Vector2 overlayRangeDry;

    [HideInInspector] public Transform copyDisplay;

    private ReverbState currentState = ReverbState.NORMAL;

    private float ogScaleZ;
    private float capScaleZ;
    [HideInInspector] public float ogCopyZ;

    private bool isLeftTriggerDown = false;
    private bool isRightTriggerDown = false;

    private Transform handL;
    private Transform handR;
    private float wetInitialY = 0f;
    private float dryInitialY = 0f;

    private Vector2 wetLevelRangeLin;
    private Vector2 dryLevelRangeLin;

    private TrackTool toolCube;
    private TrackFMODManager trackMng;


    //private ChurchPopulator _churchPopulator;

    // Start is called before the first frame update
    void Start()
    {
        ogScaleZ = transform.localScale.z;
        capScaleZ = ogScaleZ * 4f;

        handL = GameObject.FindGameObjectWithTag("HandL").transform;
        handR = GameObject.FindGameObjectWithTag("HandR").transform;

        //Convert db range to linear range:
        wetLevelRangeLin = new Vector2(General_Utilities.dB2lin(wetLevelRangedB.x), General_Utilities.dB2lin(wetLevelRangedB.y));
        dryLevelRangeLin = new Vector2(General_Utilities.dB2lin(dryLevelRangedB.x), General_Utilities.dB2lin(dryLevelRangedB.y));

        toolCube = GetComponent<TrackTool>();

        //_churchPopulator = GetComponent<ChurchPopulator>();

    }

    // Update is called once per frame
    void Update()
    {
        TrackOrbCopied appliedTo = toolCube.GetAppliedOrbCopy();

        if (appliedTo)
        {
            trackMng = appliedTo.og.GetComponent<TrackFMODManager>();

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
              
            }
            else
            {
                if (isLeftTriggerDown && !isRightTriggerDown)
                {
                    if (currentState != ReverbState.CHANGINGDRY)
                    {
                        dryInitialY = handL.position.y;
                        currentState = ReverbState.CHANGINGDRY;
                    }
                }
                else if (!isLeftTriggerDown && isRightTriggerDown)
                {
                    if (currentState != ReverbState.CHANGINGWET)
                    {
                        wetInitialY = handR.position.y;
                        currentState = ReverbState.CHANGINGWET;
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
                float currentY = handR.position.y;
                float expansion = currentY - wetInitialY;

                float newOverlay = churchMesh.material.GetFloat(General_ShaderID.rimOverlay) + expansion * wetChangeSensitivity;
                if (newOverlay > overlayRangeWet.y)
                {
                    newOverlay = overlayRangeWet.y;
                }
                else if (newOverlay < overlayRangeWet.x)
                {
                    newOverlay = overlayRangeWet.x;
                }

                churchMesh.material.SetFloat(General_ShaderID.rimOverlay, newOverlay);

                // Change the wet value:
                float newWetLin = General_Utilities.Map(overlayRangeWet, wetLevelRangeLin, newOverlay);
                trackMng.reverbWetLevel = General_Utilities.lin2dB(newWetLin);

                wetInitialY = currentY;

            }
            else if (currentState == ReverbState.CHANGINGDRY)
            {
                float currentY = handL.position.y;
                float expansion = currentY - dryInitialY;

                float newOverlay = appliedTo.GetOverlay() + expansion * dryChangeSensitivity;
                if (newOverlay > overlayRangeDry.y)
                {
                    newOverlay = overlayRangeDry.y;
                }
                else if (newOverlay < overlayRangeDry.x)
                {
                    newOverlay = overlayRangeDry.x;
                }

                appliedTo.SetOverlay(newOverlay);
                Debug.Log("newOverlay: " + newOverlay);

                // Change the dry value:
                float newDryLin = General_Utilities.Map(overlayRangeDry, dryLevelRangeLin, newOverlay);
                trackMng.reverbDryLevel = General_Utilities.lin2dB(newDryLin);

                dryInitialY = currentY;

            }
        }
    }

    public void ResetOverlay()
    {
        churchMesh.material.SetFloat(General_ShaderID.rimOverlay, overlayRangeWet.x);
        toolCube.GetAppliedOrbCopy().SetOverlay(overlayRangeDry.y);
    }
}
