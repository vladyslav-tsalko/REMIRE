using System;
using System.Linq;
using LearnXR.Core.Utilities;
using Managers;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using Hands.Grabbables;
using LiquidPhysics;
using Tasks.TaskObjectScripts;
using Tasks.TaskProperties;

namespace Tasks
{
    /// <summary>
    /// A task where the user must pour water from a bottle into a glass.
    /// The task is completed when the glass is fully filled while both objects are held in the user's hands, 
    /// and then both are placed on the podests.
    /// </summary>
    public class PourWaterTask: Task
    {
        public override string Name => "Pour Water";
        
        public override ETaskType TaskType => ETaskType.PourWater;
        
        public override string TaskDescription => "Put filled glass and the bottle on the podests";

        private Container _spawnedGlassContainer;
        private KinematicGrabbable _spawnedGlassKinematicGrabbable;
        private float _lastValidFullness;
        
        protected override void SpawnObjects()
        {
            var table = TableManager.Instance.SelectedTable;
            TaskObjectPrefabsManager taskObjMan = TaskObjectPrefabsManager.Instance;
            Difficulty currentDifficulty = TaskSettings.difficulty;

            var primaryPodest = table.SpawnPrefab(taskObjMan.circularPodest, TableManager.Table.ESpawnLocation.Primary, currentDifficulty);
            
            var secondaryPodest = table.SpawnPrefab(taskObjMan.circularPodest, TableManager.Table.ESpawnLocation.Secondary, currentDifficulty);
            
            GameObject spawnedGlass = table.SpawnPrefab(taskObjMan.glassPrefab, TableManager.Table.ESpawnLocation.Secondary, currentDifficulty);
            _spawnedGlassKinematicGrabbable = spawnedGlass.GetComponent<KinematicGrabbable>();
            _spawnedGlassContainer = spawnedGlass.GetComponent<Container>();
            
            _spawnedGlassKinematicGrabbable.SetPressBlockAreaSize(currentDifficulty);
            _lastValidFullness = _spawnedGlassContainer.Fullness;
            
            SpawnedObjects.Add(spawnedGlass);
            
            GameObject spawnedBottle = table.SpawnPrefab(taskObjMan.bottlePrefab, TableManager.Table.ESpawnLocation.Primary, currentDifficulty);
            spawnedBottle.GetComponent<KinematicGrabbable>().SetPressBlockAreaSize(currentDifficulty);
            SpawnedObjects.Add(spawnedBottle);
            
            //Add them into the list later, so AreAllObjectsSatisfyConditions function
            //will firstly check all KinematicGrabbables, and then only podests, to efficiently return false,
            //because when an object is not staying, we dont need to check podests.
            //Only when object are staying, we check if they are on the podests
            
            SpawnedObjects.Add(primaryPodest);
            SpawnedObjects.Add(secondaryPodest);
            
        }
        
        protected override void IncreaseScore()
        {
            base.IncreaseScore();
            _spawnedGlassContainer.MakeEmpty();
        }
        
        protected override bool AreAllObjectsSatisfyConditions()
        {
            foreach (var spawnedObject in SpawnedObjects)
            {
                if (spawnedObject.TryGetComponent(out KinematicGrabbable kinematicGrabbable))
                {
                    //Is held or not stays on the table
                    if (kinematicGrabbable.IsHeld || !IsObjectWatchingUpwards(spawnedObject))
                    {
                        return false;
                    }
                }else if (spawnedObject.TryGetComponent(out Podest podest))
                {
                    //If no object stays on the podest
                    if (!podest.IsGreen)
                    {
                        UpdateHint("Put objects on the podests!");
                        return false;
                    }
                }
                else return false;
            }
        
            if (_spawnedGlassContainer.IsEmpty) return false;
            if (!_spawnedGlassContainer.IsFilledEnough())
            {
                UpdateHint("The glass must be fully filled :(");
                _spawnedGlassContainer.MakeEmpty();
                return false;
            }
            
            UpdateHint("The glass is fully filled :)");
            return true;
        }

        protected override void EvaluateTask()
        {
            if (_spawnedGlassKinematicGrabbable.IsHeld)
            {
                // Update the last valid fullness value while it's being held
                _lastValidFullness = _spawnedGlassContainer.Fullness;
            }
            else
            {
                // If the glass was filled after it was released, reset it
                if (_spawnedGlassContainer.Fullness > _lastValidFullness)
                {
                    _spawnedGlassContainer.MakeEmpty();
                    _lastValidFullness = _spawnedGlassContainer.Fullness;
                    UpdateHint("Glass can be filled only while grabbed.");
                }
            }
        }
    }
}