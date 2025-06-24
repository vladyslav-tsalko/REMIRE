using System.Collections;
using System.Collections.Generic;
using LearnXR.Core.Utilities;
using Managers;
using Meta.XR.MRUtilityKit;
using Tasks;
using UnityEngine;

namespace Utilities
{
    public class Table
    {
        /// <summary>
        /// Perpendicular to the length
        /// </summary>
        private Vector3 _forward;
        
        public Vector3 Forward => _forward;
        
        /// <summary>
        /// Perpendicular to the width
        /// </summary>
        private Vector3 _right;

        public Vector3 Right => _right;

        private Vector3 _up = Vector3.up;
        
        /// <summary>
        /// Half sizes. Width is always less than length. x - length, y - height, z- width
        /// </summary>
        private Vector3 _extents;

        public Vector3 Extents => _extents;

        public float HalfLength => _extents.x;
        public float HalfWidth => _extents.z;
        public float HalfHeight => _extents.y;
        
        public float Length => _extents.x * 2;
        public float Width => _extents.z * 2;
        public float Height => _extents.y * 2;
        
        /// <summary>
        /// Global center
        /// </summary>
        private Vector3 _center;
        
        public Vector3 TopCenter => _center + Vector3.up * HalfHeight;

        //public bool IsInit { get; private set; }

        public bool UpdateVectors()
        {
            if (Vector3.Dot(Camera.main.transform.forward.normalized, _forward.normalized) < 0f)
            {
                _forward = -_forward;
                _right = -_right;
                return true;
            }
            return false;
        }

        public Quaternion DesiredRotation()
        {
            UpdateVectors();
            return Quaternion.LookRotation(_forward, Vector3.up);
        }

        public Table(MRUKAnchor tableAnchor)
        {
            if (!tableAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE)) return;
            if (!tableAnchor.VolumeBounds.HasValue) return;
            
            Vector3 tableBounds = tableAnchor.VolumeBounds.Value.extents;
            
            _center = tableAnchor.GetAnchorCenter();
            bool isXLonger = tableBounds.x > tableBounds.y;
            
            _extents.x = isXLonger ? tableBounds.x : tableBounds.y; // Length
            _extents.z = isXLonger ? tableBounds.y : tableBounds.x; // Width
            _extents.y = tableBounds.z; // Height
            
            Quaternion rotation = Quaternion.Euler(0, tableAnchor.transform.rotation.eulerAngles.y, 0);

            // Set _forward (perpendicular to length, along local z-axis)
            _forward = rotation * (isXLonger ? Vector3.forward : Vector3.right).normalized;

            // Set _left (perpendicular to width, along local x-axis)
            _right = rotation * (isXLonger ? Vector3.right : Vector3.forward).normalized;

            UpdateVectors();
            //IsInit = true;
        }

        public bool IsPositionOnTable(Vector3 worldPosition, float distanceBufferXZ = 0.0f, float distanceBufferY = 0.05f) //TODO: make for other checks
        {
            //if (!IsInit) return false;

            Vector3 toPoint = worldPosition - _center;

            float x = Vector3.Dot(toPoint, _right);    // Width axis
            float y = Vector3.Dot(toPoint, _up);       // Height axis
            float z = Vector3.Dot(toPoint, _forward);  // Length axis
            
            float halfLength = HalfLength + distanceBufferXZ;
            float halfWidth = HalfWidth + distanceBufferXZ;
            
            return Mathf.Abs(x) <= halfLength && Mathf.Abs(z) <= halfWidth && Mathf.Abs(y - HalfHeight) <= distanceBufferY;
        }


        /// <param name="offset">Offset in meters to the right</param>
        /// <returns>Position on the table based on the offset</returns>
        public Vector3 GetWorldSpawnPosition(float offset)
        {
            UpdateVectors();
            return TopCenter + offset * _right;
        }
        
        ///<summary>Returns world position on the </summary>
        /// <param name="go">Game object</param>
        /// <param name="offset">Offset in meters to the right</param>
        /// <returns></returns>
        public Vector3 GetObjectWorldSpawnPosition(GameObject go, float offset = 0f)
        {
            UpdateVectors();
            Vector3 goExtents = go.GetComponent<Renderer>().bounds.extents;
            return TopCenter + Vector3.up * goExtents.y + offset * _right;
        }
        
        public GameObject SpawnPrefab(GameObject prefab, ESpawnLocation spawnLocation, Difficulty taskDifficulty)
        {
            return SpawnPrefab(prefab, spawnLocation, taskDifficulty, Vector3.zero);
        }
        
        public GameObject SpawnPrefab(GameObject prefab, ESpawnLocation spawnLocation, Difficulty taskDifficulty, bool shouldTransformY)
        {
            return SpawnPrefab(prefab, spawnLocation, taskDifficulty, Vector3.zero, shouldTransformY);
        }

