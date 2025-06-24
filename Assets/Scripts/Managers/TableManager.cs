using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LearnXR.Core.Utilities;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using Tasks;
using TMPro;
using UI.Panels;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class TableManager : Singleton<TableManager>
    {
        [SerializeField] private EffectMesh collidersEffectMesh;
        [SerializeField] private EffectMesh invalidTablesEffectMesh;
        [SerializeField] private EffectMesh validTablesEffectMesh;
        [SerializeField] private EffectMesh selectedTableEffectMesh;
        [SerializeField] private GameObject tableLabelPrefab;
        
        public event Action OnTableSelected;
        public event Action OnStartTableSelection;
        
        private readonly Dictionary<RayInteractable, Action<PointerEvent>> _tableHoveringHandlers = new();
        private const float MinTableSideOffset = 0.2f;

        public bool IsInit => _selectedTableAnchor;
        public Table SelectedTable { get; private set; }

        private MRUKAnchor _selectedTableAnchor;


        protected override void Awake()
        {
            base.Awake();
            OVRScene.RequestSpaceSetup();
        }

        public void StartTableSelecting()
        {
            OnStartTableSelection?.Invoke();
            if (_selectedTableAnchor)
            {
                //SpatialLogger.Instance.LogInfo("Table anchor is already selected");
                validTablesEffectMesh.CreateEffectMesh(_selectedTableAnchor);
                selectedTableEffectMesh.DestroyMesh(_selectedTableAnchor);
                _selectedTableAnchor = null;
            }
            else
            {
                //SpatialLogger.Instance.LogInfo("Table anchor is not selected");
                ValidateTables();
            }
            
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
            //SpatialLogger.Instance.LogInfo("Show tables");

            validTablesEffectMesh.HideMesh = false;
            invalidTablesEffectMesh.HideMesh = false;
            selectedTableEffectMesh.HideMesh = false;
            //SpatialLogger.Instance.LogInfo("Tables are shown");
        }


        private void ClearTableLabels()
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

        private void OnDisable()
        {
            foreach (var (interactable, action) in _tableHoveringHandlers)
            {
                if(interactable) {
                    interactable.WhenPointerEventRaised -= action;
                }
            }
        }
        
        private void HandleTableHovering(PointerEvent pointerEvent, MRUKAnchor table)
        {
            switch (pointerEvent.Type)
            {
                case PointerEventType.Hover:
                    if (selectedTableEffectMesh.EffectMeshObjects.Count == 0) //a table can trigger hover event only it's the first table that is being hovered
                    {
                        selectedTableEffectMesh.CreateEffectMesh(table);
                        validTablesEffectMesh.DestroyMesh(table);
                    }
                    break;

                case PointerEventType.Unhover:
                    if (selectedTableEffectMesh.EffectMeshObjects.Keys.Contains(table)) //can be triggered only with already selected table
                    {
                        selectedTableEffectMesh.DestroyMesh(table);
                        validTablesEffectMesh.CreateEffectMesh(table);
                    }

                    break;

                case PointerEventType.Select:
                    if (selectedTableEffectMesh.EffectMeshObjects.Keys.Contains(table)) //select event can be happened only with the selected table
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
        
        private void ValidateTables()
        {
            //SpatialLogger.Instance.LogInfo("Validate tables");
            Vector2 stairsSizes = TaskObjectPrefabsManager.Instance.GetStairsSizes();
            if (stairsSizes.Equals(Vector2.zero))
            {
                
                Debug.LogError("StairsPrefab is not init");
                return;
            }
            
            foreach (MRUKAnchor anchor in MRUK.Instance.GetCurrentRoom().Anchors)
            {
                if (!anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE)) continue;
                //SpatialLogger.Instance.LogInfo("Table");
                selectedTableEffectMesh.DestroyMesh(anchor);

                if (!anchor.VolumeBounds.HasValue) continue;
                //SpatialLogger.Instance.LogInfo("Has volume");
                var anchorExtents = anchor.VolumeBounds.Value.extents;
                bool isXLarger = anchorExtents.x > anchorExtents.y;
                float width = (isXLarger ? anchorExtents.y : anchorExtents.x) * 2;
                float length = (isXLarger ? anchorExtents.x : anchorExtents.y) * 2;
                
                //if table invalid - remove from valid, else - remove from invalid
                if (length - stairsSizes.x < MinTableSideOffset || width - stairsSizes.y < MinTableSideOffset)
                {
                    validTablesEffectMesh.DestroyMesh(anchor);
                    //SpatialLogger.Instance.LogInfo("Invalid");
                }
                else
                {
                    invalidTablesEffectMesh.DestroyMesh(anchor);
                    //SpatialLogger.Instance.LogInfo("Invalid");
                }
            }
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


    }
}

