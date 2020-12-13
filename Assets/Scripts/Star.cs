using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] Vector2 freqRange;
    [SerializeField] Vector2 ampRange;

    private float freq;
    private float amp;
    private Vector3 ogSize;

    // Start is called before the first frame update
    void Start()
    {
        freq = Random.Range(freqRange.x, freqRange.y);
        amp = Random.Range(ampRange.x, ampRange.y);

        ogSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

        float sizeOffset = amp * Mathf.Sin(freq * Time.time);
        transform.localScale = ogSize + Vector3.one * sizeOffset;
    }
}
