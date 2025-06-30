using System;
using System.Timers;
using LearnXR.Core.Utilities;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;
using Hands.Grabbers;

#nullable enable

namespace Managers
{
    public class HandsManager : Singleton<HandsManager>
    {
        [SerializeField] private KinematicGrabber kinematicGrabberLeft;
        [SerializeField] private KinematicGrabber kinematicGrabberRight;

        public KinematicGrabber KinematicGrabberLeft => kinematicGrabberLeft;
        public KinematicGrabber KinematicGrabberRight => kinematicGrabberRight;

        public KinematicGrabber? GetKinematicGrabber(EHand hand) {
            switch (hand)
            {
                case EHand.Left: return kinematicGrabberLeft;
                case EHand.Right: return kinematicGrabberRight;
                default: return null;
            }
        }
    }
}

