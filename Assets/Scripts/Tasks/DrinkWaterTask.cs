using System;
using LearnXR.Core.Utilities;
using Managers;
using Hands.Grabbables;
using UnityEngine;
using LiquidPhysics;
using Tasks.TaskObjectScripts;
using Tasks.TaskProperties;

namespace Tasks
{
    /// <summary>
    /// A task where the user must drink water from a glass.
    /// The task is considered complete when the user drinks a sufficient amount of water and places the glass on the podest.
    /// </summary>
    public class DrinkWaterTask: Task
    {
        public override string Name => "Drink Water";
        public override ETaskType TaskType => ETaskType.DrinkWater;
        public override string TaskDescription => "Drink water and put the glass on the podest";

        [SerializeField] private DrinkingArea drinkingArea;

        private Container _spawnedGlassContainer;
        private KinematicGrabbable _spawnedGlassKinematicGrabbable;

        private float _drankInsideDrinkingArea;
        private float _initialFullness;
        private bool _isInDrinkingArea;
        private bool _startedDrinking;
        private Podest _podest;

        #region CONST

        /// <summary>
        /// Max angle between camera forward vector and vector between camera and object in degrees
        /// </summary>
        private const float MaxViewAngle = 30f;

        private const float MinDrankToCompleteBase = 0.6f;

        private float MinDrankToComplete => MinDrankToCompleteBase + 0.1f * (float)TaskSettings.difficulty;

        #endregion
        
        

        protected override void IncreaseScore()
        {
            base.IncreaseScore();
            InitializeDefaults();
        }

        protected override bool AreAllObjectsSatisfyConditions()
        {
            if (_spawnedGlassContainer.IsFull || !IsGlassStandsStraightOnTable) return false;
            if (_podest.IsRed || _podest.IsBlue)
            {
                UpdateHint("Put the glass on the podest!");
                return false;
            }

            if (_podest.IsGreen && _drankInsideDrinkingArea < MinDrankToComplete)
            {
                UpdateHint(GetPercentage());
                InitializeDefaults();
                return false;
            }
            UpdateHint(GetPercentage());
            return true;
        }

        protected override void EvaluateTask()
        {
            bool isDrinking = _isInDrinkingArea && _spawnedGlassContainer.IsPouring && IsCameraLookingAtGlass();
            
            if (isDrinking)
            {
                if (!_startedDrinking)
                {
                    // Just started drinking
                    _initialFullness = _spawnedGlassContainer.Fullness;
                }

                _startedDrinking = true;
            }
            else
            {
                if (_startedDrinking)
                {
                    // Just stopped drinking
                    float deltaFullness = _initialFullness - _spawnedGlassContainer.Fullness;
                    if (deltaFullness > 0)
                    {
                        _drankInsideDrinkingArea += deltaFullness;
                        SpatialLogger.Instance.LogInfo($"Drank this session: {deltaFullness}, Total drank: {_drankInsideDrinkingArea}");
                    }
                }

                _startedDrinking = false;
            }
        }

        protected override void SpawnObjects()
        {
            var table = TableManager.Instance.SelectedTable;
            TaskObjectPrefabsManager taskObjMan = TaskObjectPrefabsManager.Instance;
            Difficulty currentDifficulty = TaskSettings.difficulty;
            
            GameObject podest = table.SpawnPrefab(taskObjMan.circularPodest, TableManager.Table.ESpawnLocation.Primary, currentDifficulty);
            SpawnedObjects.Add(podest);
            _podest = podest.GetComponent<Podest>();
            
            GameObject spawnedGlass = table.SpawnPrefab(taskObjMan.glassPrefab, TableManager.Table.ESpawnLocation.Primary, currentDifficulty);
            SpawnedObjects.Add(spawnedGlass);
            
            _spawnedGlassContainer = spawnedGlass.GetComponent<Container>();
            _spawnedGlassContainer.Refill();
            _spawnedGlassKinematicGrabbable = spawnedGlass.GetComponent<KinematicGrabbable>();
            _spawnedGlassKinematicGrabbable.SetPressBlockAreaSize(currentDifficulty);
            
            //reset this value when ResetObjects is called
            _drankInsideDrinkingArea = 0;
        }
        
        protected override void InitializeDefaults()
        {
            _spawnedGlassContainer.Refill();
            _drankInsideDrinkingArea = 0;
        }
        
        /// <summary>
        /// Checks whether the player doesn't pour water on his leg or head
        /// </summary>
        /// <returns>True if looks at the glass, otherwise false</returns>
        private bool IsCameraLookingAtGlass()
        {
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError("No Camera");
                return false;
            }

            var cameraTransform = mainCamera.transform;
            var directionToTarget = (_spawnedGlassContainer.PourOriginPos - cameraTransform.position).normalized;
            return Vector3.Angle(cameraTransform.forward, directionToTarget) <= MaxViewAngle;
        }

        private void OnEnterDrinkingArea(Collider newCollider)
        {
            if (!newCollider.CompareTag("Glass")) return;
            _isInDrinkingArea = true;
        }
        
        private void OnExitDrinkingArea(Collider newCollider)
        {
            if (!newCollider.CompareTag("Glass")) return;
            _isInDrinkingArea = false;
        }
        
        private string GetPercentage()
        {
            float drankInside = (float) Math.Round(_drankInsideDrinkingArea * 100, 2);
            float minDrank = (float) Math.Round(MinDrankToComplete * 100, 2);
            return drankInside < minDrank?
                $"Drank {drankInside}% < {minDrank}%, sad :(":
                $"Drank {drankInside}% >= {minDrank}%, well done :)";
        }
        

        private bool IsGlassStandsStraightOnTable =>
            !_spawnedGlassKinematicGrabbable.IsHeld &&
            //IsObjectOnTable(_spawnedGlassContainer.gameObject) &&
            TableManager.Instance.SelectedTable.IsPositionOnTable(_spawnedGlassContainer.GetComponent<Renderer>().bounds.min, 0f, 0.01f) &&
            IsObjectWatchingUpwards(_spawnedGlassContainer.gameObject);
        
        private void OnEnable()
        {
            drinkingArea.TriggerEntered += OnEnterDrinkingArea;
            drinkingArea.TriggerExited += OnExitDrinkingArea;
        }

        private void OnDisable()
        {
            drinkingArea.TriggerEntered -= OnEnterDrinkingArea;
            drinkingArea.TriggerExited -= OnExitDrinkingArea;
        }
    }
}