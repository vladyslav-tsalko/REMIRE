using System;
using LearnXR.Core.Utilities;
using Managers;
using UnityEngine;


namespace LiquidPhysics
{
    [Serializable]
    public class LiquidBehaviour
    {
        [Range(10, 200)]
        [Tooltip("Liquid flow rate per seconds in ml")]
        [SerializeField] private float flowVelocity = 0.05f;
        
        [Tooltip("Game object with a Collider and MeshRenderer containing the liquid shader.")]
        [SerializeField] private GameObject liquid;

        [Tooltip("Transform of pour origin is used to define liquid stream spawn positions")]
        [SerializeField] private GameObject pourOrigin;

        /// <summary>
        /// Current liquid level in world space along the y-axis.
        /// </summary>
        public float LiquidHeight { get; private set; }

        /// <summary>
        /// Reference to the container this liquid behaviour is attached to.
        /// </summary>
        private Container _container;

        /// <summary>
        /// MeshCollider used to determine bounds for the liquid height calculation.
        /// </summary>
        private MeshCollider _liquidCollider;

        /// <summary>
        /// Renderer attached to the pour origin GameObject, used to check whether the liquid level is above the pour origin.
        /// </summary>
        private Renderer _pourOriginRenderer;

        /// <summary>
        /// Material with a shader that utilizes the '_LiquidHeight' property (y-axis in world space) 
        /// to dynamically visualize the liquid level inside the container.
        /// </summary>
        private Material _liquidMaterial;

        /// <summary>
        /// Current stream of liquid being poured from the container.
        /// </summary>
        private StreamBehaviour _currentStream;

        #region Getters
        
        public bool IsPouring => _currentStream;
        
        public float FlowVelocity => flowVelocity;
        
        public Vector3 PourOriginPos => pourOrigin.transform.position;
        
        private bool LiquidAbovePourOrigin => pourOrigin && LiquidHeight > _pourOriginRenderer.bounds.min.y;
        
        #endregion
        
        public void Init(Container container)
        {
            _container = container;
            _pourOriginRenderer = pourOrigin.GetComponent<MeshRenderer>();
            
            flowVelocity = flowVelocity * (_container.MaxCapacity - _container.MinCapacity) + _container.MinCapacity;

            _liquidCollider = liquid.GetComponent<MeshCollider>();
            _liquidMaterial = liquid.GetComponent<Renderer>().material;
        }
        
        public void Update()
        {
            Bounds bounds = _liquidCollider.bounds;
            LiquidHeight = bounds.min.y + (bounds.max.y - bounds.min.y) * _container.Filled;
            _liquidMaterial.SetFloat("_LiquidHeight", LiquidHeight); 

            if (LiquidAbovePourOrigin && _container.TryPourOut())
            {
                CreateStream();
            }
            else
            {
                DestroyStream();
            }
        }

        
        private void DestroyStream()
        {
            if (!_currentStream) return;
            
            _currentStream.End();
            _currentStream = null;
        }
        
        private void CreateStream()
        {
            if (_currentStream) return;
            
            Vector3 spawnPos = GetLowestPourPoint();
            GameObject streamObject = UnityEngine.Object.Instantiate(TaskObjectPrefabsManager.Instance.LiquidStreamPrefab, spawnPos, Quaternion.identity, pourOrigin.transform);

            _currentStream = streamObject.GetComponent<StreamBehaviour>();
            _currentStream.flowVelocity = flowVelocity;
        }
        
        /// <summary>
        /// Calculates the lowest point around the pour origin for spawning the liquid stream position.
        /// </summary>
        /// <param name="resolution">Number of points sampled around the pour origin's circumference
        /// to determine the lowest point. Higher values increase precision.</param>
        /// <returns>The world-space position of the lowest point around the pour origin
        /// where the liquid stream should spawn.</returns>
        private Vector3 GetLowestPourPoint(int resolution = 16)
        {
            float radius = pourOrigin.transform.lossyScale.x * 0.5f;

            Vector3 lowestPoint = Vector3.zero;
            float lowestY = float.MaxValue;

            Transform t = pourOrigin.transform;
            Vector3 center = t.position;
            Vector3 right = t.right;
            Vector3 forward = t.forward;

            for (int i = 0; i < resolution; i++)
            {
                float angle = i * Mathf.PI * 2 / resolution;
                Vector3 offset = Mathf.Cos(angle) * right + Mathf.Sin(angle) * forward;
                Vector3 worldPoint = center + offset * radius;

                if (worldPoint.y < lowestY)
                {
                    lowestY = worldPoint.y;
                    lowestPoint = worldPoint;
                }
            }

            return lowestPoint;
        }
    }
}
