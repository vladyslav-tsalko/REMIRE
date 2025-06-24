using System;
using LearnXR.Core.Utilities;
using Managers;
using UnityEngine;

// Comment-out to see the liquid physics behaviour in edit mode.
// May misfunction if this script is dependant on other scripts that don't run in edit mode!
//[ExecuteInEditMode]

[Serializable]
public class LiquidBehaviour
{
    [Tooltip("If set to true, the liquid can pour out of the container when tilted.")]
    public bool pourable = true;

    [Range(0, 1)]
    [Tooltip("Liquid flow rate per frame in fractions.")]
    public float flowVelocity = 0.05f;

    [SerializeField]
    [Tooltip("Game object with a Collider and MeshRenderer containing the liquid shader.")]
    private GameObject liquid;

    [Tooltip("Transform of pour origin is used to define if liquid should be poring out of the container. It should be a child of the container GameObject and cover the container opening e.g. bottle mouth.")]
    public GameObject pourOrigin;
    
    [Tooltip("Current level of liquid in worldspace (on y-axis).")]
    private float _liquidHeight = 0.0f;

    public float LiquidHeight => _liquidHeight;

    [Tooltip("Container this behaviour is attached to.")]
    private Container _container;

    [Tooltip("Collider attached to liquid game object which will determine mesh bounds for liquid height.")]
    private MeshCollider liquidCollider = null;

    private Renderer pourOriginRenderer;

    [Tooltip("Material depending on _LiquidHeight shader property on y-axis in worldspace which will be dynamically set to visualize container fullness.")]
    private Material liquidMaterial;

    [Tooltip("Stream behaviour contained in streamPrefab. When enabled, stream behaviour will render a line from pour origin to destination until end method called. Self-destroys once origin of line reached destination.")]
    private StreamBehaviour currentStream;

    /*[Tooltip("Find offset for lower edge. Position returns coords of object center)")]
    private float pourOriginOffset = 0.0f;*/

    // Calculate when liquid in tilted bottle reaches the bottle mouth to start pouring
    /*public bool LiquidAbovePourOrigin => pourOrigin != null &&
            liquidHeight > pourOrigin.transform.position.y - pourOriginOffset;*/

    public void Init(Container container)
    {
        _container = container;
        pourOriginRenderer = pourOrigin.GetComponent<MeshRenderer>();
        
        flowVelocity = flowVelocity * (_container.MaxCapacity - _container.MinCapacity) + _container.MinCapacity;

        liquidCollider = liquid.GetComponent<MeshCollider>();
        liquidMaterial = liquid.GetComponent<Renderer>().material;
    }

    public void Update()
    {
        // Keep track of the top of the liquid surface in world space.
        // Height is calculated from objects most-down position in world space plus the amount
        // of liquid filled in proportion to objects bounds volume.
        Bounds bounds = liquidCollider.bounds;
        _liquidHeight = bounds.min.y + (bounds.max.y - bounds.min.y) * _container.Filled;
        liquidMaterial.SetFloat("_LiquidHeight", _liquidHeight); // Update liquid height in material component which renders the correct liquid volume.

        if (pourable && LiquidAbovePourOrigin && _container.TryPourOut())
        {
            CreateStream();
        }
        else
        {
            DestroyStream();
        }
    }
    
    public bool IsPouring()
    {
        return currentStream;
    }

    private void DestroyStream()
    {
        if (!currentStream) return;
        
        currentStream.End();
        currentStream = null;
    }
    
    private void CreateStream()
    {
        if (currentStream) return;
        
        Vector3 spawnPos = GetLowestPourPoint();
        GameObject streamObject = UnityEngine.Object.Instantiate(TaskObjectPrefabsManager.Instance.LiquidStreamPrefab, spawnPos, Quaternion.identity, pourOrigin.transform);

        currentStream = streamObject.GetComponent<StreamBehaviour>();
        currentStream.flowVelocity = flowVelocity;
    }
    
    private bool LiquidAbovePourOrigin => pourOrigin && _liquidHeight > pourOriginRenderer.bounds.min.y;
    
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