using System;
using System.Timers;
using LearnXR.Core.Utilities;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;
using Hands.Grabbers;
using JetBrains.Annotations;

namespace Managers
{
    /// <summary>
    /// Provides access to left and right kinematic grabbers used for hand interactions.
    /// </summary>
    public class HandsManager : Singleton<HandsManager>
    {
        [SerializeField] private KinematicGrabber kinematicGrabberLeft;
        [SerializeField] private KinematicGrabber kinematicGrabberRight;

        public KinematicGrabber KinematicGrabberLeft => kinematicGrabberLeft;
        public KinematicGrabber KinematicGrabberRight => kinematicGrabberRight;

        [CanBeNull]
        public KinematicGrabber GetKinematicGrabber(EHand hand) => hand switch
        {
            EHand.Left => kinematicGrabberLeft,
            EHand.Right => kinematicGrabberRight,
            _ => null
        };
    }
}

