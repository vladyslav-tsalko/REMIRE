using System;
using LearnXR.Core.Utilities;
using UnityEngine;
using Oculus.Interaction.Input;
using Hands.Grabbables;


namespace Hands.Grabbers.Finger
{
    public class CapsuleCollisionController : MonoBehaviour
    {
        private HandJointId BoneId { get; set; } = HandJointId.Invalid;
        private EHand Hand { get; set; } = EHand.None;
        
        public void Init(HandJointId boneId, EHand hand)
        {
            BoneId = boneId;
            Hand = hand;
        }
    
        private void OnEnter(Collider other)
        {
            if (other.CompareTag("PressBlockArea"))
            {
                Destroy(other.GetComponentInParent<KinematicGrabbable>().gameObject);
            }
            
            if (!other.TryGetComponent(out KinematicGrabbable grabbable)) return;
            grabbable.OnFingerCollisionEnter(BoneId, Hand);
        }
    
        private void OnExit(Collider other)
        {
            if (!other.TryGetComponent(out KinematicGrabbable grabbable)) return;
            grabbable.OnFingerCollisionExit(BoneId, Hand);
        }
    
        private void OnTriggerEnter(Collider other)
        {
            OnEnter(other);
        }
    
        private void OnTriggerExit(Collider other)
        {
            OnExit(other);
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            OnEnter(collision.collider);
        }
    
        private void OnCollisionExit(Collision collision)
        {
            OnExit(collision.collider);
        }
    }

}
