using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningScene : MonoBehaviour
{
    [SerializeField] Transform world;
    [SerializeField] Transform grip;
    [SerializeField] GameObject headphone;
    [SerializeField] Timeline timeline;
    [SerializeField] FrequencyScaleDrawer freqDrawer;
    
    [FMODUnity.EventRef]
    public string introSoundEffect = null;

    private bool isWorldOn = false;
    private bool isTimelineDrawn = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWorldOn)
        {
            world.position = Vector3.Lerp(world.position, Vector3.zero, 3.5f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, Vector3.down * 5f, 3.5f * Time.deltaTime);
            if (grip)
            {
                grip.position = Vector3.Lerp(grip.position, Vector3.down * 5f, 3.5f * Time.deltaTime);
            }
            Destroy(headphone);

            if (Vector3.Distance(world.position, Vector3.zero) <= 0.005f && !isTimelineDrawn)
            {
                timeline.DrawLineSegment();
                if (freqDrawer)
                {
                    freqDrawer.GenerateAllTextScales();
                }

                isTimelineDrawn = true;
            }

        }
    }

    public void OpenUp()
    {
        isWorldOn = true;
        FMODUnity.RuntimeManager.PlayOneShot(introSoundEffect);
    }
}
