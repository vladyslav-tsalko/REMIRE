using System;
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using Tasks.TaskProperties;

namespace Managers
{
    /// <summary>
    /// Manages table detection, validation, visual highlighting, selection, and persistence.
    /// </summary>
    public class TableManager : Singleton<TableManager>
    {
        private const float MinTableSideOffset = 0.2f;
        
        public event Action OnTableSelected;
        public event Action OnStartTableSelection;
        
        [SerializeField] private EffectMesh collidersEffectMesh;
        [SerializeField] private EffectMesh invalidTablesEffectMesh;
        [SerializeField] private EffectMesh validTablesEffectMesh;
        [SerializeField] private EffectMesh selectedTableEffectMesh;
        
        [SerializeField] private GameObject tableLabelPrefab;
        
        private readonly Dictionary<RayInteractable, Action<PointerEvent>> _tableHoveringHandlers = new();
        
        private MRUKAnchor _selectedTableAnchor;
        
        public bool IsInit => _selectedTableAnchor;
        
        public Table SelectedTable { get; private set; }
        
        
        /// <summary>
        /// Marks all tables with surface dimension labels and highlights validity.  
        /// Enables interaction for valid tables and starts the table selection process.
        /// </summary>
        public void StartTableSelection()
        {
            OnStartTableSelection?.Invoke();
            if (_selectedTableAnchor)
            {
                validTablesEffectMesh.CreateEffectMesh(_selectedTableAnchor);
                selectedTableEffectMesh.DestroyMesh(_selectedTableAnchor);
                _selectedTableAnchor = null;
            }
            else
            {
                ValidateTables();
            }
            SpatialLogger.Instance.LogInfo($"{validTablesEffectMesh.EffectMeshObjects.Count} valid tables");
            
            foreach (var validTableAnchor in validTablesEffectMesh.EffectMeshObjects.Keys)
            {
                SpawnLabelForTable(validTableAnchor);
                //Add possibility to point on this anchor.
                if (collidersEffectMesh.EffectMeshObjects.TryGetValue(validTableAnchor, out EffectMesh.EffectMeshObject effectMeshObj))
                {
                    var interactable = AddRayInteractionComponents(validTableAnchor, effectMeshObj);
                                
                    Action<PointerEvent> tableHoveringHandler = (PointerEvent evt) => { HandleTableHovering(evt, validTableAnchor); };
                                
                    interactable.WhenPointerEventRaised += tableHoveringHandler;
                    _tableHoveringHandlers[interactable] = (tableHoveringHandler);
                }
            }

            foreach (var invalidTableAnchor in invalidTablesEffectMesh.EffectMeshObjects.Keys)
            {
                SpawnLabelForTable(invalidTableAnchor);
            }

            validTablesEffectMesh.HideMesh = false;
            invalidTablesEffectMesh.HideMesh = false;
            selectedTableEffectMesh.HideMesh = false;
        }
        
        private static RayInteractable AddRayInteractionComponents(MRUKAnchor anchor, EffectMesh.EffectMeshObject effectMeshObj)
        {
            var colliderSurface = anchor.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(effectMeshObj.collider);
                                
            var interactable = anchor.AddComponent<RayInteractable>();
            interactable.InjectSurface(colliderSurface);

            return interactable;
        }

        private static void RemoveRayInteractionComponents(MRUKAnchor anchor)
        {
            if (anchor.TryGetComponent<RayInteractable>(out var interactable))
            {
                Destroy(interactable);
            }
            if (anchor.TryGetComponent<ColliderSurface>(out var colliderSurface))
            {
                Destroy(colliderSurface);
            }
        }
        
        private static void ClearTableLabels()
        {
            GameObject[] labels = GameObject.FindGameObjectsWithTag("TableLabel");
            foreach (GameObject label in labels)
            {
                Destroy(label);
            }
        }

