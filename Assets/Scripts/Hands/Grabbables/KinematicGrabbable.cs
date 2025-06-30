using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Managers;
using Oculus.Interaction.Input;
using UnityEngine;
using Hands.Grabbers.Finger;
using Hands.Grabbables.Finger;
using Hands.Grabbers;
using LearnXR.Core.Utilities;
using Tasks.TaskProperties;

namespace Hands.Grabbables
{ 
    public class KinematicGrabbable : MonoBehaviour
    {
        #region Static Variables

        private static readonly EHand RightHand = EHand.Right;
        private static readonly EHand LeftHand = EHand.Left;
        
        private static readonly float FingerMaxDeltaDistance = 0.003f; //0.01 = 1cm
        private static readonly float PressScaleMultiplier = 0.05f; //Can be used for hard lvl\

        #endregion

        #region Left Fingers

        private readonly TouchingFingers _touchingFingersLeft = new();
        private readonly Dictionary<HandJointId, float> _exitDistancesLeft = new();

        #endregion
        
        #region Right Fingers

        private readonly TouchingFingers _touchingFingersRight = new();
        private readonly Dictionary<HandJointId, float> _exitDistancesRight = new();

        #endregion
        
        #region Getters
        
        private TouchingFingers GetTouchingFingers(EHand hand) =>
            hand == RightHand ? _touchingFingersRight : _touchingFingersLeft;
        
        
        /*private GrabbingFingers GetGrabbingFingersOppositeHand(EHand hand) =>
            hand == RightSkeleton ? _grabbingFingersLeft : _grabbingFingersRight;*/
        
        
        private Dictionary<HandJointId, float> GetExitDistances(EHand hand) =>
            hand == RightHand ? _exitDistancesRight : _exitDistancesLeft;
        

        private KinematicGrabber GetKinematicGrabber(EHand hand) => 
            HandsManager.Instance.GetKinematicGrabber(hand);
        
        #endregion
        
        #region Checkers
        
        private bool IsForeignHand(EHand hand) => 
            _grabbingHand != EHand.Both &&
            _grabbingHand != hand;
        
        private bool IsForeignAndHoldingAnotherObject(EHand hand) => IsForeignHand(hand) && GetKinematicGrabber(hand).IsGrabbing;
        
        private bool IsHeldByAnotherHand(EHand hand) => !isBothHanded && IsHeld && IsForeignHand(hand);

        private bool BothHandsAreGrabbing => 
            GetKinematicGrabber(LeftHand).IsGrabbing && GetKinematicGrabber(RightHand).IsGrabbing;
        
        public bool IsHeld { get; private set; }
        
        #endregion
        
        
        //For a cube, hands must point to different directions, unsigned delta angle = 30 -> 180-30 = 150 
        [Tooltip("Angle between object's center and hands' palm positions. Used only in both hand grabbables. -1 for ignore")]
        [SerializeField] private int maxAcceptableHandsAngle = 120; 

        [SerializeField] private List<FingerGrabRule> validRules = new();
        [SerializeField] private bool isBothHanded;
        
        private EHand _grabbingHand = EHand.None;
        
        private Rigidbody _grabbableRb;
        private Collider _collider;
        private GameObject _pressBlockArea;
        private GameObject _twoHandedMidpoint;

        private Vector3 _customPressArea = Vector3.zero;
        
        
        // do nothing if (this hand is holding another object) or (this object is held by another hand)
        public void OnFingerCollisionEnter(HandJointId jointId, EHand hand)
        {
            if (IsForeignAndHoldingAnotherObject(hand) || IsHeldByAnotherHand(hand)) return;
            
            var exitDistances = GetExitDistances(hand);
            if (exitDistances.ContainsKey(jointId))
            {
                exitDistances.Remove(jointId);
            }

            GetTouchingFingers(hand).AddFinger(jointId);

            if (IsHeld) return;

            if (!IsGrabbingRuleSatisfied(hand)) return;

            if (isBothHanded)
            {
                GetKinematicGrabber(LeftHand).GrabObject(this);
                GetKinematicGrabber(RightHand).GrabObject(this);
                _grabbingHand = EHand.Both;
            }
            else
            {
                GetKinematicGrabber(hand).GrabObject(this);
                _grabbingHand = hand;
            }
        }

        public void OnFingerCollisionExit(HandJointId jointId, EHand hand)
        {
            if (IsForeignAndHoldingAnotherObject(hand) || IsHeldByAnotherHand(hand)) return;
            
            var currentDistance = GetKinematicGrabber(hand).
                ComputeDistanceBetweenFingerAndPoint(TouchingFingers.GetBoneId(jointId), transform.position);
            GetExitDistances(hand)[jointId] = currentDistance;
        }

        
        public void KinematicGrab(Transform parent)
        {
            if (isBothHanded)
            {
                if (!BothHandsAreGrabbing) return;
                if (!_twoHandedMidpoint)
                    _twoHandedMidpoint = Instantiate(TaskObjectPrefabsManager.Instance.GrabMidpointPrefab);

                UpdateTwoHandedMidpoint();

                transform.SetParent(_twoHandedMidpoint.transform);
            }
            else
            {
                transform.SetParent(parent);
            }
            
            IsHeld = true;
            _grabbableRb.isKinematic = true;
            _collider.isTrigger = true;
        }

