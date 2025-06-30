using System;
using UnityEngine;
using Hands.Grabbables;
using Hands.Grabbers.Finger;
using LearnXR.Core.Utilities;
using Oculus.Interaction.Input;
using Oculus.Interaction;

namespace Hands.Grabbers
{
    
    /// <summary>
    /// Attached to GameObjects representing synthetic left and right hands,
    /// enabling interaction with Grabbable objects.
    /// </summary>
    public class KinematicGrabber: MonoBehaviour
    {
        #region Events
        
        public static event Action<KinematicGrabbable, KinematicGrabber> OnGrabEnter;
        public static event Action<KinematicGrabbable, KinematicGrabber> OnGrabExit;
        
        #endregion

        #region Checkers

        public bool IsActive => _ovrHand != null && _ovrHand.IsTracked;
        public bool IsGrabbing => _grabbedObject;

        #endregion
        
        [Tooltip("Assign a hand tracking GameObject that contains both OVRHand and OVRSkeleton components.")]
        [SerializeField] private GameObject handReference;
        
        [SerializeField] private RayInteractor rayInteractor;
        
        [SerializeField] private HandPhysicsCapsules physicsCapsules;
        
        private OVRHand _ovrHand;
        private OVRSkeleton _skeleton;
        private KinematicGrabbable _grabbedObject;
        
        public void GrabObject(KinematicGrabbable go)
        {
            if (IsGrabbing) return;
            ToggleRayInteraction(false);
            _grabbedObject = go;
            _grabbedObject.KinematicGrab(handReference.transform);
            OnGrabEnter?.Invoke(go, this);
        }
        
        public void ReleaseObject()
        {
            if (_grabbedObject)
            {
                _grabbedObject.KinematicRelease();
                _grabbedObject = null;
                OnGrabExit?.Invoke(_grabbedObject, this);
            }

            ToggleRayInteraction(true);
        }
        
        public float ComputeDistanceBetweenFingerAndPoint(short fingerId, Vector3 worldPos)
        {
            if (worldPos.Equals(Vector3.zero) || fingerId >= _skeleton.GetCurrentNumSkinnableBones() || fingerId == (short) OVRSkeleton.BoneId.Invalid)
            {
                Debug.LogWarning("Error trying to compute distance. Rigidbody was null or incorrect finger bone id.");
                return Mathf.Infinity;
            }

            Vector3 handPos = _skeleton.Bones[fingerId].Transform.position;
            float distance = Vector3.Distance(handPos, worldPos);
            return distance;
        }

        public Vector3 GetWorldPos()
        {
            return _skeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_Wrist].Transform.position;
        }

        public Vector3 GetPalmPosition()
        {
            return _skeleton.Bones[(int)OVRSkeleton.BoneId.XRHand_Palm].Transform.position;
        }
        
        /// <summary>
        /// Attaches <see cref="CapsuleCollisionController"/> components to the bone capsules
        /// that correspond to finger collision start joints, enabling collision tracking on them.
        /// </summary>
        private void AttachColliders()
        {
            int i = 0;
            foreach (BoneCapsule capsule in physicsCapsules.Capsules)
            {
                if (TouchingFingers.FingerCollisionStartJoints.Contains(capsule.StartJoint))
                {
                    GameObject capsuleRbgo = capsule.CapsuleRigidbody.gameObject;
                    var collisionGo = capsuleRbgo.AddComponent<CapsuleCollisionController>();
                    collisionGo.name = "CapsuleCollision";
                    collisionGo.Init(capsule.StartJoint, _skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.XRHandLeft ? EHand.Left: EHand.Right);
                    i++;
                }
            }
            SpatialLogger.Instance.LogInfo($"Attached {i} controllers");
        }
        
        private void Start()
        {
            _ovrHand = handReference.GetComponent<OVRHand>();
            _skeleton = handReference.GetComponent<OVRSkeleton>();
            physicsCapsules.WhenCapsulesGenerated += AttachColliders;
        }
        
        private void OnDestroy()
        {
            physicsCapsules.WhenCapsulesGenerated -= AttachColliders;
        }

        private void ToggleRayInteraction(bool toggle)
        {
            if (rayInteractor) rayInteractor.gameObject.SetActive(toggle);
        }
    }

}
