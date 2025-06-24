using System;
using System.Timers;
using LearnXR.Core.Utilities;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public class HandsManager : Singleton<HandsManager>
{
    [SerializeField] private KinematicGrabber kinematicGrabberLeft;
    [SerializeField] private KinematicGrabber kinematicGrabberRight;

    public KinematicGrabber KinematicGrabberLeft => kinematicGrabberLeft;
    public KinematicGrabber KinematicGrabberRight => kinematicGrabberRight;

    public KinematicGrabber GetKinematicGrabber(OVRSkeleton.SkeletonType skeletonType) =>
        skeletonType == OVRSkeleton.SkeletonType.XRHandRight ? kinematicGrabberRight : kinematicGrabberLeft;

    /*public float GetDistanceFromJointToObject(OVRSkeleton.SkeletonType skeletonType, short boneId, Vector3 worldPos)
    {
        return GetKinematicGrabber(skeletonType).ComputeDistanceBetweenFingerAndPoint(boneId, worldPos);
    }*/
}