        private void SpawnLabelForTable(MRUKAnchor tableAnchor)
        {
            if (!tableAnchor.VolumeBounds.HasValue) return;
            
            Vector3 tableBoundsExtents = tableAnchor.VolumeBounds.Value.extents;
            float spawnedObjectHeight =
                tableLabelPrefab.GetComponent<RectTransform>().rect.height * tableLabelPrefab.transform.lossyScale.y;
                
            Vector3 spawnPosition = tableAnchor.GetAnchorCenter() + (Vector3.up * (tableBoundsExtents.z + spawnedObjectHeight + 0.05f));

            GameObject label = Instantiate(tableLabelPrefab, spawnPosition, Quaternion.identity);

            // Update text
            TextMeshProUGUI text = label.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null) return;
            
            int x = Mathf.RoundToInt(tableBoundsExtents.x * 2 * 100);
            int y = Mathf.RoundToInt(tableBoundsExtents.y * 2 * 100);
                
            text.text = x > y ? $"{x} x {y} cm": $"{y} x {x} cm";
        }
        
        /// <summary>
        /// Handles pointer interactions with tables during the selection phase.
        /// Adds or removes selection highlighting based on pointer event type (hover, unhover),
        /// and finalizes the table selection when a valid table is selected.
        /// </summary>
        /// <param name="pointerEvent">The pointer event data (hover, unhover, or select).</param>
        /// <param name="table">The table (MRUKAnchor) that triggered the pointer event.</param>
        private void HandleTableHovering(PointerEvent pointerEvent, MRUKAnchor table)
        {
            switch (pointerEvent.Type)
            {
                case PointerEventType.Hover:
                    //a table can trigger hover event only it's the first table that is being hovered
                    if (selectedTableEffectMesh.EffectMeshObjects.Count == 0) 
                    {
                        selectedTableEffectMesh.CreateEffectMesh(table);
                        validTablesEffectMesh.DestroyMesh(table);
                    }
                    break;

                case PointerEventType.Unhover:
                    //can be triggered only with already selected table
                    if (selectedTableEffectMesh.EffectMeshObjects.Keys.Contains(table)) 
                    {
                        selectedTableEffectMesh.DestroyMesh(table);
                        validTablesEffectMesh.CreateEffectMesh(table);
                    }

                    break;

                case PointerEventType.Select:
                    //select event can be happened only with the selected table
                    if (selectedTableEffectMesh.EffectMeshObjects.Keys.Contains(table)) 
                    {
                        foreach (var (interactable, action) in _tableHoveringHandlers)
                        {
                            if(interactable) {
                                interactable.WhenPointerEventRaised -= action;
                            }
                        }
                        _tableHoveringHandlers.Clear();
                        
                        foreach (var tableAnchor in validTablesEffectMesh.EffectMeshObjects.Keys)
                        {
                            RemoveRayInteractionComponents(tableAnchor);
                        }
                        RemoveRayInteractionComponents(table);
                        _selectedTableAnchor = table;
                        
                        validTablesEffectMesh.HideMesh = true;
                        invalidTablesEffectMesh.HideMesh = true;
                        
                        ClearTableLabels();
                        SelectedTable = new Table(_selectedTableAnchor);
                        OnTableSelected?.Invoke();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Validates tables based on their surface dimensions relative to the biggest prefab
        /// </summary>
        private void ValidateTables()
        {
            Vector2 stairsSizes = TaskObjectPrefabsManager.Instance.GetStairsSizes();
            if (stairsSizes.Equals(Vector2.zero))
            {
                Debug.LogError("StairsPrefab is not init");
                return;
            }
            SpatialLogger.Instance.LogInfo($"Started validation with {MRUK.Instance.GetCurrentRoom().Anchors.Count} anchors");
            foreach (MRUKAnchor anchor in MRUK.Instance.GetCurrentRoom().Anchors)
            {
                if (!anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE)) continue;
                selectedTableEffectMesh.DestroyMesh(anchor);

                if (!anchor.VolumeBounds.HasValue) continue;
                var anchorExtents = anchor.VolumeBounds.Value.extents;
                bool isXLarger = anchorExtents.x > anchorExtents.y;
                float width = (isXLarger ? anchorExtents.y : anchorExtents.x) * 2;
                float length = (isXLarger ? anchorExtents.x : anchorExtents.y) * 2;
                
                //if table invalid - remove from valid, else - remove from invalid
                if (length - stairsSizes.x < MinTableSideOffset || width - stairsSizes.y < MinTableSideOffset)
                {
                    SpatialLogger.Instance.LogInfo("Invalid");
                    validTablesEffectMesh.DestroyMesh(anchor);
                }
                else
                {
                    SpatialLogger.Instance.LogInfo("Valid");
                    invalidTablesEffectMesh.DestroyMesh(anchor);
                }
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            OVRScene.RequestSpaceSetup();
        }
        
        private void OnDisable()
        {
            foreach (var (interactable, action) in _tableHoveringHandlers)
            {
                if(interactable) {
                    interactable.WhenPointerEventRaised -= action;
                }
            }
        }
        
        /// <summary>
        /// Represents a real-world table detected via MRUKAnchor, with utility functions for object placement.
        /// Calculates orientation, dimensions, and spawn positions for virtual objects on the table surface.
        /// Also ensures that objects are safely placed avoiding collisions (e.g., with user hands).
        /// </summary>
        public class Table
        {
            /// <summary>
            /// Indicates where an object should be spawned on the table relative to handedness.
            /// </summary>
            public enum ESpawnLocation
            {
                /// <summary>
                /// Non-dominant hand (Left for right-handed, Right for left-handed)
                /// </summary>
                Secondary,
                
                /// <summary>
                /// Centered for use with both hands.
                /// </summary>
                Middle,
                
                /// <summary>
                ///  Dominant hand (Right for right-handed, Left for left-handed).
                /// </summary>
                Primary
            }
            /// <summary>
            /// Perpendicular to the length
            /// </summary>
            private Vector3 _forward;
            
            /// <summary>
            /// Perpendicular to the width
            /// </summary>
            private Vector3 _right;
            
            private Vector3 _up = Vector3.up;
            
            /// <summary>
            /// Half sizes. Width is always less than length. x - length, y - height, z- width
            /// </summary>
            private Vector3 _extents;

            private float HalfLength => _extents.x;
            private float HalfWidth => _extents.z;
            private float HalfHeight => _extents.y;
            
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

            private Quaternion DesiredRotation()
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
            }

            /// <summary>
            /// Checks if a given world position lies on the surface of the table,
            /// optionally expanding the bounds with a buffer in XZ and Y directions.
            /// </summary>
            /// <param name="worldPosition">The world position to test.</param>
            /// <param name="distanceBufferXZ">Optional buffer for X and Z axes.</param>
            /// <param name="distanceBufferY">Optional buffer for Y axis (height).</param>
            /// <returns>True if the position lies within the table's bounds.</returns>
            public bool IsPositionOnTable(Vector3 worldPosition, float distanceBufferXZ = 0.0f, float distanceBufferY = 0.05f)
            {
                Vector3 toPoint = worldPosition - _center;

                float x = Vector3.Dot(toPoint, _right);    // Width axis
                float y = Vector3.Dot(toPoint, _up);       // Height axis
                float z = Vector3.Dot(toPoint, _forward);  // Length axis
                
                float halfLength = HalfLength + distanceBufferXZ;
                float halfWidth = HalfWidth + distanceBufferXZ;
                
                return Mathf.Abs(x) <= halfLength && Mathf.Abs(z) <= halfWidth && Mathf.Abs(y - HalfHeight) <= distanceBufferY;
            }
            
            /// <summary>
            /// Spawns a prefab at a target location on the table based on difficulty and spawn zone,
            /// applying optional vertical and offset adjustments.
            /// </summary>
            /// <param name="prefab">The GameObject prefab to spawn.</param>
            /// <param name="spawnLocation">Hand-based target location on the table.</param>
            /// <param name="taskDifficulty">Current task difficulty level.</param>
            /// <returns>The spawned GameObject instance.</returns>
            public GameObject SpawnPrefab(GameObject prefab, ESpawnLocation spawnLocation, Difficulty taskDifficulty)
            {
                return SpawnPrefab(prefab, spawnLocation, taskDifficulty, Vector3.zero);
            }
            
            /// <summary>
            /// Spawns a prefab at a target location on the table based on difficulty and spawn zone,
            /// applying optional vertical and offset adjustments.
            /// </summary>
            /// <param name="prefab">The GameObject prefab to spawn.</param>
            /// <param name="spawnLocation">Hand-based target location on the table.</param>
            /// <param name="taskDifficulty">Current task difficulty level.</param>
            /// <param name="shouldTransformY">Whether to lift the object above the surface using its bounds.</param>
            /// <returns>The spawned GameObject instance.</returns>
            public GameObject SpawnPrefab(GameObject prefab, ESpawnLocation spawnLocation, Difficulty taskDifficulty, bool shouldTransformY)
            {
                return SpawnPrefab(prefab, spawnLocation, taskDifficulty, Vector3.zero, shouldTransformY);
            }

            /// <summary>
            /// Spawns a prefab at a target location on the table based on difficulty and spawn zone,
            /// applying optional vertical and offset adjustments.
            /// </summary>
            /// <param name="prefab">The GameObject prefab to spawn.</param>
            /// <param name="spawnLocation">Hand-based target location on the table.</param>
            /// <param name="taskDifficulty">Current task difficulty level.</param>
            /// <param name="offset">Optional positional offset.</param>
            /// <param name="shouldTransformY">Whether to lift the object above the surface using its bounds.</param>
            /// <returns>The spawned GameObject instance.</returns>
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
                GameObject spawnedObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                targetPos = GetCorrectedPositionToKeepObjectOnTable(spawnedObject.GetComponent<Collider>().bounds, Vector3.zero, targetPos, targetRot);
                Instance.StartCoroutine(DelayedSafePlace(spawnedObject, targetPos, targetRot));
                return spawnedObject;
            }
            
            /// <summary>
            /// Safely places the given GameObject at the target position and rotation after ensuring
            /// no collision occurs with objects on the "Hands" layer for a consecutive number of frames.
            /// During the wait, the object's renderers are temporarily disabled and Rigidbody is set kinematic
            /// to avoid physics interference.
            /// </summary>
            /// <param name="obj">The GameObject to be placed.</param>
            /// <param name="targetPosition">The target world position where the object should be placed.</param>
            /// <param name="targetRotation">The target rotation to apply to the object.</param>
            /// <param name="requiredClearFrames">
            /// Number of consecutive frames during which the placement area must be clear of collisions with "Hands" layer objects.
            /// Default is 3.
            /// </param>
            /// <returns>An IEnumerator for coroutine execution.</returns>
            private IEnumerator DelayedSafePlace(GameObject obj, Vector3 targetPosition, Quaternion targetRotation, int requiredClearFrames = 3)
            {
                Transform objTransform = obj.transform;
                if (!obj.TryGetComponent(out Rigidbody rb))
                {
                    objTransform.SetPositionAndRotation(targetPosition, targetRotation);
                    yield break;
                }
                
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

                objTransform.SetPositionAndRotation(targetPosition, targetRotation);
            }
            
            /// <summary>
            /// Adjusts the target spawn position of an object to ensure it stays fully within the table boundaries.
            /// Checks the object's rotated bounds corners against the table extents and computes a positional correction if needed.
            /// </summary>
            /// <param name="bounds">The bounding box of the object to be spawned (in local space).</param>
            /// <param name="spawnPos">The original spawn position of the object (used as reference for bounds center).</param>
            /// <param name="targetPosition">The desired target position where the object should be placed before correction.</param>
            /// <param name="targetRotation">The rotation that will be applied to the object at spawn time.</param>
            /// <returns>
            /// The adjusted position corrected to keep the object fully on the table surface.
            /// </returns>
            private Vector3 GetCorrectedPositionToKeepObjectOnTable(Bounds bounds, Vector3 spawnPos, Vector3 targetPosition, Quaternion targetRotation)
            {
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
}

