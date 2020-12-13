using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_EQ_Simple : MonoBehaviour
{
    public int eqLevel = 0;
    private TrackFMODManager _trackFMODReference;
    [HideInInspector] public GameObject[] _optionReferences;
    [HideInInspector] public bool isOn;

    private Transform orbit;
    private float rot = 0f;
    [SerializeField] private float rotSpd;
    private GameObject[] visualStuff = new GameObject[0];

    [SerializeField] GameObject brightPrefab;
    [SerializeField] GameObject muddyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        _trackFMODReference = GetComponent<TrackFMODManager>();
        orbit = transform.GetChild(4);
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate the orbit always:
        rot -= rotSpd * Time.deltaTime;
        orbit.rotation = Quaternion.Euler(0, rot, 0);

        //Populate the vis:
        if (visualStuff.Length == 0)
        {
            visualStuff = new GameObject[6];
            GameObject muddy1 = Instantiate(muddyPrefab);
            GameObject muddy2 = Instantiate(muddyPrefab);
            GameObject muddy3 = Instantiate(muddyPrefab);
            GameObject bright1 = Instantiate(brightPrefab);
            GameObject bright2 = Instantiate(brightPrefab);
            GameObject bright3 = Instantiate(brightPrefab);

            muddy1.SetActive(false);
            visualStuff[0] = muddy1;
            muddy2.SetActive(false);
            visualStuff[1] = muddy2;
            muddy3.SetActive(false);
            visualStuff[2] = muddy3;

            bright1.SetActive(false);
            visualStuff[3] = bright1;
            bright2.SetActive(false);
            visualStuff[4] = bright2;
            bright3.SetActive(false);
            visualStuff[5] = bright3;
        }

        visualStuff[0].transform.position = orbit.GetChild(0).position;
        visualStuff[1].transform.position = orbit.GetChild(1).position;
        visualStuff[2].transform.position = orbit.GetChild(2).position;
        visualStuff[3].transform.position = orbit.GetChild(0).position;
        visualStuff[4].transform.position = orbit.GetChild(1).position;
        visualStuff[5].transform.position = orbit.GetChild(2).position;

        switch (eqLevel)
        {
            case 3:
                visualStuff[0].gameObject.SetActive(false);
                visualStuff[1].gameObject.SetActive(false);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(true);
                visualStuff[4].gameObject.SetActive(true);
                visualStuff[5].gameObject.SetActive(true);
                break;
            case 2:
                visualStuff[0].gameObject.SetActive(false);
                visualStuff[1].gameObject.SetActive(false);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(true);
                visualStuff[4].gameObject.SetActive(true);
                visualStuff[5].gameObject.SetActive(false);
                break;

            case 1:
                visualStuff[0].gameObject.SetActive(false);
                visualStuff[1].gameObject.SetActive(false);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(true);
                visualStuff[4].gameObject.SetActive(false);
                visualStuff[5].gameObject.SetActive(false);
                break;

            case -1:
                visualStuff[0].gameObject.SetActive(true);
                visualStuff[1].gameObject.SetActive(false);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(false);
                visualStuff[4].gameObject.SetActive(false);
                visualStuff[5].gameObject.SetActive(false);
                break;

            case -2:
                visualStuff[0].gameObject.SetActive(true);
                visualStuff[1].gameObject.SetActive(true);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(false);
                visualStuff[4].gameObject.SetActive(false);
                visualStuff[5].gameObject.SetActive(false);
                break;

            case -3:
                visualStuff[0].gameObject.SetActive(true);
                visualStuff[1].gameObject.SetActive(true);
                visualStuff[2].gameObject.SetActive(true);
                visualStuff[3].gameObject.SetActive(false);
                visualStuff[4].gameObject.SetActive(false);
                visualStuff[5].gameObject.SetActive(false);
                break;

            default:
                visualStuff[0].gameObject.SetActive(false);
                visualStuff[1].gameObject.SetActive(false);
                visualStuff[2].gameObject.SetActive(false);
                visualStuff[3].gameObject.SetActive(false);
                visualStuff[4].gameObject.SetActive(false);
                visualStuff[5].gameObject.SetActive(false);
                break;

        }
        

        //Update option avaliabilities
        if (_optionReferences.Length == 2)
        {
            if (isOn)
            {
                if (IsMuddiest())
                {
                    _optionReferences[0].SetActive(false);
                }
                else
                {
                    if (!_optionReferences[0].activeSelf)
                    {
                        _optionReferences[0].transform.position = transform.position;
                        _optionReferences[0].SetActive(true);
                    }
                }

                if (IsBrightest())
                {
                    _optionReferences[1].SetActive(false);
                }
                else
                {
                    if (!_optionReferences[1].activeSelf)
                    {
                        _optionReferences[1].transform.position = transform.position;
                        _optionReferences[1].SetActive(true);
                    }
                }
            }
            else
            {
                _optionReferences[0].SetActive(false);
                _optionReferences[1].SetActive(false);

            }
        }
    }

    public bool IsBrightest()
    {
        return eqLevel == 3;
    }

    public bool IsMuddiest()
    {
        return eqLevel == -3;
    }

    public void MakeBrighter()
    {
        _trackFMODReference.MakeEQBrighter();
        eqLevel += 1;
    }

    public void MakeMuddier()
    {
        _trackFMODReference.MakeEQMuddier();
        eqLevel -= 1;
    }

    public void Reset()
    {
        _trackFMODReference.ResetEqGainParameters();
        eqLevel = 0;
    }
}
