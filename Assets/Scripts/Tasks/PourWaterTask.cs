using System;
using System.Linq;
using LearnXR.Core.Utilities;
using Managers;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using Utilities;

namespace Tasks
{
    public class PourWaterTask: Task
    {
        public override string Name => "Pour Water";
        
        public override ETaskType TaskType => ETaskType.PourWater;
        
        public override string TaskDescription => "Put filled glass and the bottle on the podests";

        private Container _spawnedGlassContainer;
        private Grabbable _spawnedGlassGrabbable;
        private float _lastValidFullness;
        
        protected override void SpawnObjects()
        {
            Table table = TableManager.Instance.SelectedTable;
            TaskObjectPrefabsManager taskObjMan = TaskObjectPrefabsManager.Instance;
            Difficulty currentDifficulty = TaskSettings.difficulty;

            var primaryPodest = table.SpawnPrefab(taskObjMan.CircularPodest, ESpawnLocation.Primary, currentDifficulty);
            
            var secondaryPodest = table.SpawnPrefab(taskObjMan.CircularPodest, ESpawnLocation.Secondary, currentDifficulty);
            
            GameObject spawnedGlass = table.SpawnPrefab(taskObjMan.GlassPrefab, ESpawnLocation.Secondary, currentDifficulty);
            _spawnedGlassGrabbable = spawnedGlass.GetComponent<Grabbable>();
            _spawnedGlassContainer = spawnedGlass.GetComponent<Container>();
            
            _spawnedGlassGrabbable.SetPressBlockAreaSize(currentDifficulty);
            _lastValidFullness = _spawnedGlassContainer.Fullness;
            
            SpawnedObjects.Add(spawnedGlass);
            
            GameObject spawnedBottle = table.SpawnPrefab(taskObjMan.BottlePrefab, ESpawnLocation.Primary, currentDifficulty);
            spawnedBottle.GetComponent<Grabbable>().SetPressBlockAreaSize(currentDifficulty);
            SpawnedObjects.Add(spawnedBottle);
            
            //Add them into the list later, so AreAllObjectsSatisfyConditions function
            //will firstly check all grabbables, and then only podests, to efficiently return false,
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
                if (spawnedObject.TryGetComponent(out Grabbable grabbable))
                {
                    if (grabbable.IsHeld || !IsObjectWatchingUpwards(spawnedObject))
                    {
                        return false;
                    }
                }else if (spawnedObject.TryGetComponent(out Podest podest))
                {
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
            if (_spawnedGlassGrabbable.IsHeld)
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