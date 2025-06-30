using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oculus.Interaction.Input;


namespace Hands.Grabbers.Finger
{
    /// <summary>
    /// Tracks which fingers are currently touching an object by maintaining
    /// a dictionary of active finger joints. Provides a combined finger state as flags.
    /// </summary>
    public class TouchingFingers
    {
        /// <summary>
        /// Maps each finger to a set of active hand joint IDs currently touching.
        /// </summary>
        private readonly Dictionary<EFinger, HashSet<HandJointId>> _activeJointsPerFinger = new();
        
        /// <summary>
        /// Current touching fingers.
        /// </summary>
        public EFinger Fingers { get; private set; } = EFinger.None;

        public bool IsInvalid => Fingers == EFinger.None;

        /// <summary>
        /// Initializes empty joints sets for each finger in <see cref="_activeJointsPerFinger"/>.
        /// </summary>
        public TouchingFingers()
        {
            foreach (EFinger finger in Enum.GetValues(typeof(EFinger)))
            {
                if (finger == EFinger.None) continue;
                _activeJointsPerFinger[finger] = new HashSet<HandJointId>();
            }
        }

        /// <summary>
        /// Adds the specified finger joint ID to the set of active touching fingers,
        /// updating the combined finger flags accordingly.
        /// </summary>
        /// <param name="jointId">The joint ID of the finger to add.</param>
        /// <remarks>
        /// A finger is considered touching the object if at least one of its joint IDs is active.
        /// </remarks>
        public void AddFinger(HandJointId jointId)
        {
            EFinger finger = MapFinger(jointId);
            if (finger == EFinger.None) return;
            
            _activeJointsPerFinger[finger].Add(jointId); //If contains - won't be added
            
            Fingers |= finger; //If contains - nothing changes
        }
        
        /// <summary>
        /// Removes the specified finger joint ID from the set of active touching fingers,
        /// updating the combined finger flags accordingly.
        /// </summary>
        /// <param name="jointId">The joint ID of the finger to remove.</param>
        /// <remarks>
        /// A finger is considered no longer touching the object if none of its joint IDs remain active.
        /// </remarks>
        public void RemoveFinger(HandJointId jointId)
        {
            EFinger finger = MapFinger(jointId);
            if (finger == EFinger.None) return;
            
            var joints = _activeJointsPerFinger[finger];
            joints.Remove(jointId);
            
            if (joints.Count == 0)
            {
                Fingers &= ~finger;
            }
        }

        /// <summary>
        /// Retrieves the corresponding bone ID for the specified hand joint ID.
        /// </summary>
        /// <param name="handJointId">The hand joint ID to map.</param>
        /// <returns>
        /// The bone ID associated with the given joint ID, or
        /// <see cref="OVRSkeleton.BoneId.Invalid"/> if the joint ID is not recognized.
        /// </returns>
        public static short GetBoneId(HandJointId handJointId)
        {
            if (ThumbCollisionStartJoints.Contains(handJointId)) 
                return (short) ThumbCollisionBones[ThumbCollisionStartJoints.IndexOf(handJointId)];
            if (IndexCollisionStartJoints.Contains(handJointId)) 
                return (short) IndexCollisionBones[IndexCollisionStartJoints.IndexOf(handJointId)];
            if (MiddleCollisionStartJoints.Contains(handJointId)) 
                return (short) MiddleCollisionBones[MiddleCollisionStartJoints.IndexOf(handJointId)];
            if (RingCollisionStartJoints.Contains(handJointId)) 
                return (short) RingCollisionBones[RingCollisionStartJoints.IndexOf(handJointId)];
            if (PinkyCollisionStartJoints.Contains(handJointId)) 
                return (short) PinkyCollisionBones[PinkyCollisionStartJoints.IndexOf(handJointId)];
            return PalmCollisionStartJoints.Contains(handJointId)
                ? (short)OVRSkeleton.BoneId.XRHand_Palm
                : (short)OVRSkeleton.BoneId.Invalid;
        }
        
