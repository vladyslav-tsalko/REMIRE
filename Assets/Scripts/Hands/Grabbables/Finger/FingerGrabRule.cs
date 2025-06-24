using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hands.Finger
{

    [Serializable]
    public class FingerGrabRule
    {
        [SerializeField] private EGrabRuleType grabRuleType;

        [Tooltip("If chosen AnyWithMain choose between Palm and Thumb, or both. Other will be ignored")]
        [SerializeField]
        private EFinger requiredFingers;

        public bool Matches(GrabbingFingers currentGrabbingFingers)
        {
            if (currentGrabbingFingers.IsInvalid) return false;
            var currentFingers = currentGrabbingFingers.Fingers;

            switch (grabRuleType)
            {
                case EGrabRuleType.ExactMatch:
                    return currentFingers == requiredFingers;

                case EGrabRuleType.Contains:
                    return (currentFingers & requiredFingers) == requiredFingers; // 111 & 101 = 101, 101 == 101

                case EGrabRuleType.AnyWithMain:
                    return
                        (currentFingers & requiredFingers) != EFinger.None && // at least one main finger
                        (currentFingers & ~requiredFingers) != EFinger.None; // this means that when we negate
                // required fingers 101 -> 010 and then
                // do the bitwise and, they won't be 0,
                // that means, that initial currentFingers
                // AT LEAST has the same amount of ones
                // as required + 1, example: 1101 and 1001
                case EGrabRuleType.Any:
                    return (currentFingers & requiredFingers) != EFinger.None;
            }

            return false;
        }

        private int CountSetBits(EFinger fingers)
        {
            int count = 0;
            int value = (int)fingers;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }
    }
}