        public GameObject SpawnPrefab(GameObject prefab, ESpawnLocation spawnLocation, Difficulty taskDifficulty, Vector3 offset, bool shouldTransformY = true)
        {
            var handPositions = ReachAreaManager.Instance.GetHandPositions();
            var targetPos = handPositions.GetSpawnPosition(taskDifficulty, spawnLocation);
            targetPos.y = TopCenter.y; //interesting only xz
            if (shouldTransformY)
            {
                float prefabExtentsY = prefab.GetComponent<Renderer>().bounds.extents.y;
                targetPos.y += prefabExtentsY;
            }

            targetPos += offset;
            Quaternion targetRot = DesiredRotation() * prefab.transform.rotation;
            GameObject spawnedObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            targetPos = GetCorrectedPositionToKeepObjectOnTable(spawnedObject.GetComponent<Collider>().bounds, Vector3.zero, targetPos, targetRot);
            TableManager.Instance.StartCoroutine(DelayedSafePlace(spawnedObject, targetPos, targetRot));
            /*GameObject spawnedObject = Object.Instantiate(prefab,  spawnPos, spawnRot);
            Vector3 correction = GetCorrectionToKeepObjectOnTable(spawnedObject);
            spawnedObject.transform.position += correction;*/
            
            return spawnedObject;
            /*float prefabExtentsY = prefab.GetComponent<Renderer>().bounds.extents.y;
            return Object.Instantiate(prefab, TopCenter + Vector3.up*prefabExtentsY + offset, DesiredRotation() * prefab.transform.rotation);
            */

        }
        
        private IEnumerator DelayedSafePlace(GameObject obj, Vector3 targetPosition, Quaternion targetRotation)
        {
            Transform objTransform = obj.transform;
            if (!obj.TryGetComponent(out Rigidbody rb))
            {
                objTransform.SetPositionAndRotation(targetPosition, targetRotation);
                yield break;
            }
            
            //Collider objCollider = obj.GetComponentInChildren<Collider>();
            if (!obj.TryGetComponent(out Collider collider))
            {
                Debug.LogWarning("No collider found on spawned object!");
                objTransform.SetPositionAndRotation(targetPosition, targetRotation);
                yield break;
            }
            
            int handLayer = LayerMask.NameToLayer("Hands");
            if (handLayer == -1)
            {
                Debug.LogError("Layer 'Hands' not found. Please define it in the Unity editor.");
                objTransform.SetPositionAndRotation(targetPosition, targetRotation);
                yield break;
            }
            
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            

            rb.isKinematic = true;
            
            Bounds objBounds = collider.bounds;
            Vector3 center = targetPosition + (objBounds.center - objTransform.position);
            
            int handLayerMask = 1 << handLayer;
            const int maxHits = 25;
            Collider[] hitBuffer = new Collider[maxHits];
            int consecutiveClearFrames = 0;
            const int requiredClearFrames = 3;
            // Wait until no hand collides with target placement
            while (consecutiveClearFrames < requiredClearFrames)
            {
                int hitCount = Physics.OverlapBoxNonAlloc(
                    center,
                    objBounds.extents,
                    hitBuffer,
                    targetRotation,
                    handLayerMask);
                if (hitCount == 0)
                {
                    consecutiveClearFrames++;
                }
                else
                {
                    consecutiveClearFrames = 0;
                }

                yield return new WaitForSeconds(0.05f);
            }
            
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                renderer.enabled = true;

            rb.isKinematic = false;

            // Safe to place
            objTransform.SetPositionAndRotation(targetPosition, targetRotation);
        }
        
        private Vector3 GetCorrectedPositionToKeepObjectOnTable(Bounds bounds, Vector3 spawnPos, Vector3 targetPosition, Quaternion targetRotation)
        {
            // Get all 4 bottom corners in world space (XZ plane)
            Vector3[] corners = new Vector3[4];
            Vector3 extents = bounds.extents;
            Vector3 center = bounds.center;
            
            Vector3[] localCorners = new Vector3[4];
            localCorners[0] = new Vector3(-extents.x, -extents.y, -extents.z);
            localCorners[1] = new Vector3(-extents.x, -extents.y, extents.z);
            localCorners[2] = new Vector3(extents.x, -extents.y, -extents.z);
            localCorners[3] = new Vector3(extents.x, -extents.y, extents.z);
            
            for (int i = 0; i < 4; i++)
            {
                Vector3 rotated = targetRotation * localCorners[i];
                corners[i] = rotated + targetPosition + (center - spawnPos); // offset due to bounds.center
                corners[i].y = TopCenter.y;
            }


            float minLength = float.MaxValue;
            float maxLength = float.MinValue;
            float minWidth = float.MaxValue;
            float maxWidth = float.MinValue;

            foreach (var corner in corners)
            {
                Vector3 toCorner = corner - _center;

                float alongLength = Vector3.Dot(toCorner, _right);   // Length direction
                float alongWidth  = Vector3.Dot(toCorner, _forward); // Width direction

                minLength = Mathf.Min(minLength, alongLength);
                maxLength = Mathf.Max(maxLength, alongLength);
                minWidth = Mathf.Min(minWidth, alongWidth);
                maxWidth = Mathf.Max(maxWidth, alongWidth);
            }

            Vector3 correction = Vector3.zero;

            if (minLength < -HalfLength)
                correction += _right * (-HalfLength - minLength);
            if (maxLength > HalfLength)
                correction += _right * (HalfLength - maxLength);
            if (minWidth < -HalfWidth)
                correction += _forward * (-HalfWidth - minWidth);
            if (maxWidth > HalfWidth)
                correction += _forward * (HalfWidth - maxWidth);
            SpatialLogger.Instance.LogInfo($"correction {correction}");
            return correction + targetPosition;
        }
    }
}