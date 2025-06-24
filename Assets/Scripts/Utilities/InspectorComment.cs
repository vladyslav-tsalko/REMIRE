using UnityEngine;

public class InspectorComment : MonoBehaviour
{
    [TextArea(3, 10)] // Adjustable size: 3 lines min, 10 lines max
    public string comment = "Enter your notes here.";
}
