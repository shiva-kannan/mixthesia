using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrackOrbCopied : Clickable
{
    [SerializeField] Material orbMat;
    [SerializeField] float pushBackThreshold;

    [HideInInspector] public bool isFixedOnDisplay;
    [HideInInspector] public int numberOfEffects = 0;

    private MeshRenderer myMR;
    private Transform centerDisplay;
    [HideInInspector] public Transform og;
    private TrackOrb ogScript;

    enum CopiesState
    {
        NORMAL,
        BACK
    }

    private CopiesState myCState = CopiesState.NORMAL;

    // Start is called before the first frame update
    void Start()
    {
        centerDisplay = GameObject.FindGameObjectWithTag("CopyDisplay").transform;
    }

    // Update is called once per frame
    public override void AdditionalUpdate()
    {
        if (myCState == CopiesState.NORMAL)
        {
            transform.position = Vector3.Lerp(transform.position, centerDisplay.position, 7f * Time.deltaTime);
        }else if (myCState == CopiesState.BACK)
        {
            transform.position = Vector3.Lerp(transform.position, og.position + ogScript.TF_GetOffsetToOrigin(), 7f * Time.deltaTime);

            if (Vector3.Distance(transform.position, og.position) <= 0.01f)
            {
                Destroy(this.gameObject);
            }
        }
        
        // Temporary visual respons
        if (og != null)
        {
            transform.localScale = og.localScale * 0.55f;
        }
    }

    public void SetSizeAndPosition(Vector3 size, Vector3 pos)
    {
        transform.localScale = size;
        transform.position = pos;
    }

    public void SetIconAndName(Sprite icon, string name)
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = icon;
        transform.GetChild(1).GetComponent<TextMeshPro>().text = name;
    }

    public void SetMainColorAndScrollSpd(Color c, float scrollSpd)
    {
        if (myMR == null || (myMR != null && myMR.material == null))
        {
            myMR = GetComponent<MeshRenderer>();
            myMR.material = new Material(orbMat);
        }

        myMR.material.SetColor(General_ShaderID.rimMainColor, c);
        myMR.material.SetFloat(General_ShaderID.rimScrollSpd, scrollSpd);

    }

    public void SetOrigin(Transform t)
    {
        og = t;
        ogScript = og.GetComponent<TrackOrb>();
    }

    public void PushBack()
    {
        myCState = CopiesState.BACK;
        ogScript.BringBackTF();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "GrabVolumeBig")
        {
            GameObject hand = other.transform.parent.parent.gameObject;
            float velo = hand.GetComponent<Hand_Additional>().velocity.magnitude;

            if (velo >= pushBackThreshold && !isFixedOnDisplay)
            {
                PushBack();
            }
        }
    }

    public Color GetMainColor()
    {
        if (myMR && myMR.material)
        {
            return myMR.material.GetColor(General_ShaderID.rimMainColor);
        }
        else
        {
            return Color.white;
        }
    }

    public void SetMainColor(Color c)
    {
        if (myMR == null || (myMR != null && myMR.material == null))
        {
            myMR = GetComponent<MeshRenderer>();
            myMR.material = new Material(orbMat);
        }

        myMR.material.SetColor(General_ShaderID.rimMainColor, c);
    }

    public float GetOverlay()
    {
        if (myMR && myMR.material)
        {
            return myMR.material.GetFloat(General_ShaderID.rimOverlay);
        }
        else
        {
            return 0.0f;
        }
    }

    public void SetOverlay(float o)
    {
        if (myMR == null || (myMR != null && myMR.material == null))
        {
            myMR = GetComponent<MeshRenderer>();
            myMR.material = new Material(orbMat);
        }

        myMR.material.SetFloat(General_ShaderID.rimOverlay, o);
    }
}
