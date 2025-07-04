using System;
using System.Collections.Generic;
using Hands.Grabbables;
using Managers;
using UnityEngine;
using Tasks.TaskProperties;

namespace Tasks
{
    /// <summary>
    /// Provides base functionality for any task that can be created. Any task must be inherited from this class.
    /// </summary>
    public abstract class Task: MonoBehaviour
    {
        public static Action<int> OnScoreUpdate;
        public static Action<string> OnHintUpdated;

        #region ABSTRACT VARIABLES

        /// <summary>
        /// The display name of the task
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// The type of the task
        /// </summary>
        public abstract ETaskType TaskType { get; }

        public abstract string TaskDescription { get; }

        #endregion
        
        #region COEFFICIENTS

        private const float BaseXRotation = 0f;

        private const float MinXRotationOffset = 2.0f;
        
        private const float MinOnFloorOffset = 0.1f;
                
        private const float MinObjectTableOffset = 0.01f;

        #endregion
        
        public bool IsCompleted { get; private set; }
        
        protected TaskSettings TaskSettings;

        public TaskProgress TaskProgress { get; private set; } = new ();

        protected readonly List<GameObject> SpawnedObjects = new();
        
        public void ResetObjects()
        {
            HandsManager.Instance.KinematicGrabberLeft.ReleaseObject();
            HandsManager.Instance.KinematicGrabberRight.ReleaseObject();
            ClearSpawnedObjects();
            SpawnObjects();
            UpdateHint(TaskDescription);
        }
        
        public void Load()
        {
            TaskSettings = SettingsManager.Instance.GetTaskSettings(TaskType);
            SpawnObjects();
            InitializeDefaults();
            Begin();
        }
        
        public void Restart()
        {
            End();
            ResetProgress();
            ResetObjects();
            InitializeDefaults();
            Begin();
        }

        public void Unload()
        {
            ClearSpawnedObjects();
            IsCompleted = true;
        }

        public void Pause()
        {
            Disable();
        }

        public void Continue()
        {
            Enable();
        }
        
        public int GetTaskTimeDuration()
        {
            return TaskSettings.taskTimeDuration;
        }

        public void ResetTaskInfo()
        {
            IsCompleted = false;
            
            ResetProgress();
        }
        
        public void End()
        {
            Disable();
            TaskProgress.TimeDuration = TimeManager.Instance.StopTaskTimer();
        }
        
        protected virtual void IncreaseScore()
        {
            TaskProgress.IncreaseScore();
            OnScoreUpdate?.Invoke(TaskProgress.Score);
        }
        
        /// <summary>
        /// Must be overridden in every child class to define rules that determine when the task is completed to increase score
        /// </summary>
        /// <returns>True if all conditions for scoring are satisfied; otherwise, false.</returns>
        protected virtual bool AreAllObjectsSatisfyConditions()
        {
            return false;
        }
        
        protected void UpdateHint(string newHint)
        {
            OnHintUpdated?.Invoke(newHint);
        }
        
        /// <summary>
        /// Can be overridden in a child class to evaluate task-related variables or perform post-processing logic.
        /// </summary>
        protected virtual void EvaluateTask() { }
        
        /// <summary>
        /// Must be overriden in every child class to spawn the objects required for the task execution.
        /// </summary>
        protected abstract void SpawnObjects();
        
        /// <summary>
        /// Initializes task related variables.
        /// </summary>
        protected virtual void InitializeDefaults() {}
        
        protected static bool IsObjectWatchingUpwards(GameObject go)
        {
            float xRotation = go.transform.rotation.eulerAngles.x;
            float normalizedX = xRotation > 180f ? xRotation - 360f : xRotation;
            return Mathf.Abs(normalizedX - BaseXRotation) <= MinXRotationOffset;
        }
        
        private void Begin()
        {
            TimeManager.Instance.StartTaskTimer(GetTaskTimeDuration());
            Enable();
        }
        
        /// <summary>
        /// Checks if any object is dropped, if should increase score and evaluates task
        /// </summary>
        private void Update()
        {
            CheckIfAnyObjectsDropped();
            
            if (AreAllObjectsSatisfyConditions())
            {
                IncreaseScore();
            }
            else
            {
                EvaluateTask();
            }
        }
        
        private void CheckIfAnyObjectsDropped()
        {
            List<GameObject> listToRemove = new();
            SpawnedObjects.ForEach(o =>
            {
                if (o.CompareTag("Podest") || !o.TryGetComponent(out KinematicGrabbable grabbable)) return;
                if (grabbable.IsHeld || !(o.GetComponent<Renderer>().bounds.min.y - FloorManager.Instance.FloorY < MinOnFloorOffset)) return;
                if (o.TryGetComponent(out Rigidbody rg) && rg.isKinematic) return;
                
                var onTheFloorString = $"Task {Name} - object '{o.tag}' is on the floor";
                Debug.Log(onTheFloorString);
                //LSLSender.SendLsl(onTheFloorString, new float[] { 141 });
                listToRemove.Add(o);
            });
            SpawnedObjects.RemoveAll(o=>listToRemove.Contains(o));
            listToRemove.ForEach(Destroy);
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.StopTaskTimer();
            ClearSpawnedObjects();
        }
        
        private void ClearSpawnedObjects()
        {
            SpawnedObjects.ForEach(Destroy);
            SpawnedObjects.Clear();
        }
        
        private void ResetProgress()
        {
            TaskProgress = new TaskProgress();
            OnScoreUpdate?.Invoke(TaskProgress.Score);
        }

        /// <summary>
        /// Enables <see cref="Update"/>. method
        /// </summary>
        private void Enable()
        {
            enabled = true;
        }
        
        /// <summary>
        /// Disables <see cref="Update"/>. method
        /// </summary>
        private void Disable()
        {
            enabled = false;
        }

        /*protected bool IsObjectOnTable(GameObject go) =>
            Math.Abs(TableManager.Instance.SelectedTable.TopCenter.y - go.GetComponent<Renderer>().bounds.min.y) <=
        MinObjectTableOffset;*/
    }
}