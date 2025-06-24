using UnityEngine;

// This script can be attached to a grabbable object itself to change behaviour when it is grabbed
// or to other objects such as hand fingers or environment objects.
public class OnGrabBehaviour : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Material to change to when this behaviour is triggered.")]
    private Material materialOnGrabEnter = null;

    private Material materialOnGrabExit = null;
    private Renderer rend = null;

    [SerializeField]
    [Tooltip("HandController that should trigger this behaviour. If empty, this behaviour will trigger for both hands.")]
    private BaseGrabber grabbingHand = null;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        materialOnGrabExit = rend.material;

        BaseGrabber.OnGrabEnter += OnGrabEnter;
        BaseGrabber.OnGrabExit += OnGrabExit;
    }

    private void OnDestroy()
    {
        BaseGrabber.OnGrabEnter -= OnGrabEnter;
        BaseGrabber.OnGrabExit -= OnGrabExit;
    }

    private void OnGrabEnter(Grabbable go, BaseGrabber hand)
    {
        if (grabbingHand && hand != grabbingHand) return;

        if (materialOnGrabEnter)
            rend.material = materialOnGrabEnter;
    }

    private void OnGrabExit(Grabbable go, BaseGrabber hand)
    {
        if (grabbingHand && hand != grabbingHand) return;
        
        if (materialOnGrabExit)
            rend.material = materialOnGrabExit;
    }
}
