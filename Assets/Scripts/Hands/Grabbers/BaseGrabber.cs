using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hands;
using LearnXR.Core.Utilities;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Serialization;


// BaseFingerGrabber is responsible for initializing hand colliders, collecting collision events etc.
// Should be extended by a grabber class which handles when to grab and release etc.
// Currently 2 subclasses are implemented: KinematicGrabber and PhysicsGrabber.

// Base grabber keeps a list of all active collisions with fingers which can be used by extending classes
// such as for kinematic grabbing. Provides methods common for all grabber classes such as
// finding if any object is colliding with specific hand bones, useful to check e.g.
// if a thumb is colliding with the same object as index finger for pinch grab or to check
// if colliders of both hands are grabbing the same object for both handed grabbing. 
public class BaseGrabber : MonoBehaviour
{

    #region GRAB EVENTS

    public static event Action<Grabbable, BaseGrabber> OnGrabEnter;
    public static event Action<Grabbable, BaseGrabber> OnGrabExit;
    
    protected void GrabEnter(Grabbable go) => OnGrabEnter?.Invoke(go, this);
    protected void GrabExit(Grabbable go) => OnGrabExit?.Invoke(go, this);

    #endregion

    [SerializeField]
    [Tooltip("If Object Parenting is used (KinematicGrabber), grabbed object will be childed to this Transform." +
             " If Object Parenting is disabled, object will follow the movement of this Transform which will in turn be" + 
             " recalculated every time a collider is added to collision list for grabbed object.")]
    public GameObject handReference;
    
    [FormerlySerializedAs("rayInteractorRight")] [SerializeField] private RayInteractor rayInteractor;

    private OVRHand _ovrHand;
    
    public bool IsActive => _ovrHand != null && _ovrHand.IsTracked;


    [SerializeField]
    private HandPhysicsCapsules physicsCapsules;
    //public HandState State { get; set; } = HandState.DEFAULT;
    
    [SerializeField]
    protected OVRSkeleton skeleton;
    
    protected Grabbable GrabbedObject;
    
    public bool IsGrabbing => GrabbedObject;
    
    void AttachColliders()
    {
        int i = 0;
        foreach (BoneCapsule capsule in physicsCapsules.Capsules)
        {
            if (GrabbingFingers.FingerCollisionStartJoints.Contains(capsule.StartJoint))
            {
                GameObject capsuleRBGO = capsule.CapsuleRigidbody.gameObject;
                CapsuleCollisionController collisionGO = capsuleRBGO.AddComponent<CapsuleCollisionController>();
                collisionGO.name = "CapsuleCollision";
                collisionGO.Init(capsule.StartJoint, skeleton.GetSkeletonType());
                i++;
            }
        }
        SpatialLogger.Instance.LogInfo($"Colliders successfully attached to {i} capsules");
    }
    
    
    void Start()
    {
        physicsCapsules.WhenCapsulesGenerated += AttachColliders;
        _ovrHand = handReference.GetComponent<OVRHand>();
    }
    
    void OnDestroy()
    {
        physicsCapsules.WhenCapsulesGenerated -= AttachColliders;
    }
    
   public virtual void GrabObject(Grabbable go)
   {
       if (IsGrabbing) return;
       if (rayInteractor) rayInteractor.gameObject.SetActive(false);
       GrabbedObject = go;
       GrabEnter(go);
   }

   public virtual void ReleaseObject()
   {
       if (GrabbedObject)
       {
           GrabExit(GrabbedObject);
           GrabbedObject = null;
       }
       if (rayInteractor) rayInteractor.gameObject.SetActive(true);
   }

}
