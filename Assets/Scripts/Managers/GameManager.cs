using System;
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using Managers;
using Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Tasks.TaskProperties;

namespace Managers
{
    /// <summary>
    /// Main manager that is responsible for switching between tasks and saving progress.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private List<Task> tasks = new();
        
        private Task _currentTask;
        
        #region Events
        
        public static event Action OnInstanceCreated;
        public event Action<string, int, string> OnNewTask;
        public event Action <Dictionary<ETaskType, TaskProgress>> OnTaskEnded;
        
        #endregion

        
        #region Checkers

        public bool IsSessionCompleted => tasks.All(task => task.IsCompleted);
        public bool IsSessionInProgress => _currentTask;

        #endregion
        
        
        #region Session
        
        public void StartSession()
        {
            LoadNextTask();
        }
        
        public void PauseSession()
        {
            TimeManager.FreezeTime();
            _currentTask.Pause();
        }

        public void ResumeSession()
        {
            _currentTask.Continue();
            TimeManager.UnfreezeTime();
        }
        
        public void EndSession()
        {
            TimeManager.UnfreezeTime();
            if (!_currentTask.IsCompleted)
            {
                _currentTask.End();
                _currentTask.Unload();
            }
            _currentTask = null;
            tasks.ForEach(task => task.ResetTaskInfo());
        }
        
        #endregion
        
        
        #region CurrentTask
        
        public void ResetObjectsCurrentTask()
        {
            if (!_currentTask) return;
            _currentTask.ResetObjects();
        }

        public void RestartCurrentTask()
        {
            if (!_currentTask) return;
            _currentTask.Restart();
            TimeManager.UnfreezeTime();
        }

        public void EndCurrentTask()
        {
            _currentTask.End();
            Dictionary<ETaskType, TaskProgress> taskProgresses = new();
            foreach (var task in tasks)
            {
                taskProgresses[task.TaskType] = task.TaskProgress;
            }
            OnTaskEnded?.Invoke(taskProgresses);
            UIManager.Instance.ShowSummaryCanvas();
        }

        public void UnloadCurrentTask()
        {
            _currentTask.Unload();
        }
        
        private void LoadCurrentTask()
        {
            _currentTask.Load();
            OnNewTask?.Invoke(_currentTask.Name, _currentTask.GetTaskTimeDuration(), _currentTask.TaskDescription);
            SpatialLogger.Instance.LogInfo($"Start task");
            TimeManager.UnfreezeTime();
        }
        
        #endregion

        public void LoadNextTask()
        {
            if (SettingsManager.Instance.IsRandomTasks) //handle random task
            {
                var rnd = new System.Random(DateTime.Now.Millisecond);
                int rndTask;
                do
                {
                    rndTask = rnd.Next(0, tasks.Count);
                    if (IsSessionCompleted)
                    {
                        EndSession();
                        return;
                    }
                } while (tasks[rndTask].IsCompleted);

                if (_currentTask)
                {
                    UnloadCurrentTask();
                }

                _currentTask = tasks[rndTask];
            }
            else if (!_currentTask) // load first task if no task active
            {
                _currentTask = tasks[0];
            }
            else // else load next task in order
            {
                UnloadCurrentTask();
                if (IsSessionCompleted)
                {
                    EndSession();
                    return;
                }

                _currentTask = tasks[tasks.IndexOf(_currentTask) + 1];
            }
            LoadCurrentTask();
        }
        
        protected override void Awake()
        {
            base.Awake();
            OnInstanceCreated?.Invoke();
        }

        private void Start()
        {
            TimeManager.Instance.OnTimeEnded += EndCurrentTask;
            tasks.ForEach(task =>
            {
                task.enabled = false;
            });
        }

        private void OnDestroy()
        {
            TimeManager.Instance.OnTimeEnded -= EndCurrentTask;
        }
    }
}

