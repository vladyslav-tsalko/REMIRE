using System.Collections;
using LearnXR.Core.Utilities;
using UnityEngine;

namespace LiquidPhysics
{
    [RequireComponent(typeof(LineRenderer))]
    public class StreamBehaviour : MonoBehaviour
    {
        private static readonly float StreamVelocity = 1.0f;
        /// <summary>
        /// Basic line renderer with 2 access points. Point 0 at pouring origin and point 1 at pouring destination.
        /// </summary>
        private LineRenderer _lineRenderer;

        /// <summary>
        /// Splash particles displayed at the end of line renderer.
        /// </summary>
        private ParticleSystem _splashParticles;

        private Coroutine _pourRoutine;
        
        /// <summary>
        /// Position at which stream should hit the ground.
        /// </summary>
        private Vector3 _targetPosition = Vector3.zero;
        
        /// <summary>
        /// Used to determine the stream end position at current liquid level. Null if currently not colliding with container.
        /// </summary>
        private Container _fillableContainer;

        /// <summary>
        /// Layers to ignore by new line renderers
        /// </summary>
        private LayerMask _ignoreLayers;
        
        public float flowVelocity = 0.02f;
        
        public void End()
        {
            if(_pourRoutine != null) 
                StopCoroutine(_pourRoutine);

            _pourRoutine = StartCoroutine(EndPourRoutine());
        }
        
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _splashParticles = GetComponentInChildren<ParticleSystem>();

            _ignoreLayers = ~((1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Grabbable") | 1 << LayerMask.NameToLayer("Water")));
        }

        private void Start()
        {
            // initiate stream begin at origin (position of component stream is attached to)
            MoveToPosition(0, transform.position);

            // initiate the end of the stream to the same point as the beginning at start
            MoveToPosition(1, transform.position);

            StartCoroutine(UpdateParticles());
            _pourRoutine = StartCoroutine(PourRoutine());
        }

        private void Update()
        {
            // If raycast hits anything, check if line renderer collides with a container and try to pour in if so.
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 2.0f, _ignoreLayers))
            {
                Container container = hit.collider.gameObject.GetComponent<Container>();
                //SpatialLogger.Instance.LogInfo($"Hit collider: {hit.collider}, container: {container}");
                if (container != null)
                {
                    container.PourIn(flowVelocity);
                    _fillableContainer = container;
                }
                else
                {
                    _fillableContainer = null;
                }
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private Vector3 FindEndPoint()
        {
            // create a ray from current position directly downwards
            Ray ray = new Ray(transform.position, Vector3.down);

            // visualize the ray with default length if not stopped by any object. 'Hit' will detect the nearest colliding object
            Physics.Raycast(ray, out RaycastHit hit, 2.0f, _ignoreLayers);

            // if raycasthit hits a container set stream end at liquid level, otherwise if any collider set the end of the stream to the collision point else use default stream length

            if (_fillableContainer && hit.collider)
            {
                return new Vector3(hit.point.x, _fillableContainer.LiquidHeight, hit.point.z);
            }
            if (_fillableContainer)
            {
                // sets stream direction in the middle of the container if no collider found
                Vector3 containerPos = _fillableContainer.transform.position;
                return new Vector3(containerPos.x, _fillableContainer.LiquidHeight, containerPos.z);
            }
            return hit.collider ? hit.point : ray.GetPoint(2.0f);
        }

        /// <summary>
        /// Instantly sets the line renderer's point at the specified index to the given position.
        /// </summary>
        /// <param name="index">The index of the point in the line renderer to move.</param>
        /// <param name="targetPosition">The target world-space position to move to.</param>
        private void MoveToPosition(int index, Vector3 targetPosition)
        {
            _lineRenderer.SetPosition(index, targetPosition);
        }

        /// <summary>
        /// Smoothly animates the line renderer's point at the specified index toward the given position.
        /// </summary>
        /// <param name="index">The index of the point in the line renderer to animate.</param>
        /// <param name="targetPosition">The target world-space position to animate toward.</param>
        private void AnimateToPosition(int index, Vector3 targetPosition)
        {
            Vector3 currentPosition = _lineRenderer.GetPosition(index);
            Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * StreamVelocity);

            _lineRenderer.SetPosition(index, newPosition);
        }

        /// <summary>
        /// manages start and end points of the stream while pouring
        /// </summary>
        private IEnumerator PourRoutine()
        {
            // continue routine while the object is active
            while (gameObject.activeSelf)
            {
                // find where stream should hit the ground
                _targetPosition = FindEndPoint();

                // let stream beginning stay at the origin
                MoveToPosition(0, transform.position);

                // gradually move the stream ending towards target position on the ground
                AnimateToPosition(1, _targetPosition);

                yield return null;
            }
        }

        /// <summary>
        /// Animate the stream start and end points until it hits the target before destroying it
        /// </summary>
        private IEnumerator EndPourRoutine()
        {
            // continue animating the stream until the origin meets destination
            while (_lineRenderer.GetPosition(0) != _targetPosition)
            {
                AnimateToPosition(0, _targetPosition);
                AnimateToPosition(1, _targetPosition);
                yield return null;
            }

            // destroy the strem instance once whole stream reached target
            Destroy(gameObject);
        }

        /// <summary>
        /// manage position of splash particles
        /// </summary>
        private IEnumerator UpdateParticles()
        {
            while (gameObject.activeSelf)
            {
                // position the splash at target stream end
                _splashParticles.gameObject.transform.position = _targetPosition;

                // show splash particles only once the first stream point reaches the position
                bool hasReachedTarget = _lineRenderer.GetPosition(1) == _targetPosition;

                _splashParticles.gameObject.SetActive(hasReachedTarget);

                yield return null;
            }
        }
    }
}
