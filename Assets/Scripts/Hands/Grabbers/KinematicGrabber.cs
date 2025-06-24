using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hands;
using LearnXR.Core.Utilities;
using UnityEngine;


// Grabbing relies on finger colliders from OVRCustomSkeleton to detect finger collisions with the object
// via BoneCollisionController attached to relevant bone colliders specified in BaseGrabber,
// proximal and distal finger bones. All collisions registered in BaseGrabber are analyzed to check
// if all grabbing conditions are satisfied. Once an object is grabbed, it is set to kinematic and is parented
// to the hand to follow hand position and rotation in world space, similarly to ParentHeldObject in OVRGRabber (for controllers only).

// Since setting the object to kinematic disables its physics properties collisions with hands
// are no longer detected and another method for release had to be implemented. At grab-begin,
// the distance between the grabbing thumb and closest opposite finger is saved as the grab offset.
// While the hand is grabbing, the distance is calculated in every frame and the release action is triggered
// if the distance becomes greater than the initial distance plus specified threshold. 

public class KinematicGrabber : BaseGrabber
{
    [SerializeField]
    [Tooltip("GameObject childed to Right/Left HandAnchor. Position will be updated in FixedUpdate()" +
             " to represent the midpoint between thumb and closest grabbing opposite finger" +
             " to detect when grip release threshold is exceeded.")]
    private GameObject grabMidpoint;


    private void Update()
    {
        //UpdateGrabMidpoint();
    }


    public override void GrabObject(Grabbable go)
    {
        base.GrabObject(go); ;
        GrabbedObject.KinematicGrab(handReference.transform);
    }
    
    public override void ReleaseObject()
    {
        if (GrabbedObject)
        {
            GrabbedObject.KinematicRelease();
        }
        base.ReleaseObject();
    }

    public void DestroyGrabbedObject()
    {
        Grabbable temp = GrabbedObject;
        ReleaseObject();
        Destroy(temp.gameObject);
    }

    
    public float ComputeDistanceBetweenFingerAndPoint(short fingerId, Vector3 worldPos)
    {
        if (worldPos.Equals(Vector3.zero) || fingerId >= skeleton.GetCurrentNumSkinnableBones() || fingerId == (short) OVRSkeleton.BoneId.Invalid)
        {
            Debug.LogWarning("Error trying to compute distance. Rigidbody was null or incorrect finger bone id.");
            return Mathf.Infinity;
        }

        Vector3 handPos = skeleton.Bones[fingerId].Transform.position;
        float distance = Vector3.Distance(handPos, worldPos);
        return distance;
    }

    public Vector3 GetWorldPos()
    {
        return skeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_Wrist].Transform.position;
    }

    public Vector3 GetPalmPosition()
    {
        return skeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_Palm].Transform.position;
    }
}
