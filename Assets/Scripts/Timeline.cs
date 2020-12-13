using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timeline : MonoBehaviour
{
    public bool isDrawingHorizontal;

    [Header("----------Interaction Related----------")]
    [SerializeField] Pointer[] pointers;
    [SerializeField] float interactZone;
    [SerializeField] GameObject playHeadGhost;

    [Header("----------Line Related----------")]
    [SerializeField] float startDistance;
    [SerializeField] float lineDistanceOffset;
    [SerializeField] int lineResolution;
    [SerializeField] float lineWidth;
    [SerializeField] Material lineMaterial;
    [SerializeField] Material playedMaterial;
    [SerializeField] TimelineController tlController;
    private float horizontalLength;
    [SerializeField] Vector3 totalTextOffset;

    private TrackField tField;
    private AudioCube aCube;
    private LineRenderer entireLine;
    private LineRenderer playedLine;
    private Transform playHead;
    private TextMeshPro totalTimeText;

    private float totalTime = 0f;
    private float currentTime = 0f;

    //Interaction related private varaiables:
    private Vector3 leftHandPoint;
    private Vector3 rightHandPoint;
    private bool leftTrigger;
    private bool rightTrigger;

    private bool isPointedAt = false;
    private Vector3 pointingHand = Vector3.zero;
    private bool pointingTrigger = false;
    private Vector3 pointedPos = Vector3.zero;

    private GameObject ghost;

    // Start is called before the first frame update
    void Start()
    {
        tField = transform.parent.GetComponent<TrackField>();
        aCube = transform.parent.GetComponent<AudioCube>();

        entireLine = transform.GetChild(0).GetComponent<LineRenderer>();
        playedLine = transform.GetChild(1).GetComponent<LineRenderer>();
        playHead = transform.GetChild(2);
        totalTimeText = transform.GetChild(3).GetComponent<TextMeshPro>();

        ghost = Instantiate(playHeadGhost);
        ghost.SetActive(false);

        //Initialize some line properties:
        entireLine.positionCount = lineResolution;
        entireLine.startWidth = lineWidth;
        entireLine.endWidth = lineWidth;
        entireLine.material = new Material(lineMaterial);

        playedLine.positionCount = 0;
        playedLine.startWidth = lineWidth;
        playedLine.endWidth = lineWidth;
        playedLine.material = new Material(playedMaterial);

        //Position the timeline:
        if (isDrawingHorizontal)
        {
            transform.position = new Vector3(aCube.GetCubeOrigin().x, aCube.GetCubeOrigin().y, startDistance);
            horizontalLength = aCube.GetAxisLength();
        }
        else
        {
            transform.position = tField.GetReferenceStart() + tField.GetLeftAxis().normalized * startDistance;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        totalTime = tlController.totalTimeInMS;
        currentTime = tlController.currentTimeInMS;
        totalTimeText.text = TimeInString((int)totalTime);

        float currentRatio = currentTime / totalTime;

        //Update playhead position:
        if (isDrawingHorizontal)
        {
            float playHeadXPos = transform.position.x + (currentRatio * horizontalLength);

            Vector3 playHeadTargetPos = new Vector3(playHeadXPos, transform.position.y, transform.position.z);
            playHead.position = playHeadTargetPos;
        }
        else
        {
            float playHeadAngle = tField.AngleBetweenLeftRight() * currentRatio + tField.GetAngleRange().x;

            Vector3 playHeadTargetPos = tField.GetReferenceStart() + Quaternion.AngleAxis(playHeadAngle, tField.GetTFieldUp()) * tField.GetMidAxis().normalized * startDistance;
            playHead.position = playHeadTargetPos;
        }

        //Update playhead time:
        playHead.GetChild(0).GetComponent<TextMeshPro>().text = TimeInString((int)currentTime);

        //Update playedline:
        int playedLineLength = Mathf.FloorToInt(lineResolution * currentRatio);
        if (playedLineLength < 0)
        {
            playedLineLength = 0;
        }
        else if (playedLineLength > lineResolution)
        {
            playedLineLength = lineResolution;
        }

        playedLine.positionCount = playedLineLength;
        for (int i = 0; i < playedLineLength; i++)
        {
            playedLine.SetPosition(i, entireLine.GetPosition(i));
        }

        //Interact with the timeline:
        leftHandPoint = pointers[0].layoutPlaneHitPoint;
        leftTrigger = pointers[0].isTriggerDown && !pointers[0].occupiedBy;
        rightHandPoint = pointers[1].layoutPlaneHitPoint;
        rightTrigger = pointers[1].isTriggerDown && !pointers[1].occupiedBy;

        if (isDrawingHorizontal)
        {
            // Check if timeline is pointed at:
            if (pointers[1].isOnTimeline && !pointers[1].occupiedBy)
            {
                isPointedAt = true;
                pointingHand = rightHandPoint;
                pointingTrigger = rightTrigger;
            }else if (pointers[0].isOnTimeline && !pointers[0].occupiedBy)
            {
                isPointedAt = true;
                pointingHand = leftHandPoint;
                pointingTrigger = leftTrigger;
            }
            else
            {
                isPointedAt = false;
                pointingHand = Vector3.zero;
                pointingTrigger = false;
            }
        }
        else
        {
            //Check if timeline is pointed at:
            float rightDist = (rightHandPoint - tField.GetReferenceStart()).magnitude;
            float leftDist = (leftHandPoint - tField.GetReferenceStart()).magnitude;

            if (Mathf.Abs(rightDist - startDistance) <= interactZone && !pointers[1].occupiedBy)
            {
                isPointedAt = true;
                pointingHand = rightHandPoint;
                pointingTrigger = rightTrigger;
            }
            else if (Mathf.Abs(leftDist - startDistance) <= interactZone && !pointers[0].occupiedBy)
            {
                isPointedAt = true;
                pointingHand = leftHandPoint;
                pointingTrigger = leftTrigger;
            }
            else
            {
                isPointedAt = false;
                pointingHand = Vector3.zero;
                pointingTrigger = false;
            }
        }

        if (!isPointedAt)
        {
            ghost.SetActive(false);
        }
        else
        {
            if (isDrawingHorizontal)
            {
                float newRatio = (pointingHand.x - aCube.GetCubeOrigin().x) / aCube.GetAxisLength();
                if (newRatio < 0)
                {
                    newRatio = 0;
                }
                else if (newRatio > 1)
                {
                    newRatio = 1;
                }

                int pointedTimeMS = Mathf.FloorToInt(totalTime * newRatio);
                ghost.transform.GetChild(0).GetComponent<TextMeshPro>().text = TimeInString(pointedTimeMS);

                //Place the translucent playhead:
                ghost.transform.position = transform.position + Vector3.right * newRatio * aCube.GetAxisLength() + Vector3.back * 0.01f;
                ghost.SetActive(true);

                //Jump the song to the pointed pos when trigger pressed:
                if (pointingTrigger)
                {
                    tlController.SetTimelinePositionForAll(pointedTimeMS);
                }
            }
            else
            {
                pointedPos = tField.GetReferenceStart() + (pointingHand - tField.GetReferenceStart()).normalized * startDistance;
                pointedPos = ClipPositionInRange(pointedPos);

                //Place the translucent playhead:
                ghost.transform.position = pointedPos;
                ghost.SetActive(true);

                float pointedPosAngle = Vector3.Angle(tField.GetLeftAxis(), pointedPos - tField.GetReferenceStart());
                float newRatio = pointedPosAngle / tField.AngleBetweenLeftRight();
                if (newRatio < 0)
                {
                    newRatio = 0;
                }
                else if (newRatio > 1)
                {
                    newRatio = 1;
                }

                int pointedTimeMS = Mathf.FloorToInt(totalTime * newRatio);
                ghost.transform.GetChild(0).GetComponent<TextMeshPro>().text = TimeInString(pointedTimeMS);

                //Jump the song to the pointed pos when trigger pressed:
                if (pointingTrigger)
                {
                    tlController.SetTimelinePositionForAll(pointedTimeMS);
                }
            }
        }


    }

    public void DrawLineSegment()
    {
        if (isDrawingHorizontal)
        {
            float xDistancePerPoint = horizontalLength / (lineResolution - 1);

            Vector3 currentPos = transform.position;
            for (int i = 0; i < lineResolution; i++)
            {
                entireLine.SetPosition(i, currentPos);
                currentPos += Vector3.right * xDistancePerPoint;
            }

            //Turn on collision box here:
            GetComponent<BoxCollider>().enabled = true;
            this.gameObject.tag = "GrabPlane";
        }
        else
        {
            Vector3 refStart = tField.GetReferenceStart();
            float angleDistancePerPoint = tField.AngleBetweenLeftRight() / (lineResolution - 1);
            //Debug.Log("Total Angle: " + tField.AngleBetweenLeftRight());
            //Debug.Log("Angle segment: " + angleDistancePerPoint);

            Vector3 currentPos = transform.position + tField.GetLeftAxis().normalized * lineDistanceOffset;
            for (int i = 0; i < lineResolution; i++)
            {
                entireLine.SetPosition(i, currentPos);
                currentPos = refStart + Quaternion.AngleAxis(angleDistancePerPoint, tField.GetTFieldUp()) * (currentPos - refStart);
            }

            //Turn off collision box:
            GetComponent<BoxCollider>().enabled = false;
            this.gameObject.tag = "Untagged";
        }

        totalTimeText.transform.position = entireLine.GetPosition(lineResolution - 1) + totalTextOffset;
        totalTimeText.gameObject.SetActive(true);
    }

    private Vector3 ClipPositionInRange(Vector3 vectTotest)
    {
        Vector2 allowedAngle = tField.GetAngleRange();
        Vector3 refPoint = tField.GetReferenceStart();

        Vector3 refToTarget = vectTotest - refPoint;
        float distance = refToTarget.magnitude;
        float angle = General_Utilities.GetSignedAngle(tField.GetMidAxis(), refToTarget, "X");

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

        return vectTotest;
    }

    private string TimeInString(int timeMS)
    {
        int currentTimeInS = Mathf.FloorToInt(timeMS / 1000f);
        int currentMinute = Mathf.FloorToInt(currentTimeInS / 60f);
        int currentSecond = currentTimeInS - (currentMinute * 60);

        if (currentMinute >= 60)
        {
            currentMinute -= 60;
        }

        if (currentSecond >= 60)
        {
            currentSecond -= 60;
        }

        string minute = "";
        if (currentMinute < 10)
        {
            minute = "0" + currentMinute;
        }
        else
        {
            minute = currentMinute.ToString();
        }

        string second = "";
        if (currentSecond < 10)
        {
            second = "0" + currentSecond;
        }
        else
        {
            second = currentSecond.ToString();
        }

        return minute + ":" + second;
    }
}
