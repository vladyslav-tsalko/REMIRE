using System;
using UnityEngine;
using Hands.Grabbables;
using Hands.Grabbers.Finger;
using LearnXR.Core.Utilities;
using Oculus.Interaction.Input;
using Oculus.Interaction;

namespace Hands.Grabbers
{
    public class KinematicGrabber: MonoBehaviour
    {
        public static event Action<KinematicGrabbable, KinematicGrabber> OnGrabEnter;
        public static event Action<KinematicGrabbable, KinematicGrabber> OnGrabExit;
        
        [SerializeField]
        [Tooltip("If Object Parenting is used (KinematicGrabber), grabbed object will be childed to this Transform." +
                 " If Object Parenting is disabled, object will follow the movement of this Transform which will in turn be" + 
                 " recalculated every time a collider is added to collision list for grabbed object.")]
        public GameObject handReference;
        
        [SerializeField] private RayInteractor rayInteractor;
        
        private OVRHand _ovrHand;
        
        public bool IsActive => _ovrHand != null && _ovrHand.IsTracked;


        [SerializeField]
        private HandPhysicsCapsules physicsCapsules;
        //public HandState State { get; set; } = HandState.DEFAULT;
        
        [SerializeField]
        protected OVRSkeleton skeleton;
        
        protected KinematicGrabbable GrabbedObject;
        
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
        
        
        public void GrabObject(KinematicGrabbable go)
        {
            if (IsGrabbing) return;
            if (rayInteractor) rayInteractor.gameObject.SetActive(false);
            GrabbedObject = go;
            OnGrabEnter?.Invoke(go, this);
            GrabbedObject.KinematicGrab(handReference.transform);
        }
        
        public void ReleaseObject()
        {
            if (GrabbedObject)
            {
                GrabbedObject.KinematicRelease();
                OnGrabExit(GrabbedObject, this);
                GrabbedObject = null;
            }
            if (rayInteractor) rayInteractor.gameObject.SetActive(true);
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

}
