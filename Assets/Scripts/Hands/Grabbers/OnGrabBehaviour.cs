using UnityEngine;
using Hands.Grabbables;

// This script can be attached to a grabbable object itself to change behaviour when it is grabbed
// or to other objects such as hand fingers or environment objects.

namespace Hands.Grabbers
{
    public class OnGrabBehaviour : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Material to change to when this behaviour is triggered.")]
        private Material materialOnGrabEnter;

        private Material _materialOnGrabExit;
        private Renderer _rend;

        [SerializeField]
        [Tooltip("HandController that should trigger this behaviour. If empty, this behaviour will trigger for both hands.")]
        private KinematicGrabber grabbingHand;

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


