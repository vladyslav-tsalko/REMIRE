using System.Collections.Generic;
using System.Linq;
using Managers;
using Oculus.Interaction.Input;
using UnityEngine;
using Hands.Grabbers.Finger;
using Hands.Grabbables.Finger;
using Hands.Grabbers;
using Tasks.TaskProperties;

namespace Hands.Grabbables
{ 
    /// <summary>
    /// Attach this class to any object that needs to be grabbed by a KinematicGrabber.
    /// The object can be grabbed with one or two hands. While grabbed, it's physical properties
    /// are temporarily disabled, and it attaches either to the grabber or to a midpoint between both hands.
    /// </summary>
    public class KinematicGrabbable : MonoBehaviour
    {
        #region Static Variables

        private static readonly EHand RightHand = EHand.Right;
        private static readonly EHand LeftHand = EHand.Left;
        
        private static readonly float FingerMaxDeltaDistance = 0.003f; //0.01 = 1cm
        private static readonly float PressScaleMultiplier = 0.05f; //Can be used for hard lvl

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
        
        private Dictionary<HandJointId, float> GetExitDistances(EHand hand) =>
            hand == RightHand ? _exitDistancesRight : _exitDistancesLeft;
        
        private KinematicGrabber GetKinematicGrabber(EHand hand) => HandsManager.Instance.GetKinematicGrabber(hand);
        
        #endregion
        
        #region Checkers
        
        private bool IsForeignHand(EHand hand) => _grabbingHand != EHand.Both && _grabbingHand != hand;
        
        private bool IsForeignAndHoldingAnotherObject(EHand hand) => IsForeignHand(hand) && GetKinematicGrabber(hand).IsGrabbing;
        
        private bool IsHeldByAnotherHand(EHand hand) => !isBothHanded && IsHeld && IsForeignHand(hand);

        private bool BothHandsAreGrabbing => 
            GetKinematicGrabber(LeftHand).IsGrabbing && GetKinematicGrabber(RightHand).IsGrabbing;
        
        public bool IsHeld { get; private set; }
        
        #endregion
        
        
        [Tooltip("Maximum allowed angle (in degrees) between the object’s center and the palms of both hands." +
                 " Used only for two-hand grabbables. Set to -1 to ignore.")]
        [SerializeField] private int maxAcceptableHandsAngle = 120; 
        
        [Tooltip("List of rules that define how the object can be grabbed.")]
        [SerializeField] private List<FingerGrabRule> validRules = new();
        
        [Tooltip("True, if the object can be grabbed with both hands")]
        [SerializeField] private bool isBothHanded;
        

        private EHand _grabbingHand = EHand.None;
        private Rigidbody _grabbableRb;
        private Collider _collider;
        private GameObject _twoHandedMidpoint;

        /// <summary>
        /// Internal collider used not only to simulate realistic touch interactions,
        /// but also to detect excessive pressure applied to the object.
        /// It has the same mesh collider as the object but is scaled by <see cref="_customPressAreaScale"/>.
        /// When any finger collides with this area, it simulates breakage of the object.
        /// </summary>
        private GameObject _pressBlockArea;
        private Vector3 _customPressAreaScale = Vector3.zero;
        
        /// <summary>
        /// Called when any finger collides with the object. 
        /// If a valid grabbing rule is satisfied, it triggers <see cref="KinematicGrabber.GrabObject"/> on
        /// the corresponding hand (or both hands if two-handed).
        /// </summary>
        /// <param name="jointId">The joint ID of the finger.</param>
        /// <param name="hand">The hand (left or right) from which the finger belongs.</param>
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
        
        /// <summary>
        /// Called when any finger leaves the object.
        /// </summary>
        /// <param name="jointId">The joint ID of the finger.</param>
        /// <param name="hand">The hand (left or right) from which the finger belongs.</param>
        /// <remarks>
        /// While an object is being grabbed, it no longer participates in physical collisions and
        /// instead relies on trigger-based volume overlaps. Triggers are less precise than collisions and
        /// can be trigger both Enter and Exit by mistake events unintentionally.
        /// To mitigate this, the distance between the finger's exit point and the object's center is saved,
        /// allowing a check to determine whether the finger is truly too far to remain engaged or
        /// if it has re-entered the interaction zone.
        /// </remarks>
        public void OnFingerCollisionExit(HandJointId jointId, EHand hand)
        {
            if (IsForeignAndHoldingAnotherObject(hand) || IsHeldByAnotherHand(hand)) return;
            
            var currentDistance = GetKinematicGrabber(hand).
                ComputeDistanceBetweenFingerAndPoint(TouchingFingers.GetBoneId(jointId), transform.position);
            GetExitDistances(hand)[jointId] = currentDistance;
        }
        
        /// <summary>
        /// Called from <see cref="KinematicGrabber.GrabObject"/> when the object is grabbed.
        /// </summary>
        /// <param name="parent">The transform of the grabber (used only in one-handed grabbing).</param>
        /// <remarks>
        /// Sets the object’s parent transform to either the grabber or a midpoint between both hands,
        /// depending on whether two-handed grabbing is enabled. Also disables physics interactions.
        /// </remarks>
        public void KinematicGrab(Transform parent)
        {
            if (isBothHanded)
            {
                if (!BothHandsAreGrabbing) return;
                if (!_twoHandedMidpoint)
                    _twoHandedMidpoint = Instantiate(TaskObjectPrefabsManager.Instance.grabMidpointPrefab);

                UpdateTwoHandedMidpoint();

                transform.SetParent(_twoHandedMidpoint.transform);
            }
            else
            {
                transform.SetParent(parent);
            }
            
            IsHeld = true;
            TogglePhysicsInteractions(true);
        }

