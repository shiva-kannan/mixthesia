using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPopulator : MonoBehaviour
{
    [SerializeField] GameObject starPrefab;

    private List<GameObject> stars = new List<GameObject>();
    private LineRenderer myLR;

    // Start is called before the first frame update
    void Start()
    {
        myLR = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (myLR.enabled)
        {
            int numOfPoints = myLR.positionCount;
            for (int i = 0; i < numOfPoints; i++)
            {
                if (i >= stars.Count)
                {
                    GameObject newStar = Instantiate(starPrefab);
                    newStar.transform.position = myLR.GetPosition(i);

                    float scale = Random.Range(0.7f, 1.4f);
                    newStar.transform.localScale = Vector3.one * scale;


                    Color c = GetComponentInParent<TrackFieldAxisDrawer>().GetAutomationColor();
                    newStar.GetComponent<SpriteRenderer>().color = c;


                    newStar.SetActive(true);
                    stars.Add(newStar);
                }
                else
                {
                    stars[i].transform.position = myLR.GetPosition(i);
                    stars[i].SetActive(true);
                }
            }

        }
        else
        {
            foreach (GameObject star in stars)
            {
                star.SetActive(false);
            }
        }

    }
}
