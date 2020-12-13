using UnityEngine;

public class TrackFieldAxisDrawer : MonoBehaviour
{
    [SerializeField] private GameObject volumeDrawerGO;
    [SerializeField] private GameObject panDrawerGO;
    [SerializeField] private GameObject automationDrawerGO;
    [SerializeField] private bool isAutomationFloating;
    private LineRenderer _panLineRenderer;
    private LineRenderer _volumeLineRenderer;
    private LineRenderer _automationLineRenderer;

    private TrackField _trackFieldReference;
    private AudioCube _audioCubeReference;
    private TrackOrb _trackOrbReference;
    private TrackAutomation _trackAutomationReference;
    private float farCloseDifference;
    private float angleBetweenLeftRightAxis;
    

    [SerializeField] private Material lineRendererMat;
    [SerializeField] private Material automationMat;
    [SerializeField] private float autoLineOpacity;

    [SerializeField] private int numberOfPanPoints = 20;
    [SerializeField] private int numberOfVolumePoints = 20;
    [SerializeField] private int numberofAudioCubeVolumepoints = 20;
    [SerializeField] private float angleOffset = 5;
    
    // Start is called before the first frame update
    void Start()
    {
        _panLineRenderer = panDrawerGO.GetComponent<LineRenderer>();
        _volumeLineRenderer = volumeDrawerGO.GetComponent<LineRenderer>();
        _automationLineRenderer = automationDrawerGO.GetComponent<LineRenderer>();

        _trackFieldReference = GameObject.Find("TrackField").GetComponent<TrackField>();
        _audioCubeReference = GameObject.Find("TrackField").GetComponent<AudioCube>();
        _trackOrbReference = GetComponentInParent<TrackOrb>();
        _trackAutomationReference = GetComponentInParent<TrackAutomation>();
        
        //Initialize some line renderer propertities:
        _panLineRenderer.positionCount = numberOfPanPoints;
        _panLineRenderer.startWidth = 0.03f;
        _panLineRenderer.endWidth = 0.03f;
        _panLineRenderer.textureMode = LineTextureMode.RepeatPerSegment;
        _panLineRenderer.material = lineRendererMat;
        _panLineRenderer.enabled = true;
        
        _volumeLineRenderer.positionCount = numberOfVolumePoints;
        _volumeLineRenderer.startWidth = 0.03f;
        _volumeLineRenderer.endWidth = 0.03f;
        _volumeLineRenderer.textureMode = LineTextureMode.RepeatPerSegment;
        _volumeLineRenderer.material = lineRendererMat;
        _volumeLineRenderer.enabled = true;

        _automationLineRenderer.startWidth = 0.068f;
        _automationLineRenderer.endWidth = 0.068f;
        _automationLineRenderer.material = new Material(automationMat);

        farCloseDifference = (_trackFieldReference.GetReferenceFar() - _trackFieldReference.GetReferenceClose())
            .magnitude;
        angleBetweenLeftRightAxis = _trackFieldReference.AngleBetweenLeftRight();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_trackOrbReference.myState == Clickable.ClickableState.HOVER)
        {
            if (!_trackOrbReference.isInAudioCube)
            {
                if (!_panLineRenderer.enabled)
                {
                    _panLineRenderer.enabled = true;
                    _volumeLineRenderer.enabled = true;
                }
                DrawPanAxis();
                DrawVolumeAxes();
            }
        }
        else
        {
            // Find better way to not draw
            _panLineRenderer.enabled = false;
            _volumeLineRenderer.enabled = false;
        }