        /// <summary>
        /// Called from <see cref="KinematicGrabber.ReleaseObject"/> when the object is released.
        /// </summary>
        /// <remarks>
        /// Removes the parent, destroys midpoint if two-handed grabbing is enabled, enables physics interactions.
        /// </remarks>
        public void KinematicRelease()
        {
            if (!transform.parent) return;

            transform.SetParent(null);
            if (isBothHanded && _twoHandedMidpoint)
            {
                Destroy(_twoHandedMidpoint);
            }

            TogglePhysicsInteractions(false);
            IsHeld = false;
        }
        
        /// <summary>
        /// Sets the size of the press block area collider based on the specified difficulty level.
        /// </summary>
        /// <param name="difficulty">The difficulty level that influences the scale of the press block area.</param>
        public void SetPressBlockAreaSize(Difficulty difficulty)
        {
            _customPressAreaScale = Vector3.one * (PressScaleMultiplier + PressScaleMultiplier * (float)difficulty);
            if (_pressBlockArea)
            {
                _pressBlockArea.transform.localScale = _customPressAreaScale;
            }
        }

        
        /// <summary>
        /// Toggles the object's physics interactions: Rigidbody's kinematic state and Collider's trigger state.
        /// </summary>
        private void TogglePhysicsInteractions(bool toggle)
        {
            _grabbableRb.isKinematic = toggle;
            _collider.isTrigger = toggle;
        }
        
        
        /// <summary>
        /// Checks if the angle between the palms of both hands is acceptable for grabbing.
        /// Returns true if the object is not two-handed or if angle checking is disabled (-1).
        /// Otherwise, compares the angle between hands to the configured maximum acceptable angle.
        /// </summary>
        /// <returns>True if the angle between hands meets or exceeds the acceptable threshold; otherwise false.</returns>
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
        
        /// <summary>
        /// Handles the release of a finger from the object by removing its joint ID from tracking collections.
        /// If the object is currently held but no longer satisfies the grabbing rules, it releases the object.
        /// </summary>
        /// <param name="hand">The hand (left or right) releasing the finger.</param>
        /// <param name="jointId">The joint ID of the finger being released.</param>
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
        
        /// <summary>
        /// Determines whether the specified hand satisfies any of the valid finger grabbing rules
        /// based on the fingers currently touching the object.
        /// </summary>
        /// <param name="hand">The hand (left or right) to evaluate.</param>
        /// <returns>True if any grabbing rule is matched; otherwise, false.</returns>
        private bool IsHandSatisfiesGrabbingRule(EHand hand)
        {
            return validRules.Any(fingerGrabRule => fingerGrabRule.Matches(GetTouchingFingers(hand)));
        }
        
        /// <summary>
        /// Checks if the grabbing rules are satisfied for the given hand or hands.
        /// </summary>
        /// <param name="hand">The hand (left or right) to check grabbing rules for.</param>
        /// <returns>True if the grabbing conditions are met; otherwise, false.</returns>
        /// <remarks>
        /// For one-handed objects, verifies the specified hand against grabbing rules.
        /// For two-handed objects, verifies both hands satisfy the grabbing rules and
        /// that the angle between hands is acceptable.
        /// </remarks>
        private bool IsGrabbingRuleSatisfied(EHand hand)
        {
            return !isBothHanded ? 
                IsHandSatisfiesGrabbingRule(hand):
                IsHandSatisfiesGrabbingRule(EHand.Right) && 
                IsHandSatisfiesGrabbingRule(EHand.Left) && 
                IsAngleBetweenHandsAcceptable();
        }

        /// <summary>
        /// Removes finger joints from tracking if their distance from the object has exceeded
        /// the maximum allowed threshold since their last recorded exit distance.
        /// </summary>
        /// <param name="hand">The hand (left or right) whose joints are being evaluated for removal.</param>
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

        /// <summary>
        /// Updates the position and rotation of the two-handed midpoint object
        /// to be centered between both hands' palm positions and oriented
        /// along the direction from the left hand to the right hand.
        /// </summary>
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
        
        /// <summary>
        /// Creates a press block area by duplicating the current game object and stripping
        /// all non-essential components except Transform and Collider. Sets up the duplicated
        /// object as a trigger collider with an adjustable scale used for press interaction detection.
        /// </summary>
        private void CreatePressBlockArea()
        {
            _pressBlockArea = Instantiate(gameObject, transform);
            
            _pressBlockArea.name = "CompressedArea";
            _pressBlockArea.tag = "PressBlockArea";
            _pressBlockArea.transform.localPosition = Vector3.zero;
            _pressBlockArea.transform.localRotation = Quaternion.identity;
            
            for (int i = _pressBlockArea.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(_pressBlockArea.transform.GetChild(i).gameObject);
            }
            
            var comps = _pressBlockArea.GetComponents<Component>();
            
            foreach (Component comp in comps)
            {
                if (comp is Transform or Collider) continue;
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

            if (_customPressAreaScale.Equals(Vector3.zero))
            {
                _pressBlockArea.transform.localScale = Vector3.one * PressScaleMultiplier * 2;
            }
            else
            {
                _pressBlockArea.transform.localScale = _customPressAreaScale;
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
            if (isBothHanded && IsHeld) UpdateTwoHandedMidpoint();
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
    }

}
