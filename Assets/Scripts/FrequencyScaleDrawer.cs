using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEngine;

public class FrequencyScaleDrawer : MonoBehaviour
{

    private AudioCube _audioCubeReference;
    [SerializeField] private GameObject trackFieldGameObject;
    [SerializeField] private List<float> scaleValuesToShow;
    [SerializeField] private GameObject scaleTextPrefab;
    private List<GameObject> scaleTextObjects;
    private float yRange;
    private float yOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        _audioCubeReference = trackFieldGameObject.GetComponent<AudioCube>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // IEnumerator to wait for initialisation of the axislength value from audio cube
    public void GenerateAllTextScales()
    {
        yOffset = _audioCubeReference.GetAxisRanges()[1].x;
        var startPos = _audioCubeReference.GetAxisRanges();
        string valueText = "";
        yRange = _audioCubeReference.GetAxisLength();
        foreach (var value in scaleValuesToShow)
        {
            var yHeight = General_Utilities.GetMappedYForLogFrequency(value, yRange, yOffset);
            var textUI = Instantiate(scaleTextPrefab);
            var textUI_2 = Instantiate(scaleTextPrefab);
            if (value >= 1000)
            {
                valueText = $"{(int)(value / 1000)} kHz";
            }
            else
            {
                valueText = $"{value} Hz";
            }
            textUI.GetComponent<TextMeshPro>().text = valueText;
            textUI_2.GetComponent<TextMeshPro>().text = valueText;
            //Debug.Log($"For {value} - height {yHeight}");
            textUI.transform.position = new Vector3(startPos[0].x - 0.1f, yHeight, startPos[2].x);
            textUI_2.transform.position = new Vector3(startPos[0].x + yRange + 0.1f, yHeight, startPos[2].x);
        }
    }
}
