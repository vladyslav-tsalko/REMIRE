using System;
using UnityEngine;
using Hands.Grabbers.Finger;

namespace Hands.Grabbables.Finger
{
    /// <summary>
    /// Defines a grab rule based on specific combinations of fingers.
    /// </summary>
    [Serializable]
    public class FingerGrabRule
    {
        [SerializeField] private EGrabRuleType grabRuleType;

        [SerializeField] private EFinger requiredFingers;

        /// <summary>
        /// Checks if the current grabbing fingers match the defined rule.
        /// </summary>
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
    }
}