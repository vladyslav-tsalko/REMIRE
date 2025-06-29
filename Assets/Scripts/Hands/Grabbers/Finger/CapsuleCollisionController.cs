using System;
using LearnXR.Core.Utilities;
using UnityEngine;
using Oculus.Interaction.Input;
using Hands.Grabbables;


namespace Hands.Grabbers.Finger
{
    public class CapsuleCollisionController : MonoBehaviour
    {
        public HandJointId BoneId { get; private set; } = HandJointId.Invalid;
        public OVRSkeleton.SkeletonType Hand { get; private set; } = OVRSkeleton.SkeletonType.None;
        
        public void Init(HandJointId boneid, OVRSkeleton.SkeletonType hand)
        {
            BoneId = boneid;
            Hand = hand;
        }
    
        private void OnEnter(Collider other)
        {
            if (other.CompareTag("PressBlockArea"))
            {
                Destroy(other.GetComponentInParent<KinematicGrabbable>().gameObject);
            }
            
            if (!other.TryGetComponent(out KinematicGrabbable grabbable)) return;
            //SpatialLogger.Instance.LogInfo("Entered");
            grabbable.OnFingerCollisionEnter(BoneId, Hand);
        }
    
        private void OnExit(Collider other)
        {
            if (!other.TryGetComponent(out KinematicGrabbable grabbable)) return;
            //SpatialLogger.Instance.LogInfo("Exited");
            grabbable.OnFingerCollisionExit(BoneId, Hand);
        }
    
        private void OnTriggerEnter(Collider other)
        {
            //SpatialLogger.Instance.LogInfo("Trigger entered");
            OnEnter(other);
        }
    
        private void OnTriggerExit(Collider other)
        {
            //SpatialLogger.Instance.LogInfo("Trigger exited");
            OnExit(other);
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            //SpatialLogger.Instance.LogInfo("Collision entered");
            OnEnter(collision.collider);
        }
    
        private void OnCollisionExit(Collision collision)
        {
            //SpatialLogger.Instance.LogInfo("Collision exited");
            OnExit(collision.collider);
        }
    }

}
