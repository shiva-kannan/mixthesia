using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCube : MonoBehaviour
{
    [Header("Reference Points")]
    [SerializeField] private Transform cube_origin;
    [SerializeField] private Transform cube_xend;
    [SerializeField] private Transform cube_yend;
    [SerializeField] private Transform cube_zend;

    private float axisLength = 0f;


    // Start is called before the first frame update
    void Start()
    {
        axisLength = Mathf.Abs(cube_xend.position.x - cube_origin.position.x);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetRandomStartPos()
    {
        float randomX = UnityEngine.Random.Range(-0.4f, 0.4f) * axisLength;
        //float randomZ = cube_origin.position.z + UnityEngine.Random.Range(0.2f, 0.6f) * axisLength;
        float randomZ = cube_origin.position.z;

        return new Vector3(randomX, cube_origin.position.y, randomZ);
    }

    public float GetAxisLength()
    {
        return axisLength;
    }

    public Vector2[] GetAxisRanges()
    {
        Vector2[] ret = new Vector2[3];
        Vector2 xrange = new Vector2(cube_origin.position.x, cube_origin.position.x + axisLength);
        Vector2 yrange = new Vector2(cube_origin.position.y, cube_origin.position.y + axisLength);
        Vector2 zrange = new Vector2(cube_origin.position.z, cube_origin.position.z + axisLength);

        ret[0] = xrange;
        ret[1] = yrange;
        ret[2] = zrange;

        return ret;
    }

    public Vector3 GetCubeCenter()
    {
        float x = (cube_origin.position.x + cube_xend.position.x) / 2f;
        float y = (cube_origin.position.y + cube_yend.position.y) / 2f;
        float z = (cube_origin.position.z + cube_zend.position.z) / 2f;

        return new Vector3(x, y, z);
    }

    public Vector3 GetCubeOrigin()
    {
        return cube_origin.position;
    }
}
