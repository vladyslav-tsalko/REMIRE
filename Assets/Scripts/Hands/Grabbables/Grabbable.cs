using System;
using System.Collections.Generic;
using Hands.Finger;
using JetBrains.Annotations;
using LearnXR.Core.Utilities;
using Managers;
using Oculus.Interaction.Input;
using Tasks;
using UnityEngine;


public class Grabbable : MonoBehaviour
{
    private static readonly OVRSkeleton.SkeletonType RightSkeleton = OVRSkeleton.SkeletonType.XRHandRight;
    private static readonly OVRSkeleton.SkeletonType LeftSkeleton = OVRSkeleton.SkeletonType.XRHandLeft;
    
    private static readonly float FingerMaxDistance = 0.003f; //0.01 = 1cm
    private static readonly float PressScaleMultiplier = 0.05f; //Can be used for hard lvl\
    
    private readonly GrabbingFingers _grabbingFingersLeft = new();
    private readonly GrabbingFingers _grabbingFingersRight = new();
    
    private GrabbingFingers GetGrabbingFingers(OVRSkeleton.SkeletonType skeletonType) =>
        skeletonType == RightSkeleton ? _grabbingFingersRight : _grabbingFingersLeft;

    private GrabbingFingers GetGrabbingFingersOppositeHand(OVRSkeleton.SkeletonType skeletonType) =>
        skeletonType == RightSkeleton ? _grabbingFingersLeft : _grabbingFingersRight;
    

    private readonly Dictionary<HandJointId, float> _exitDistancesLeft = new();
    private readonly Dictionary<HandJointId, float> _exitDistancesRight = new();
    
    private Dictionary<HandJointId, float> GetExitDistances(OVRSkeleton.SkeletonType skeletonType) =>
        skeletonType == RightSkeleton ? _exitDistancesRight : _exitDistancesLeft;
    
    [Tooltip("Angle between hands' forward vectors. Used only in both hand grabbables. -1 for ignore")]
    [SerializeField] private int maxAcceptableHandsAngle = 120; 
    //For a cube, hands must point to different directions, unsigned delta angle = 30 -> 180-30 = 150 

    private KinematicGrabber GetKinematicGrabber(OVRSkeleton.SkeletonType skeletonType) => 
        HandsManager.Instance.GetKinematicGrabber(skeletonType);
    
    private bool IsForeignHand(OVRSkeleton.SkeletonType skeletonType) => _grabbingSkeletonType != skeletonType;
    private bool IsForeignHandHoldingAnotherObject(OVRSkeleton.SkeletonType skeletonType) => 
        IsForeignHand(skeletonType) && GetKinematicGrabber(skeletonType).IsGrabbing;
    
    private bool IsHoldByAnotherHand(OVRSkeleton.SkeletonType skeletonType) => IsHeld && IsForeignHand(skeletonType);

    [CanBeNull] public KinematicGrabber HoldingKinematicGrabber => _grabbingSkeletonType == OVRSkeleton.SkeletonType.None
        ? null
        : HandsManager.Instance.GetKinematicGrabber(_grabbingSkeletonType);

    private bool BothHandsAreGrabbing => 
        GetKinematicGrabber(LeftSkeleton).IsGrabbing && GetKinematicGrabber(RightSkeleton).IsGrabbing;
    
    
    public bool IsHeld { get; private set; }
    private Rigidbody _grabbableRb;
    private Collider _collider;
    private OVRSkeleton.SkeletonType _grabbingSkeletonType = OVRSkeleton.SkeletonType.None;

    [SerializeField] private List<FingerGrabRule> validRules = new();
    [SerializeField] private bool isBothHanded;

    private GameObject _pressBlockArea;
    private GameObject _twoHandedMidpoint;

    private Vector3 _customPressArea = Vector3.zero;
    //[SerializeField] private Material pressBlockMaterial;

    public void OnFingerCollisionEnter(HandJointId jointId, OVRSkeleton.SkeletonType skeletonType)
    {
        if (IsForeignHandHoldingAnotherObject(skeletonType) ||
            (!isBothHanded && IsHoldByAnotherHand(skeletonType))) return;
        
        var exitDistances = GetExitDistances(skeletonType);
        if (exitDistances.ContainsKey(jointId))
        {
            exitDistances.Remove(jointId);
        }

        GetGrabbingFingers(skeletonType).AddFinger(jointId);

        if (IsHeld) return;

        if (!IsGrabbingRuleSatisfied(skeletonType)) return;

        if (isBothHanded)
        {
            GetKinematicGrabber(LeftSkeleton).GrabObject(this);
            GetKinematicGrabber(RightSkeleton).GrabObject(this);
        }
        else
        {
            GetKinematicGrabber(skeletonType).GrabObject(this);
        }

        
        _grabbingSkeletonType = skeletonType;
    }

    public void OnFingerCollisionExit(HandJointId jointId, OVRSkeleton.SkeletonType skeletonType)
    {
        if (IsForeignHandHoldingAnotherObject(skeletonType) ||
            (!isBothHanded && IsHoldByAnotherHand(skeletonType))) return;
        
        var currentDistance = GetKinematicGrabber(skeletonType).
            ComputeDistanceBetweenFingerAndPoint(GrabbingFingers.GetBoneId(jointId), transform.position);
        GetExitDistances(skeletonType)[jointId] = currentDistance;
    }

