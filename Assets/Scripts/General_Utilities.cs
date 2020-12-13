using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General_Utilities : MonoBehaviour
{
    private static float playerHeight = 1.05f; // Hand height

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static float Map(Vector2 fromRange, Vector2 toRange, float value)
    {
        float ratio = (value - fromRange.x) / (fromRange.y - fromRange.x);
        if (ratio > 1)
        {
            ratio = 1;
        }else if (ratio < 0)
        {
            ratio = 0;
        }

        float toReturn = toRange.x + (toRange.y - toRange.x) * ratio;
        return toReturn;
    }

    public static float GetSignedAngle(Vector3 from, Vector3 to, string posDirect)
    {
        float sign = 0;
        Vector3 from2to = to - from;
        switch (posDirect)
        {
            case "X":
                if (from2to.x >= 0)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
                break;
            case "Y":
                if (from2to.y >= 0)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
                break;
            case "Z":
                if (from2to.z >= 0)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
                break;
            default:
                sign = 0;
                break;
        }

        return sign * Vector3.Angle(from, to);
    }

    public static float lin2dB(float linear)
    {
        return Mathf.Log10(linear) * 20.0f;
    }

    public static float dB2lin(float dB)
    {
        return Mathf.Pow(10.0f, dB / 20.0f);
    }

    public static float milliS2Seconds(int timeInM)
    {
        return timeInM / 1000f;
    }

    public static float GetPlayerHeight()
    {
        return playerHeight;
    }

    public static void SetPlayerHeight(float h)
    {
        playerHeight = h;
    }

    public static float GetMappedYForLogFrequency(float frequency, float yCubeLength, float yOffset)
    {
        float logMinFreq = Mathf.Log10(20);
        float samplerateLogDifference = Mathf.Log10(24000) - logMinFreq;
        float logarithmicY = Mathf.Log10(frequency);
        //Debug.Log($"LogarithmicY: {logarithmicY} : LogMinFreq: {logMinFreq}, yCubeLenght: {yCubeLength}, SampleLD {samplerateLogDifference}");
        float adjustedLogY = ((logarithmicY - logMinFreq) * yCubeLength / samplerateLogDifference) + yOffset;
        //Debug.Log($"AdjustedY: {adjustedLogY}");
        return adjustedLogY;
    }
}