        /// <summary>
        /// Maps a given hand joint ID to its corresponding finger enumeration value.
        /// </summary>
        /// <param name="jointIndex">The hand joint ID to map.</param>
        /// <returns>
        /// The <see cref="EFinger"/> corresponding to the provided joint ID,
        /// or <see cref="EFinger.None"/> if no match is found.
        /// </returns>
        private static EFinger MapFinger(HandJointId jointIndex)
        {
            if (PalmCollisionStartJoints.Contains(jointIndex)) return EFinger.Palm;
            if (ThumbCollisionStartJoints.Contains(jointIndex)) return EFinger.Thumb;
            if (IndexCollisionStartJoints.Contains(jointIndex)) return EFinger.Index;
            if (MiddleCollisionStartJoints.Contains(jointIndex)) return EFinger.Middle;
            if (RingCollisionStartJoints.Contains(jointIndex)) return EFinger.Ring;
            return PinkyCollisionStartJoints.Contains(jointIndex) ? EFinger.Pinky : EFinger.None;
        }

        #region FingerJointsCollections

        private static readonly ReadOnlyCollection<HandJointId> ThumbCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandThumb2, // Thumb Proximal Joint
                HandJointId.HandThumb3  // Thumb Distal Joint
            });
        
        private static readonly ReadOnlyCollection<HandJointId> IndexCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandIndex2,   // Index Intermediate
                HandJointId.HandIndex3,   // Index Distal
            });
        
        private static readonly ReadOnlyCollection<HandJointId> MiddleCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandMiddle2,  // Middle Intermediate
                HandJointId.HandMiddle3,  // Middle Distal
            });
        
        private static readonly ReadOnlyCollection<HandJointId> RingCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandRing2,    // Ring Intermediate
                HandJointId.HandRing3,    // Ring Distal
            });
        
        private static readonly ReadOnlyCollection<HandJointId> PinkyCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandPinky2,   // Pinky Intermediate
                HandJointId.HandPinky3    // Pinky Distal
            });
        
        private static readonly ReadOnlyCollection<HandJointId> PalmCollisionStartJoints =
            Array.AsReadOnly(new[]
            {
                HandJointId.HandPalm,       // Palm
                /*HandJointId.HandThumb1,     // Thumb Metacarpal
                HandJointId.HandThumb2,     // Thumb Proximal
                HandJointId.HandIndex0,     // Index Metacarpal
                HandJointId.HandMiddle0,    // Middle Metacarpal
                HandJointId.HandRing0,      // Ring Metacarpal
                HandJointId.HandPinky0,     // Pinky Metacarpal*/
            });

        public static readonly ReadOnlyCollection<HandJointId> FingerCollisionStartJoints =
            Array.AsReadOnly(
                ThumbCollisionStartJoints
                    .Concat(IndexCollisionStartJoints)
                    .Concat(MiddleCollisionStartJoints)
                    .Concat(RingCollisionStartJoints)
                    .Concat(PinkyCollisionStartJoints)
                    .ToArray()
            );

        #endregion

        #region FingerBoneCollections
        
        private static readonly ReadOnlyCollection<OVRSkeleton.BoneId> ThumbCollisionBones =
            Array.AsReadOnly(new[]
            {
                OVRSkeleton.BoneId.XRHand_ThumbProximal, // Thumb Proximal Bone
                OVRSkeleton.BoneId.XRHand_ThumbDistal    // Thumb Distal Bone
            });
        
        private static readonly ReadOnlyCollection<OVRSkeleton.BoneId> IndexCollisionBones =
            Array.AsReadOnly(new[]
            {
                OVRSkeleton.BoneId.XRHand_IndexIntermediate, // Index Intermediate Bone
                OVRSkeleton.BoneId.XRHand_IndexDistal        // Index Distal Bone
            });
        
        private static readonly ReadOnlyCollection<OVRSkeleton.BoneId> MiddleCollisionBones =
            Array.AsReadOnly(new[]
            {
                OVRSkeleton.BoneId.XRHand_MiddleIntermediate, // Middle Intermediate Bone
                OVRSkeleton.BoneId.XRHand_MiddleDistal        // Middle Distal Bone
            });
        
        private static readonly ReadOnlyCollection<OVRSkeleton.BoneId> RingCollisionBones =
            Array.AsReadOnly(new[]
            {
                OVRSkeleton.BoneId.XRHand_RingIntermediate, // Ring Intermediate Bone
                OVRSkeleton.BoneId.XRHand_RingDistal        // Ring Distal Bone
            });
        
        private static readonly ReadOnlyCollection<OVRSkeleton.BoneId> PinkyCollisionBones =
            Array.AsReadOnly(new[]
            {
                OVRSkeleton.BoneId.XRHand_LittleIntermediate, // Little Intermediate Bone
                OVRSkeleton.BoneId.XRHand_LittleDistal        // Little Distal Bone
            });

        #endregion
    }

}
