using System;
using System.Collections.Generic;
using Hands.Grabbables;
using Managers;
using UnityEngine;
using Tasks.TaskProperties;

namespace Tasks
{
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
        
        public void Load()
        {
            TaskSettings = SettingsManager.Instance.GetTaskSettings(TaskType);
            SpawnObjects();
            
            Begin();
        }

        public void ResetObjects()
        {
            HandsManager.Instance.KinematicGrabberLeft.ReleaseObject();
            HandsManager.Instance.KinematicGrabberRight.ReleaseObject();
            ClearSpawnedObjects();
            SpawnObjects();
            UpdateHint(TaskDescription);
        }
        
        public void Restart()
        {
            End();
            ResetProgress();
            ResetObjects();
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

        protected virtual void IncreaseScore()
        {
            TaskProgress.IncreaseScore();
            OnScoreUpdate?.Invoke(TaskProgress.Score);
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
        
        private void Begin()
        {
            TimeManager.Instance.StartTaskTimer(GetTaskTimeDuration());
            Enable();
        }

        public void End()
        {
            Disable();
            TaskProgress.TimeDuration = TimeManager.Instance.StopTaskTimer();
        }

        protected virtual bool AreAllObjectsSatisfyConditions()
        {
            return false;
        }
        
        protected void Update()
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

        protected void UpdateHint(string newHint)
        {
            OnHintUpdated?.Invoke(newHint);
        }

        protected virtual void EvaluateTask() { }

        protected abstract void SpawnObjects();
        
        protected static bool IsObjectWatchingUpwards(GameObject go)
        {
            float xRotation = go.transform.rotation.eulerAngles.x;
            float normalizedX = xRotation > 180f ? xRotation - 360f : xRotation;
            return Mathf.Abs(normalizedX - BaseXRotation) <= MinXRotationOffset;
        }
        
        void CheckIfAnyObjectsDropped()
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

        private void Enable()
        {
            enabled = true;
        }

        private void Disable()
        {
            enabled = false;
        }

        protected bool IsObjectOnTable(GameObject go)
        {
            return Math.Abs(TableManager.Instance.SelectedTable.TopCenter.y - go.GetComponent<Renderer>().bounds.min.y) <=
                   MinObjectTableOffset;
        }
        
        //TODO: manage all namespace names
    }
}