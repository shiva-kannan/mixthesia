using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    [SerializeField] bool isInverted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInverted)
        {
            transform.LookAt(Camera.main.transform);
        }
        else
        {
            Vector3 ogOffset = Camera.main.transform.position - transform.position;
            Vector3 newOffset = ogOffset * -1;
            transform.LookAt(transform.position + newOffset);
        }
    }
}
