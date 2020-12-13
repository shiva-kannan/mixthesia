using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public enum ClickableState
    {
        NORMAL,
        HOVER,
    }

    [HideInInspector] public ClickableState myState = ClickableState.NORMAL;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (myState == ClickableState.HOVER)
        {
            Hover();
        }
        else
        {
            Normal();
        }

        AdditionalUpdate();

    }

    public virtual void Hover() { }

    public virtual void Normal() { }

    public virtual void ThumbstickPush(Vector2 thumbstickVal) { }

    public virtual void OnTriggerDown() { }

    public virtual void OnButtonOneDown() { }

    public virtual void OnButtonTwoDown() { }
    
    public virtual void OnGrabbed(Vector3 grabToPosition) { }

    public virtual void AdditionalUpdate() { }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Pointer")
        {
            Pointer p = other.GetComponent<Pointer>();
            if (p.occupiedBy == null)
            {
                myState = ClickableState.HOVER;
                p.occupiedBy = this;
            }
        }

        AdditionalOnTriggerS(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Pointer")
        {
            Pointer p = other.GetComponent<Pointer>();
            if (p.occupiedBy == this && !p.isGrabDown)
            {
                p.occupiedBy = null;
                myState = ClickableState.NORMAL;
            }
        }

        AdditionalOnTriggerE(other);
    }

    public virtual void AdditionalOnTriggerS(Collider other) { }

    public virtual void AdditionalOnTriggerE(Collider other) { }
}