        if (_trackAutomationReference.isRecording || _trackAutomationReference.isThereAutomation)
        {
            _automationLineRenderer.enabled = true;
            Color c = _trackOrbReference.GetAutomationColor();
            _automationLineRenderer.material.color = new Color(c.r, c.g, c.b, autoLineOpacity);

            DrawAutomation();
        }
        else
        {
            _automationLineRenderer.enabled = false;
        }
    }

    private void DrawPanAxis()
    {
        if (!_trackOrbReference.isInAudioCube)
        {
            Vector3 centerToPosRay = _trackOrbReference.TF_GetCenterToPos().normalized;
        
            // Walk from centre along centerToPos to draw segments
            Vector3 startPos = _trackFieldReference.GetReferenceStart() + _trackFieldReference.GetDistanceRange().x * centerToPosRay;
            Vector3 endPos = startPos + farCloseDifference * centerToPosRay;
            float distance = (endPos - startPos).magnitude;
            float distanceBetweenPoints = distance / numberOfPanPoints;
            Vector3 incrementalPos = startPos;
            for (int i = 0; i < numberOfPanPoints; i++)
            {
                _panLineRenderer.SetPosition(i, incrementalPos);
                incrementalPos = incrementalPos + distanceBetweenPoints * centerToPosRay;
            }
        
            _panLineRenderer.materials[0].mainTextureScale = new Vector2(distanceBetweenPoints/2, 1);
        }
        else
        {
            // Draw for when it's inside Audio Cube
            /*Vector3 trackOrbPosition = _trackOrbReference.transform.position;
            float distanceBetweenPoints = _audioCubeReference.GetAxisLength()/ numberOfPanPoints;
            Vector3 distanceBetweenPoinstVector = new Vector3(distanceBetweenPoints, 0, 0);
            var axisRanges = _audioCubeReference.GetAxisRanges();
            float xOffSet = 0.05f;
            Vector3 startPos = new Vector3(axisRanges[0].x + xOffSet, trackOrbPosition.y, trackOrbPosition.z);
            Vector3 incrementalPos = startPos;
            for (int i = 0; i < numberOfPanPoints; i++)
            {
                _panLineRenderer.SetPosition(i, incrementalPos);
                incrementalPos = incrementalPos + distanceBetweenPoinstVector;
            }*/
        }
    }

    private void DrawVolumeAxes()
    {
        if (!_trackOrbReference.isInAudioCube)
        {
            float centerToPosMagnitude = _trackOrbReference.TF_GetCenterToPos().magnitude;

            // Parameterize from left to right
            Vector3 refStart = _trackFieldReference.GetReferenceStart();
            Vector3 leftStartPoint = refStart + _trackFieldReference.GetLeftAxis().normalized * centerToPosMagnitude;
            float angularDistanceBetweenPoints = angleBetweenLeftRightAxis / (numberOfVolumePoints - 1);

            Vector3 incrementalPos = leftStartPoint;
            for (int i = 0; i < numberOfVolumePoints; i++)
            {
                _volumeLineRenderer.SetPosition(i, incrementalPos);
                incrementalPos = refStart + Quaternion.AngleAxis(angularDistanceBetweenPoints, _trackFieldReference.GetTFieldUp()) * (incrementalPos - refStart);
            }
            _volumeLineRenderer.materials[0].mainTextureScale = new Vector2(1, 1);
        }
        else
        {
            // Draw for when it's inside Audio Cube
            /*Vector3 trackOrbPosition = _trackOrbReference.transform.position;
            float distanceBetweenPoints = _audioCubeReference.GetAxisLength()/ (numberOfVolumePoints);
            Vector3 distanceBetweenPoinstVector = new Vector3(0, 0, distanceBetweenPoints);
            var axisRanges = _audioCubeReference.GetAxisRanges();
            float zOffSet = 0.2f;
            Vector3 startPos = new Vector3(trackOrbPosition.x, trackOrbPosition.y, axisRanges[2].x);
            Vector3 incrementalPos = startPos;
            for (int i = 0; i < numberOfVolumePoints; i++)
            {
                _volumeLineRenderer.SetPosition(i, incrementalPos);
                incrementalPos = incrementalPos + distanceBetweenPoinstVector;
            }*/
        }
    }

    private void DrawAutomation()
    {
        int numOfKeys = _trackAutomationReference.posXCurve.keys.Length;

        _automationLineRenderer.positionCount = numOfKeys;
        for (int i = 0; i < numOfKeys; i++)
        {
            float newYOffset;
            if (isAutomationFloating)
            {
                newYOffset = (_trackAutomationReference.posXCurve.keys[i].time - _trackAutomationReference.GetCurrentTimeInS()) * 1.5f;
            }
            else
            {
                newYOffset = 0f;
            }

            Vector3 pos = new Vector3(_trackAutomationReference.posXCurve.keys[i].value, _trackAutomationReference.posYCurve.keys[i].value + newYOffset, _trackAutomationReference.posZCurve.keys[i].value);
            _automationLineRenderer.SetPosition(i, pos);
        }
    }

    public Color GetAutomationColor()
    {
        return _trackOrbReference.GetAutomationColor();
    }

    public void EraseAutomationPoints()
    {
        // Erase all points in the linerenderer and disable line renderer
        _automationLineRenderer.positionCount = 0;
        _automationLineRenderer.enabled = false;
    }
}
