using UnityEngine;
using Hands.Grabbables;

// This script can be attached to a grabbable object itself to change behaviour when it is grabbed
// or to other objects such as hand fingers or environment objects.

namespace Hands.Grabbers
{
    /// <summary>
    /// Changes the material of the attached object when it is grabbed or released.
    /// </summary>
    public class OnGrabBehaviour : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Material to apply when an object is grabbed.")]
        private Material materialOnGrabEnter;

        [SerializeField]
        [Tooltip("The hand controller that should trigger this behaviour. If left empty, the behaviour triggers for grabs from both hands.")]
        private KinematicGrabber grabbingHand;

        private Material _materialOnGrabExit;
        private Renderer _rend;
        
        private void Start()
        {
            _rend = GetComponent<Renderer>();
            _materialOnGrabExit = _rend.material;

            KinematicGrabber.OnGrabEnter += OnGrabEnter;
            KinematicGrabber.OnGrabExit += OnGrabExit;
        }

        private void OnDestroy()
        {
            KinematicGrabber.OnGrabEnter -= OnGrabEnter;
            KinematicGrabber.OnGrabExit -= OnGrabExit;
        }

        private void OnGrabEnter(KinematicGrabbable go, KinematicGrabber hand)
        {
            if (grabbingHand && hand != grabbingHand) return;

            if (materialOnGrabEnter)
                _rend.material = materialOnGrabEnter;
        }

        private void OnGrabExit(KinematicGrabbable go, KinematicGrabber hand)
        {
            if (grabbingHand && hand != grabbingHand) return;
        
            if (_materialOnGrabExit)
                _rend.material = _materialOnGrabExit;
        }
    }
}


