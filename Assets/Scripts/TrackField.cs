using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackField : MonoBehaviour
{
    [SerializeField] private Transform refStart;
    [SerializeField] private Transform refClose;
    [SerializeField] private Transform refFar;
    [SerializeField] private Transform refLeft;
    [SerializeField] private Transform refRight;

    private Vector2 angleAllowed = Vector2.zero;
    private Vector2 distanceAllowed = Vector2.zero;

    private Vector3 midAxis = Vector3.zero;
    private Vector3 rightAxis = Vector3.zero;
    private Vector3 leftAxis = Vector3.zero;
    private Plane trackFieldPlane;

    private bool isHidden = false;
    [HideInInspector] public Vector3 offsetToOrigin;

    private void Awake()
    {
        // Get three axes:
        midAxis = refFar.position - refStart.position;
        rightAxis = refRight.position - refStart.position;
        leftAxis = refLeft.position - refStart.position;
        trackFieldPlane = new Plane(refStart.position, refLeft.position, refRight.position);

        // Calculate allowed angle range:
        float angleRangeMax = General_Utilities.GetSignedAngle(midAxis, rightAxis, "X");
        float angleRangeMin = General_Utilities.GetSignedAngle(midAxis, leftAxis, "X");
        angleAllowed = new Vector2(angleRangeMin, angleRangeMax);

        //Caculate allowed distance range:
        float close = (refClose.position - refStart.position).magnitude;
        float far = midAxis.magnitude;
        distanceAllowed = new Vector2(close, far);
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (isHidden)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, 8f * Vector3.down, 3.5f * Time.deltaTime);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, 0f * Vector3.down, 7f * Time.deltaTime);
        }

        offsetToOrigin = Vector3.zero - transform.localPosition;
    }

    public void HideAndReveal(bool hide)
    {
        isHidden = hide;
    }


    public Vector3 GetMidAxis()
    {
        return midAxis;
    }

    public Vector3 GetRightAxis()
    {
        return rightAxis;
    }

    public Vector3 GetLeftAxis()
    {
        return leftAxis;
    }

    public Vector2 GetAngleRange()
    {
        return angleAllowed;
    }

    public Vector2 GetDistanceRange()
    {
        return distanceAllowed;
    }

    public Vector3 GetTFieldUp()
    {
        return refClose.up;
    }

    public Vector3 GetReferenceStart()
    {
        return refStart.position;
    }

    public Vector3 GetReferenceClose()
    {
        return refClose.position;
    }

    public Vector3 GetReferenceFar()
    {
        return refFar.position;
    }

    public float AngleBetweenLeftRight()
    {
        return angleAllowed.y - angleAllowed.x;
    }

    public Vector3 GetRandomStartPos()
    {
        float randDist = distanceAllowed.x + Random.Range(0.5f, 1f);
        float randAngle = Random.Range(angleAllowed.x * 0.65f, angleAllowed.y * 0.65f);
        Vector3 randPoint = Quaternion.AngleAxis(randAngle, refClose.up) * (midAxis.normalized * randDist) + refStart.position;

        return randPoint;
    }
}
