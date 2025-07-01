using System;
using LearnXR.Core.Utilities;
using Tasks;
using UI.PanelControllers;
using UnityEngine;
using Tasks.TaskProperties;

namespace Managers
{
    /// <summary>
    /// Manager that is responsible for saving player's max reachable area.  
    /// </summary>
    public class ReachAreaManager: Singleton<ReachAreaManager>
    {
        /// <summary>
        /// Provides object spawn positions for interactable objects based on task's difficulty,
        /// player handedness and max reachable area.
        /// </summary>
        public readonly struct HandPositions
        {
            private readonly Vector3 _primaryHand;
            private readonly Vector3 _secondaryHand;
            private readonly Vector3 _centerPos;

            public HandPositions(Vector3 left, Vector3 right)
            {
                if (SettingsManager.Instance.IsLeftHanded)
                {
                    _primaryHand = left;
                    _secondaryHand = right;
                }
                else
                {
                    _primaryHand = right;
                    _secondaryHand = left;
                }
                _centerPos = (left + right) * 0.5f;
            }
            
            public Vector3 GetSpawnPosition(Difficulty difficulty, TableManager.Table.ESpawnLocation spawnLocation)
            {
                Vector3 handPos;
                switch (spawnLocation)
                { 
                    case TableManager.Table.ESpawnLocation.Primary:
                        handPos = _primaryHand;
                        break;
                    case TableManager.Table.ESpawnLocation.Secondary:
                        handPos = _secondaryHand;
                        break;
                    default:
                        return _centerPos;
                }

                return _centerPos + (handPos - _centerPos) * (float)difficulty / (float)Difficulty.Hard;
            }
        }
        
        private readonly Quaternion _rotationAroundY = Quaternion.Euler(0, 180f, 0);

        /// <summary>
        /// Left hand prefab to spawn on the table after recording the player's maximum reachable area.
        /// Should match the virtual hand prefab used during gameplay.
        /// </summary>
        [SerializeField] private GameObject openXRLeftHand;

        /// <summary>
        /// Right hand prefab to spawn on the table after recording the player's maximum reachable area.
        /// Should match the virtual hand prefab used during gameplay.
        /// </summary>
        [SerializeField] private GameObject openXRRightHand;

        
        private GameObject _leftSpawnedHand;
        private GameObject _rightSpawnedHand;

        public bool IsInit => _leftSpawnedHand && _rightSpawnedHand;

        
        /// <summary>
        /// Returns the current hand positions in world space, falling back to root transform if XRHand_Palm is not found.
        /// Also updates virtual hand positions before fetching.
        /// </summary>
        /// <returns>Struct containing primary, secondary, and center hand positions based on handedness.</returns>
        public HandPositions GetHandPositions()
        {
            UpdateVirtualHandsPositions();

            Transform leftPalmTransform = _leftSpawnedHand.transform.FindChildRecursive("XRHand_Palm");
            Vector3 left = leftPalmTransform != null ? leftPalmTransform.position : _leftSpawnedHand.transform.position; 
            
            Transform rightPalmTransform = _rightSpawnedHand.transform.FindChildRecursive("XRHand_Palm");
            Vector3 right = rightPalmTransform != null ? rightPalmTransform.position : _rightSpawnedHand.transform.position; 

            return new HandPositions(left, right);
        }
        
        /// <summary>
        /// Rotates the given hand transform around the selected table's center to reflect a change in the player's seating position.
        /// </summary>
        /// <param name="obj">Transform of the hand to rotate.</param>
        private void RotateHand(Transform obj)
        {
            Vector3 topCenter = TableManager.Instance.SelectedTable.TopCenter;
            Vector3 direction = obj.position - topCenter;
            Vector3 rotatedDirection = _rotationAroundY * direction;
            
            obj.position = topCenter + rotatedDirection;
            obj.rotation = _rotationAroundY * obj.rotation;
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
        
        
        
        private void OnEnable()
        {
            ToggleSpawnedHands(true);
        }
        
        private void Start()
        {
            ReachAreaSettingsPanelController.OnTimerCompleted += SpawnHands;
            TableManager.Instance.OnTableSelected += DestroyHands;
            enabled = false;
        }
        
        private void Update()
        {
            UpdateVirtualHandsPositions();
        }
        
        private void OnDisable()
        {
            ToggleSpawnedHands(false);
        }
        
        private void OnDestroy()
        {
            ReachAreaSettingsPanelController.OnTimerCompleted -= SpawnHands;
            TableManager.Instance.OnTableSelected -= DestroyHands;
        }
    }
}