    public void KinematicGrab(Transform parent)
    {
        if (isBothHanded && BothHandsAreGrabbing)
        {
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
        //transform.SetParent(parent);
        _grabbableRb.isKinematic = true;
        _collider.isTrigger = true;
    }

    public void KinematicRelease()
    {
        transform.SetParent(null);
        if (isBothHanded && BothHandsAreGrabbing)
        {
            if (_twoHandedMidpoint) Destroy(_twoHandedMidpoint);
        }
        _collider.isTrigger = false;
        _grabbableRb.isKinematic = false;
        IsHeld = false;
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
    
    private bool IsAngleBetweenHandsAcceptable()
    {
        if (!isBothHanded || maxAcceptableHandsAngle == -1) return true;
        Vector3 rightHandPalmPos = GetKinematicGrabber(RightSkeleton).GetPalmPosition();
        Vector3 leftHandPalmPos = GetKinematicGrabber(LeftSkeleton).GetPalmPosition();
        Vector3 wPos = transform.position;
        
        Vector3 toRight = (rightHandPalmPos - wPos).normalized;
        Vector3 toLeft = (leftHandPalmPos - wPos).normalized;
        
        return Vector3.Angle(toRight, toLeft) >= maxAcceptableHandsAngle;
    }
    
    void CreatePressBlockArea()
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
        
        Component[] comps = _pressBlockArea.GetComponents<Component>();
        
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

        if (_customPressArea.Equals(Vector3.zero))
        {
            _pressBlockArea.transform.localScale = Vector3.one * PressScaleMultiplier;
        }
        else
        {
            _pressBlockArea.transform.localScale = _customPressArea;
        }
    }

    public void SetPressBlockAreaSize(Difficulty difficulty)
    {
        _customPressArea = Vector3.one * (PressScaleMultiplier + PressScaleMultiplier * (float)difficulty);
        if (_pressBlockArea)
        {
            _pressBlockArea.transform.localScale = _customPressArea;
        }
    }

    private void ReleaseFinger(OVRSkeleton.SkeletonType skeletonType, HandJointId jointId)
    {
        GetExitDistances(skeletonType).Remove(jointId);
        GetGrabbingFingers(skeletonType).RemoveFinger(jointId);

        if (IsHeld && !IsGrabbingRuleSatisfied(_grabbingSkeletonType))
        {
            ReleaseObject();
            _grabbingSkeletonType = OVRSkeleton.SkeletonType.None;
        }
    }

    private void ReleaseObject()
    {
        if (isBothHanded)
        {
            GetKinematicGrabber(RightSkeleton).ReleaseObject();
            GetKinematicGrabber(LeftSkeleton).ReleaseObject();
        }
        else
        {
            GetKinematicGrabber(_grabbingSkeletonType).ReleaseObject();
        }
    }

    private void RemoveJoints(OVRSkeleton.SkeletonType skeletonType)
    {
        List<HandJointId> jointsToRemove = new();
        
        foreach (var kvp in GetExitDistances(skeletonType))
        {
            var currentDistance = GetKinematicGrabber(skeletonType).ComputeDistanceBetweenFingerAndPoint(GrabbingFingers.GetBoneId(kvp.Key), transform.position);
            if (currentDistance - kvp.Value > FingerMaxDistance)
            {
                jointsToRemove.Add(kvp.Key);
            }
        }

        foreach (var jointId in jointsToRemove)
        {
            ReleaseFinger(skeletonType, jointId);
        }
    }

    private void Update()
    {
        RemoveJoints(LeftSkeleton);
        RemoveJoints(RightSkeleton);
    }

    private void OnDestroy()
    {
        ReleaseObject();
    }

    private void UpdateTwoHandedMidpoint()
    {
        var right = GetKinematicGrabber(RightSkeleton);
        var left = GetKinematicGrabber(LeftSkeleton);

        Vector3 midPos = (right.GetPalmPosition() + left.GetPalmPosition()) * 0.5f;
        Vector3 direction = (right.GetPalmPosition() - left.GetPalmPosition()).normalized;
        Vector3 up = Vector3.up; // Or average palm up vectors

        _twoHandedMidpoint.transform.position = midPos;
        _twoHandedMidpoint.transform.rotation = Quaternion.LookRotation(direction, up);
    }

    private void LateUpdate()
    {
        if (isBothHanded && IsHeld && BothHandsAreGrabbing)
            UpdateTwoHandedMidpoint();
    }

    private bool IsGrabbingRuleSatisfied(OVRSkeleton.SkeletonType skeletonType)
    {
        foreach (var fingerGrabRule in validRules)
        {
            bool satisfy = fingerGrabRule.Matches(GetGrabbingFingers(skeletonType));
            if (isBothHanded)
            {
                if (satisfy && fingerGrabRule.Matches(GetGrabbingFingersOppositeHand(skeletonType)) && IsAngleBetweenHandsAcceptable()) return true;
            }
            else if (satisfy) return true;
        }

        return false;
    }
}
