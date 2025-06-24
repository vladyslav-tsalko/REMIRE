using System;
using LearnXR.Core.Utilities;
using Tasks;
using UI.Panels;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class ReachAreaManager: Singleton<ReachAreaManager>
    {
        public readonly struct HandPositions
        {
            public Vector3 PrimaryHand { get; }
            public Vector3 SecondaryHand { get; }
            public Vector3 CenterPos { get; }

            public HandPositions(Vector3 left, Vector3 right)
            {
                if (SettingsManager.Instance.Settings.LeftHanded)
                {
                    PrimaryHand = left;
                    SecondaryHand = right;
                }
                else
                {
                    PrimaryHand = right;
                    SecondaryHand = left;
                }
                CenterPos = (left + right) * 0.5f;
            }
            
            public Vector3 GetSpawnPosition(Difficulty difficulty, ESpawnLocation spawnLocation)
            {
                Vector3 handPos;
                switch (spawnLocation)
                { 
                    case ESpawnLocation.Primary:
                        handPos = PrimaryHand;
                        break;
                    case ESpawnLocation.Secondary:
                        handPos = SecondaryHand;
                        break;
                    default:
                        return CenterPos;
                }

                return CenterPos + (handPos - CenterPos) * (float)difficulty / (float)Difficulty.Hard;
            }
        }

        [SerializeField] private GameObject openXRLeftHand;
        [SerializeField] private GameObject openXRRightHand;
        
        private readonly Quaternion _rot = Quaternion.Euler(0, 180f, 0);
        
        private GameObject _leftSpawnedHand;
        private GameObject _rightSpawnedHand;

        public bool IsInit => _leftSpawnedHand && _rightSpawnedHand;

        public HandPositions GetHandPositions()
        {
            UpdateVirtualHandsPositions();

            Transform leftPalmTransform = _leftSpawnedHand.transform.FindChildRecursive("XRHand_Palm");
            Vector3 left = leftPalmTransform != null ? leftPalmTransform.position : _leftSpawnedHand.transform.position; 
            
            Transform rightPalmTransform = _rightSpawnedHand.transform.FindChildRecursive("XRHand_Palm");
            Vector3 right = rightPalmTransform != null ? rightPalmTransform.position : _rightSpawnedHand.transform.position; 

            return new HandPositions(left, right);
        }
        private void RotateHand(Transform obj)
        {
            Vector3 topCenter = TableManager.Instance.SelectedTable.TopCenter;
            Vector3 direction = obj.position - topCenter;
            Vector3 rotatedDirection = _rot * direction;
            
            obj.position = topCenter + rotatedDirection;
            obj.rotation = _rot * obj.rotation;
        }
        
        private void ToggleSpawnedHands(bool toggle)
        {
            if (_leftSpawnedHand && _rightSpawnedHand)
            {
                if (_leftSpawnedHand.activeSelf != toggle)
                {
                    _leftSpawnedHand.SetActive(toggle);
                    _rightSpawnedHand.SetActive(toggle);
                }
            }
        }

        private void OnEnable()
        {
            ToggleSpawnedHands(true);
        }

        private void OnDisable()
        {
            ToggleSpawnedHands(false);
        }

        private void UpdateVirtualHandsPositions()
        {
            if (_leftSpawnedHand && _rightSpawnedHand)
            {
                if (TableManager.Instance.SelectedTable.UpdateVectors())
                {
                    RotateHand(_leftSpawnedHand.transform);
                    RotateHand(_rightSpawnedHand.transform);
                }
            }
        }

        private void Update()
        {
            UpdateVirtualHandsPositions();
        }
        
        
        void Start()
        {
            ReachAreaSettingsPanelController.OnTimerCompleted += SpawnHands;
            TableManager.Instance.OnTableSelected += DestroyHands;
            enabled = false;
        }

        private void DestroyHands()
        {
            if(_leftSpawnedHand) Destroy(_leftSpawnedHand);
            if(_rightSpawnedHand) Destroy(_rightSpawnedHand);
        }

        private void SpawnHands()
        {
            DestroyHands();
            _leftSpawnedHand = Instantiate(openXRLeftHand);
            _rightSpawnedHand = Instantiate(openXRRightHand);
        }
        
        private void OnDestroy()
        {
            ReachAreaSettingsPanelController.OnTimerCompleted -= SpawnHands;
            TableManager.Instance.OnTableSelected -= DestroyHands;
        }
    }
}