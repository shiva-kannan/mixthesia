using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TrackTool : Clickable
{
    [HideInInspector] public bool isGrabbed = false;
    [SerializeField] private bool isGrabbable;

    [Header("----------Reverb----------")]
    [SerializeField] GameObject churchPrefab;

    [Header("----------EQ----------")]
    [SerializeField] bool isBright;
    [HideInInspector] public Tool_EQ_Simple _trackSimpEQReference = null;

    private TrackOrbCopied appliedTo = null;
    private Vector3 ogSize;

    private GameObject editInstance;

    [Header("DSP Tool Properties")] 
    public DSP_TYPE dspType;
    public REVERBPRESETS reverbpreset;
    

    // Start is called before the first frame update
    void Start()
    {
        ogSize = transform.localScale;
    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        if (!isGrabbed)
        {
            if (dspType == DSP_TYPE.THREE_EQ && _trackSimpEQReference)
            {
                Vector3 offset;
                if (isBright)
                {
                    offset = 0.75f * Vector3.up;
                }
                else
                {
                    offset = 0.75f * Vector3.down;
                }

                Vector3 targetPos = _trackSimpEQReference.transform.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPos, 7f * Time.deltaTime);
            }
            else
            {
                if (appliedTo)
                {
                    transform.position = appliedTo.transform.position;
                }
            }
        }
    }

    public override void Hover()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale, 1.25f * ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale;
    }

    public override void Normal()
    {
        Vector3 newScale = Vector3.Lerp(transform.localScale, ogSize, 7f * Time.deltaTime);
        transform.localScale = newScale;
    }

    public override void OnGrabbed(Vector3 grabToPosition)
    {
        if (isGrabbable)
        {
            isGrabbed = true;
            transform.position = Vector3.Lerp(transform.position, grabToPosition, 7f * Time.deltaTime);
        }
    }

    public override void OnTriggerDown()
    {
        base.OnTriggerDown();
        if (dspType == DSP_TYPE.THREE_EQ && _trackSimpEQReference)
        {
            if (isBright && !_trackSimpEQReference.IsBrightest())
            {
                _trackSimpEQReference.MakeBrighter();
            }else if (!isBright && !_trackSimpEQReference.IsMuddiest())
            {
                _trackSimpEQReference.MakeMuddier();
            }
        }
    }

    public override void AdditionalOnTriggerS(Collider other)
    {
        if (!appliedTo)
        {
            if (other.CompareTag("OrbCopy") && !isGrabbed)
            {
                appliedTo = other.GetComponent<TrackOrbCopied>();

                if (appliedTo.numberOfEffects == 0)
                {
                    // Call the function to actually apply the associated DSP
                    ApplyAssociatedDSP();
                    appliedTo.numberOfEffects += 1;
                }
                else
                {
                    appliedTo = null;
                }
            }
        }
    }

    public override void AdditionalOnTriggerE(Collider other)
    {
        if (other.CompareTag("OrbCopy") && other.GetComponent<TrackOrbCopied>() == appliedTo && isGrabbed)
        {
            // Remove the DSP before making the applied to null
            RemoveAssociatedDSP();

            Tool_Reverb_Opacity reverbV2 = GetComponent<Tool_Reverb_Opacity>();
            if (reverbV2)
            {
                reverbV2.ResetOverlay();
            }

            if (editInstance)
            {
                Destroy(editInstance);
            }

            appliedTo.numberOfEffects -= 1;
            appliedTo = null;
        }
    }

    private void ApplyAssociatedDSP()
    {
        switch (dspType)
        {
            case DSP_TYPE.SFXREVERB:
                appliedTo.og.GetComponent<TrackFMODManager>().AddReverbDSP();

                if (churchPrefab)
                {
                    //Instantiate tool edit:
                    editInstance = Instantiate(churchPrefab);
                    editInstance.transform.position = new Vector3(0, -80f, 0);

                    Tool_Reverb churchScript = editInstance.GetComponent<Tool_Reverb>();
                    Transform display = GameObject.FindGameObjectWithTag("CopyDisplay").transform;
                    churchScript.copyDisplay = display;
                    churchScript.ogCopyZ = display.position.z;
                    churchScript.trackMng = appliedTo.og.GetComponent<TrackFMODManager>();
                }

                return;
            default:
                return;
        }
    }

    private void RemoveAssociatedDSP()
    {
        switch (dspType)
        {
            case DSP_TYPE.SFXREVERB:
                int returnValue = appliedTo.og.GetComponent<TrackFMODManager>().RemoveReverbDSP();
                if (returnValue == 0)
                {
                    Debug.Log("Reverb removal unsuccessful");
                }

                return;
            default:
                return;
        }
    }

    public TrackOrbCopied GetAppliedOrbCopy()
    {
        return appliedTo;
    }
}
