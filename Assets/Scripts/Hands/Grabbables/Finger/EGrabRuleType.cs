namespace Hands.Grabbables.Finger
{
    /// <summary>
    /// Defines the rule for finger combination by which an object can be grabbed.
    /// </summary>
    public enum EGrabRuleType
    {
        /// <summary>
        /// Touching fingers exactly match the required fingers.
        /// </summary>
        ExactMatch,

        /// <summary>
        /// Touching fingers include all the required fingers.
        /// </summary>
        Contains,

        /// <summary>
        /// Matches when an object is touched by a main finger and at least one additional finger
        /// </summary>
        AnyWithMain,

        /// <summary>
        /// An object is touched by at least one finger
        /// </summary>
        Any
    }
}