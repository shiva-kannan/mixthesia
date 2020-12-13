using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizableObject : OVRGrabbable
{
    private List<OVRGrabber> mGrabbedByHands = new List<OVRGrabber>();

    private TrackFMODManager _trackFmodManager;

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        mGrabbedByHands.Add(hand);

        _trackFmodManager = GetComponent<TrackFMODManager>();
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity, OVRGrabber releasedBy = null)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = m_grabbedKinematic;
        rb.velocity = linearVelocity;
        rb.angularVelocity = angularVelocity;

        mGrabbedByHands.Remove(releasedBy);
        if (mGrabbedByHands.Count <= 0)
        {
            m_grabbedBy = null;
            m_grabbedCollider = null;
        }
    }

    private void Update()
    {
        if (mGrabbedByHands.Count > 1)
        {
            // Get contact points/grab points
            Vector3 hand1Pos = mGrabbedByHands[0].transform.position;
            Vector3 hand2Pos = mGrabbedByHands[1].transform.position;

            // Using grab points, track movement.
            float currentDistance = Vector3.Distance(hand1Pos, hand2Pos);

            // Modify this object's scale by a factor of the distance between the hands
            transform.localScale = Vector3.one * currentDistance;
        }
    }
}
