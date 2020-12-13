using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Additional : MonoBehaviour
{
    public Vector3 velocity;
    private Vector3 lastPos;

    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }
}
