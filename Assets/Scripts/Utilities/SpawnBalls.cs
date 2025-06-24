using System;
using UnityEngine;
using UnityEngine.Serialization;
using LearnXR.Core.Utilities;

public class SpawnBalls : MonoBehaviour
{
    
    [SerializeField] private GameObject spawnedPrefab;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    
    private Transform leftHand;
    private Transform rightHand;

    [SerializeField] private float spawnSpeed = 20;
    
    void Start()
    {
        GameObject trackingSpace = GameObject.Find("[BuildingBlock] Camera Rig")?.transform.Find("TrackingSpace")?.gameObject;

        if (trackingSpace != null)
        {
            leftHand = trackingSpace.transform.Find("LeftHandAnchor");
            rightHand = trackingSpace.transform.Find("RightHandAnchor");
        }

        if (leftHand == null || rightHand == null)
        {
            SpatialLogger.Instance.LogError("One or both hand anchors not found! Check hierarchy or naming.");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && rightHand)
        {
            Shoot(rightHand, redMaterial);
        }
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && leftHand)
        {
            Shoot(leftHand, greenMaterial);
        }
    }

    void Shoot(Transform handTransform, Material material)
    {
        GameObject spawnedBall = Instantiate(spawnedPrefab, handTransform.position, Quaternion.identity);
        spawnedBall.GetComponent<Rigidbody>().linearVelocity = handTransform.forward * spawnSpeed;
        spawnedBall.GetComponent<MeshRenderer>().material = material;
    }

}
