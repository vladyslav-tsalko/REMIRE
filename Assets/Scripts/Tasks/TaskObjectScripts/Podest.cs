using System;
using Hands.Grabbables;
using Tasks;
using UnityEngine;


namespace Tasks.TaskObjectScripts
{
    /// <summary>
    /// Attached to all podests, responsible for detecting and triggering events when a grabbable object is placed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Renderer))]
    public class Podest : MonoBehaviour
    {
        [Tooltip("Material when grabbable is outside.")]
        [SerializeField] private Material onEmptyMaterial;

        [Tooltip("Material when grabbable intersects but isn't correctly placed.")]
        [SerializeField] private Material onIntersectMaterial;
        
        [Tooltip("Material when grabbable positioned correct.")]
        [SerializeField] private Material onCorrectMaterial;

        [Tooltip("Set value for level. Choose None if not for stairs")] 
        [SerializeField] private EPodestLevel level = EPodestLevel.None;
        
        public Action<EPodestLevel> onCorrectTriggered;
        
        public bool IsGreen => _rend.sharedMaterial == onCorrectMaterial;
        
        public bool IsBlue => _rend.sharedMaterial == onIntersectMaterial;
        
        public bool IsRed => _rend.sharedMaterial == onEmptyMaterial;
        
        private Renderer _rend;
        private Collider _levelCollider;
        private Collider _currentCollider;
        
       

        private void Awake()
        {
            _levelCollider = GetComponent<Collider>();
            _rend = GetComponent<Renderer>();
        }
        

        private void OnTriggerStay(Collider newCollider)
        {
            if ((_currentCollider && _currentCollider != newCollider) || !newCollider.gameObject.GetComponent<KinematicGrabbable>()) return;
            
            if(!_currentCollider) _currentCollider = newCollider; 

            if (CheckIfGrabbableInBounds(newCollider))
            {
                //Should not be triggered if is not used for sequence detection
                if (_rend.sharedMaterial != onCorrectMaterial && level != 0)
                {
                    onCorrectTriggered?.Invoke(level);
                }
                ChangeMaterial(onCorrectMaterial);
            }
            else
            {
                ChangeMaterial(onIntersectMaterial);
            } 
        }

        private void OnTriggerExit(Collider oldCollider)
        {
            if(oldCollider == _currentCollider)
            {
                _currentCollider = null;
                ChangeMaterial(onEmptyMaterial);
            }
        }
        
        private bool CheckIfGrabbableInBounds(Collider collider)
        {
            Bounds podestBounds = _levelCollider.bounds;
            Bounds objectBounds = collider.bounds;

            return
                objectBounds.min.x >= podestBounds.min.x &&
                objectBounds.max.x <= podestBounds.max.x &&
                objectBounds.min.z >= podestBounds.min.z &&
                objectBounds.max.z <= podestBounds.max.z;
            
        }

        private void ChangeMaterial(Material newMaterial)
        {
            if(_rend.material != newMaterial) _rend.material = newMaterial;
        }
    }
}