        public void KinematicRelease()
        {
            if (!transform.parent) return;

            transform.SetParent(null);
            if (isBothHanded && _twoHandedMidpoint)
            {
                Destroy(_twoHandedMidpoint);
            }
            _collider.isTrigger = false;
            _grabbableRb.isKinematic = false;
            IsHeld = false;
        }
        
        public void SetPressBlockAreaSize(Difficulty difficulty)
        {
            _customPressArea = Vector3.one * (PressScaleMultiplier + PressScaleMultiplier * (float)difficulty);
            if (_pressBlockArea)
            {
                _pressBlockArea.transform.localScale = _customPressArea;
            }
        }
        
        private bool IsAngleBetweenHandsAcceptable()
        {
            if (!isBothHanded || maxAcceptableHandsAngle == -1) return true;
            Vector3 rightHandPalmPos = GetKinematicGrabber(RightHand).GetPalmPosition();
            Vector3 leftHandPalmPos = GetKinematicGrabber(LeftHand).GetPalmPosition();
            Vector3 wPos = transform.position;
            
            Vector3 toRight = (rightHandPalmPos - wPos).normalized;
            Vector3 toLeft = (leftHandPalmPos - wPos).normalized;
            
            return Vector3.Angle(toRight, toLeft) >= maxAcceptableHandsAngle;
        }

        private void ReleaseObject()
        {
            if (isBothHanded)
            {
                GetKinematicGrabber(RightHand).ReleaseObject();
                GetKinematicGrabber(LeftHand).ReleaseObject();
            }
            else
            {
                GetKinematicGrabber(_grabbingHand).ReleaseObject();
            }
        }
        
        private void ReleaseFinger(EHand hand, HandJointId jointId)
        {
            GetExitDistances(hand).Remove(jointId);
            GetTouchingFingers(hand).RemoveFinger(jointId);

            if (IsHeld && !IsGrabbingRuleSatisfied(_grabbingHand))
            {
                ReleaseObject();
                _grabbingHand = EHand.None;
            }
        }

        private void RemoveJoints(EHand hand)
        {
            List<HandJointId> jointsToRemove = new();
            
            foreach (var kvp in GetExitDistances(hand))
            {
                var currentDistance = GetKinematicGrabber(hand).ComputeDistanceBetweenFingerAndPoint(TouchingFingers.GetBoneId(kvp.Key), transform.position);
                if (currentDistance - kvp.Value > FingerMaxDeltaDistance)
                {
                    jointsToRemove.Add(kvp.Key);
                }
            }

            foreach (var jointId in jointsToRemove)
            {
                ReleaseFinger(hand, jointId);
            }
        }

        private void UpdateTwoHandedMidpoint()
        {
            var right = GetKinematicGrabber(RightHand);
            var left = GetKinematicGrabber(LeftHand);

            Vector3 midPos = (right.GetPalmPosition() + left.GetPalmPosition()) * 0.5f;
            Vector3 direction = (right.GetPalmPosition() - left.GetPalmPosition()).normalized;
            Vector3 up = Vector3.up; // Or average palm up vectors

            _twoHandedMidpoint.transform.position = midPos;
            _twoHandedMidpoint.transform.rotation = Quaternion.LookRotation(direction, up);
        }
        
        private void CreatePressBlockArea()
        {
            _pressBlockArea = Instantiate(gameObject, transform);
            
            _pressBlockArea.name = "CompressedArea";
            _pressBlockArea.transform.localPosition = Vector3.zero;
            _pressBlockArea.transform.localRotation = Quaternion.identity;
            _pressBlockArea.tag = "PressBlockArea";
            
            for (int i = _pressBlockArea.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(_pressBlockArea.transform.GetChild(i).gameObject);
            }
            
            var comps = _pressBlockArea.GetComponents<Component>();
            
            foreach (Component comp in comps)
            {
                if (comp is Transform || comp is Collider) continue;
                Destroy(comp);
            }
            
            if (_pressBlockArea.TryGetComponent(out Collider col))
            {
                if (col is MeshCollider meshCol)
                {
                    meshCol.convex = true;
                }
                col.isTrigger = true;
            }
            
            //This method is called after SetPressBlockAreaSize, but for safety reasons I check if

            if (_customPressArea.Equals(Vector3.zero))
            {
                _pressBlockArea.transform.localScale = Vector3.one * PressScaleMultiplier;
            }
            else
            {
                _pressBlockArea.transform.localScale = _customPressArea;
            }
        }
        
        private void Awake()
        {
            _grabbableRb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        private void Start()
        {
            CreatePressBlockArea();
        }

        private void LateUpdate()
        {
            if (isBothHanded && IsHeld/* && BothHandsAreGrabbing*/) //TODO: check both hand grabbing
                UpdateTwoHandedMidpoint();
        }
        
        private void Update()
        {
            RemoveJoints(LeftHand);
            RemoveJoints(RightHand);
        }

        private void OnDestroy()
        {
            ReleaseObject();
        }

        private bool IsHandSatisfiesGrabbingRule(EHand hand)
        {
            return validRules.Any(fingerGrabRule => fingerGrabRule.Matches(GetTouchingFingers(hand)));
        }

        private bool IsGrabbingRuleSatisfied(EHand hand)
        {
            return !isBothHanded ? 
                IsHandSatisfiesGrabbingRule(hand):
                IsHandSatisfiesGrabbingRule(EHand.Right) && 
                IsHandSatisfiesGrabbingRule(EHand.Left) && 
                IsAngleBetweenHandsAcceptable();
        }
    }

}
