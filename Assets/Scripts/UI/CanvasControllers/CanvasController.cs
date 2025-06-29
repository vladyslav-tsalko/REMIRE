
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using UI.PanelControllers;
using UnityEngine;

namespace UI.CanvasControllers
{
    public abstract class CanvasController : MonoBehaviour
    {
        private static readonly float DistanceThreshold = 0.1f;
        private static readonly float MoveSpeed = 4.5f;
        private static readonly float RotationThresholdDegrees = 45f;
        private static Transform CameraTransform; 

        private Vector3 _targetPosition;
        private Quaternion _initialCameraRotation;
        private bool _shouldReposition;
        
        protected GameObject CanvasObject;
        protected readonly List<UIPanel> Panels = new();
        private UIPanel _activePanel;

        protected abstract float DesiredDistance { get; }
        
        private Vector3 _repositionStartCameraPosition;
        private Quaternion _repositionStartCameraRotation;

        private static readonly float RepositionUpdateDistanceThreshold = 0.05f;
        private static readonly float RepositionUpdateRotationThreshold = 5f;


        private void Awake()
        {
            //Collect canvasObject
            CanvasObject = GetComponentInChildren<Canvas>(true).gameObject;
            if (!CanvasObject)
            {
                Debug.LogError("canvasObject is not found");
            }
            else
            {
                var uiPanels = CanvasObject.GetComponentsInChildren<UIPanel>(true).ToList();
                Panels.AddRange(uiPanels);
                Panels.ForEach(panel => panel.gameObject.SetActive(false));
            }
        }

        protected virtual void Start()
        {
            if (CameraTransform == null) CameraTransform = Camera.main.transform;
            _initialCameraRotation = CameraTransform.rotation;
            transform.position = CameraTransform.position + CameraTransform.forward * DesiredDistance;
        }


        void LateUpdate()
        {
            float angleDelta = Quaternion.Angle(_initialCameraRotation, CameraTransform.rotation);
            float currentDistance = Vector3.Distance(transform.position, CameraTransform.position);

            if (!_shouldReposition && 
                (angleDelta > RotationThresholdDegrees || Mathf.Abs(currentDistance - DesiredDistance) > DistanceThreshold))
            {
                _shouldReposition = true;
                _repositionStartCameraPosition = CameraTransform.position;
                _repositionStartCameraRotation = CameraTransform.rotation;
                _targetPosition = CameraTransform.position + CameraTransform.forward * DesiredDistance;
            }

            if (_shouldReposition)
            {
                // Check if camera moved or rotated significantly *during* reposition
                float repositionDeltaDist = Vector3.Distance(CameraTransform.position, _repositionStartCameraPosition);
                float repositionDeltaAngle = Quaternion.Angle(CameraTransform.rotation, _repositionStartCameraRotation);

                if (repositionDeltaDist > DistanceThreshold || repositionDeltaAngle > RepositionUpdateRotationThreshold)
                {
                    _repositionStartCameraPosition = CameraTransform.position;
                    _repositionStartCameraRotation = CameraTransform.rotation;
                    _targetPosition = CameraTransform.position + CameraTransform.forward * DesiredDistance;
                }

                transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * MoveSpeed);

                Quaternion lookRotation = Quaternion.LookRotation(transform.position - CameraTransform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * MoveSpeed);

                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    _shouldReposition = false;
                    _initialCameraRotation = CameraTransform.rotation;
                }
            }
        }

        public bool ShowPanel(EPanelType panelType)
        {
            bool found = false;
            Panels.ForEach(panel =>
            {
                if (panel.PanelType == panelType)
                {
                    if (_activePanel)
                    {
                        _activePanel.gameObject.SetActive(false);
                    }
                    _activePanel = panel;
                    _activePanel.gameObject.SetActive(true);
                    found = true;
                }
            });

            return found;
        }

        private void ToggleCanvas(bool isToggle)
        {
            if (CanvasObject.activeSelf != isToggle)
            {
                CanvasObject.SetActive(isToggle);
            }
        }

        public virtual void ShowCanvas()
        {
            ToggleCanvas(true);
        }

        public void HideCanvas()
        {
            ToggleCanvas(false);
        }
    }

}
