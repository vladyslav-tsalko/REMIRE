using Oculus.Interaction.Input;

namespace Hands.Finger
{
    public struct SkeletonHandJointId
    {
        public readonly HandJointId JointId;
        public readonly OVRSkeleton.SkeletonType SkeletonType;
        
        public SkeletonHandJointId(HandJointId jointId, OVRSkeleton.SkeletonType skeletonType)
        {
            JointId = jointId;
            SkeletonType = skeletonType;
        }
        
        public override bool Equals(object obj) =>
            obj is SkeletonHandJointId other &&
            JointId == other.JointId &&
            SkeletonType == other.SkeletonType;
    }
}