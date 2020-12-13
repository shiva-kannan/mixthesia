using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_EQ_Threshold : Clickable
{
    [SerializeField] private Vector2 freqRangeLow;
    [SerializeField] private Vector2 freqRangeHigh;

    [HideInInspector] public TrackFMODManager _trackFMODReference;
    [HideInInspector] public TrackOrb _trackOrbReference;
    [HideInInspector] public bool isMidHigh = false;

    private Vector2 freqRange = Vector2.zero;
    private float currentThreshold = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        base.AdditionalUpdate();

        //Position the threshold:
        if (isMidHigh)
        {
            freqRange = freqRangeHigh;
        }
        else
        {
            freqRange = freqRangeLow;
        }

        if (_trackFMODReference && _trackOrbReference)
        {
            if (isMidHigh)
            {
                currentThreshold = _trackFMODReference.eqHighCrossover;
            }
            else
            {
                currentThreshold = _trackFMODReference.eqLowCrossover;
            }

            Vector2[] axisRanges = _trackOrbReference.AC_GetAxisRanges();
            Vector2 yRange = axisRanges[1];
            //float newY = General_Utilities.Map(new Vector2(20f, 20000f), yRange, currentThreshold);
            float newY =
                General_Utilities.GetMappedYForLogFrequency(currentThreshold, Mathf.Abs(yRange.x - yRange.y), yRange.x);

            Vector3 targetPos = new Vector3(_trackOrbReference.transform.position.x, newY, _trackOrbReference.transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, 7f * Time.deltaTime);

        }
    }

    public override void OnTriggerDown()
    {
        base.OnTriggerDown();
        _trackOrbReference.OnTriggerDown();
    }

}
