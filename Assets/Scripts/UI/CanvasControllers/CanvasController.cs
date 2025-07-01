
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using UI.PanelControllers;
using UnityEngine;

namespace UI.CanvasControllers
{
    /// <summary>
    /// Base class for all canvas controllers.
    /// Responsibilities:
    /// - Shows and hides panels based on panel type.
    /// - Toggles the attached Canvas component.
    /// - Positions the canvas to follow the camera at fixed angles and a fixed distance.
    /// </summary>
    public abstract class CanvasController : MonoBehaviour
    {
        private static readonly float DistanceThreshold = 0.1f;
        private static readonly float MoveSpeed = 4.5f;
        private static readonly float RotationThresholdDegrees = 45f;
        private static readonly float RepositionUpdateRotationThreshold = 5f;
        private static Transform _cameraTransform; 

        private Vector3 _targetPosition;
        private Quaternion _initialCameraRotation;
        private bool _shouldReposition;

        private GameObject _canvasObject;
        private readonly List<UIPanel> _panels = new();
        private UIPanel _activePanel;

        protected abstract float DesiredDistance { get; }
        
        private Vector3 _repositionStartCameraPosition;
        private Quaternion _repositionStartCameraRotation;


        private void Awake()
        {
            //Collect canvasObject
            _canvasObject = GetComponentInChildren<Canvas>(true).gameObject;
            if (!_canvasObject)
            {
                Debug.LogError("canvasObject is not found");
            }
            else
            {
                var uiPanels = _canvasObject.GetComponentsInChildren<UIPanel>(true).ToList();
                _panels.AddRange(uiPanels);
                _panels.ForEach(panel => panel.gameObject.SetActive(false));
            }
        }

        protected virtual void Start()
        {
            if (_cameraTransform == null) _cameraTransform = Camera.main.transform;
            _initialCameraRotation = _cameraTransform.rotation;
            transform.position = _cameraTransform.position + _cameraTransform.forward * DesiredDistance;
        }


        void LateUpdate()
        {
            float angleDelta = Quaternion.Angle(_initialCameraRotation, _cameraTransform.rotation);
            float currentDistance = Vector3.Distance(transform.position, _cameraTransform.position);

            if (!_shouldReposition && 
                (angleDelta > RotationThresholdDegrees || Mathf.Abs(currentDistance - DesiredDistance) > DistanceThreshold))
            {
                _shouldReposition = true;
                _repositionStartCameraPosition = _cameraTransform.position;
                _repositionStartCameraRotation = _cameraTransform.rotation;
                _targetPosition = _cameraTransform.position + _cameraTransform.forward * DesiredDistance;
            }

            if (_shouldReposition)
            {
                float repositionDeltaDist = Vector3.Distance(_cameraTransform.position, _repositionStartCameraPosition);
                float repositionDeltaAngle = Quaternion.Angle(_cameraTransform.rotation, _repositionStartCameraRotation);

                if (repositionDeltaDist > DistanceThreshold || repositionDeltaAngle > RepositionUpdateRotationThreshold)
                {
                    _repositionStartCameraPosition = _cameraTransform.position;
                    _repositionStartCameraRotation = _cameraTransform.rotation;
                    _targetPosition = _cameraTransform.position + _cameraTransform.forward * DesiredDistance;
                }

                transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * MoveSpeed);

                Quaternion lookRotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * MoveSpeed);

                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    _shouldReposition = false;
                    _initialCameraRotation = _cameraTransform.rotation;
                }
            }
        }

        protected void ShowPanel(EPanelType panelType)
        {
            _panels.ForEach(panel =>
            {
                if (panel.PanelType == panelType)
                {
                    if (_activePanel)
                    {
                        _activePanel.gameObject.SetActive(false);
                    }
                    _activePanel = panel;
                    _activePanel.gameObject.SetActive(true);
                }
            });
        }

        private void ToggleCanvas(bool isToggle)
        {
            if (_canvasObject.activeSelf != isToggle)
            {
                _canvasObject.SetActive(isToggle);
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
