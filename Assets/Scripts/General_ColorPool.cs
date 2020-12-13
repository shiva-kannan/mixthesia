using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General_ColorPool : MonoBehaviour
{
    [SerializeField] Color[] allColors;
    [SerializeField] bool[] isPicked;
    [SerializeField] Color onColor;
    [SerializeField] Color offColor;
    [SerializeField] Color loudColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color GetRandonColor()
    {
        int colorIndex = UnityEngine.Random.Range((int)0, (int)allColors.Length);
        return allColors[colorIndex];
    }

    public Color GetUniqueRandonColor()
    {
        Color cToReturn = Color.white;

        if (Array.IndexOf(isPicked, false) < 0)
        {
            isPicked = new bool[allColors.Length];
            for (int i = 0; i < isPicked.Length; i++)
            {
                isPicked[i] = false;
            }
        }

        bool isColorFound = false;
        while (!isColorFound)
        {
            int colorIndex = UnityEngine.Random.Range((int)0, (int)allColors.Length);
            if (!isPicked[colorIndex])
            {
                cToReturn = allColors[colorIndex];
                isPicked[colorIndex] = true;
                isColorFound = true;
            }

        }

        return cToReturn;
    }

    public Color GetOnColor()
    {
        return onColor;
    }

    public Color GetOffColor()
    {
        return offColor;
    }

    public Color GetloudColor()
    {
        return loudColor;
    }
}
