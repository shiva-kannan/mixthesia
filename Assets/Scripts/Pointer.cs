using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField]
    private OVRInput.Controller m_controller = OVRInput.Controller.None;

    [Header("Controller Sensitivity Related")]
    [SerializeField] float triggerThreshold;
    [SerializeField] float grabThreshold;
    [SerializeField] float thumstickThreshold;
    [SerializeField] float hoveredHapticFrequency = 0.2f;
    [SerializeField] float hoveredHapticAmplitude = 0.2f;
    [SerializeField] float hoveredHapticDuration = 0.2f;

    public Clickable occupiedBy = null;

    [HideInInspector] public bool isTriggerDown = false; //Timeline needs to access this
    [HideInInspector] public bool isGrabDown = false; //Clickable needs to access this

    [HideInInspector] public Vector3 layoutPlaneHitPoint = Vector3.zero;
    [HideInInspector] public bool isOnTimeline = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (occupiedBy)
        {
            if (!occupiedBy.gameObject.activeSelf)
            {
                occupiedBy = null;
            }
        }

        float triggerVal = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        float grabValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
        Vector2 thumbStickVal = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_controller);

        //Trigger
        //Debug.Log(m_controller + ": " + triggerVal);
        if (triggerVal >= triggerThreshold && !isTriggerDown)
        {
            isTriggerDown = true;
            if (occupiedBy != null && grabValue < grabThreshold)
            {
                occupiedBy.OnTriggerDown();
            }
        }
        else if (triggerVal < triggerThreshold && isTriggerDown)
        {
            isTriggerDown = false;
        }

        //Grip
        if (grabValue >= grabThreshold && !isGrabDown)
        {
            isGrabDown = true;
            if (occupiedBy)
            {
                // Call the Grabbed method through the clickable interface
                if (occupiedBy.CompareTag("Orb"))
                {
                    occupiedBy.OnGrabbed(layoutPlaneHitPoint);

                    if (isTriggerDown)
                    {
                        occupiedBy.GetComponent<TrackAutomation>().isRecording = true;
                    }
                    else
                    {
                        occupiedBy.GetComponent<TrackAutomation>().isRecording = false;
                    }
                }
                else if (occupiedBy.CompareTag("Tool"))
                {
                    occupiedBy.OnGrabbed(transform.parent.position);
                }
            }
        }
        else if (grabValue >= grabThreshold && isGrabDown)
        {
            if (occupiedBy)
            {
                // Call the Grabbed method through the clickable interface
                if (occupiedBy.CompareTag("Orb"))
                {
                    occupiedBy.OnGrabbed(layoutPlaneHitPoint);

                    if (isTriggerDown)
                    {
                        occupiedBy.GetComponent<TrackAutomation>().isRecording = true;
                    }
                    else
                    {
                        occupiedBy.GetComponent<TrackAutomation>().isRecording = false;
                    }
                }
                else if (occupiedBy.CompareTag("Tool"))
                {
                    occupiedBy.OnGrabbed(transform.parent.position);
                }
            }
        }
        else if (grabValue < grabThreshold && isGrabDown)
        {
            isGrabDown = false;
            if (occupiedBy)
            {
                if (occupiedBy.CompareTag("Orb"))
                {
                    occupiedBy.GetComponent<TrackAutomation>().isRecording = false;
                }
                else if (occupiedBy.CompareTag("Tool"))
                {
                    occupiedBy.GetComponent<TrackTool>().isGrabbed = false;
                }

                occupiedBy.myState = Clickable.ClickableState.NORMAL;
                occupiedBy = null;
            }
        }

        //Buttons
        if (OVRInput.GetDown(OVRInput.Button.Two, m_controller))
        {
            if (occupiedBy && occupiedBy.CompareTag("Orb"))
            {
                occupiedBy.OnButtonTwoDown();
            }
        }
        
        if (OVRInput.GetDown(OVRInput.Button.One, m_controller))
        {
            if (occupiedBy && occupiedBy.CompareTag("Orb"))
            {
                occupiedBy.OnButtonOneDown();
            }
        }

        //Thumbstick
        if (thumbStickVal.magnitude >= thumstickThreshold)
        {
            if (occupiedBy)
            {
                occupiedBy.ThumbstickPush(thumbStickVal);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // As long as it's a clickable, trigger a small haptic for selection feeling
        if (other.gameObject.GetComponent<Clickable>())
        {
            OVRInput.SetControllerVibration(hoveredHapticFrequency, hoveredHapticAmplitude, m_controller);
            StartCoroutine(EndControllerVibration(hoveredHapticDuration));   
        }
    }
    
    private IEnumerator EndControllerVibration(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        OVRInput.SetControllerVibration(0f, 0f, m_controller);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("GrabPlane"))
        {
            //Debug.Log("Grab plane intersected");
            // Ray cast from pointer to plane to get intersection point
            RaycastHit hit;
            Ray pointerRay = new Ray(transform.parent.position, transform.parent.forward);
            if (Physics.Raycast(pointerRay, out hit, 50f, 1 << LayerMask.NameToLayer("TrackField")))
            {
                layoutPlaneHitPoint = hit.point;
            }

            if (other.gameObject.name == "Timeline Head")
            {
                isOnTimeline = true;
            }
            else
            {
                isOnTimeline = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GrabPlane"))
        {
            isOnTimeline = false;
        }
    }
}
