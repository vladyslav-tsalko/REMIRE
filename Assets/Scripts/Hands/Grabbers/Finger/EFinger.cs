namespace Hands.Grabbers.Finger
{
    /// <summary>
    /// Flags enum representing individual fingers and the palm,
    /// allowing combination of multiple finger states using bitwise operations.
    /// </summary>
    [System.Flags]
    public enum EFinger
    {
        None   = 0,
        Palm   = 1 << 0,
        Thumb  = 1 << 1,
        Index  = 1 << 2,
        Middle = 1 << 3,
        Ring   = 1 << 4,
        Pinky  = 1 << 5
    }
}