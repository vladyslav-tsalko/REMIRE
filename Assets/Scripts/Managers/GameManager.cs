using System;
using System.Collections.Generic;
using LearnXR.Core.Utilities;
using Managers;
using Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private List<Task> tasks = new();
    //public event Action OnSessionStarted;
    //public event Action OnSessionEnded;
    public event Action<string, int, string> OnNewTask;
    
    public static event Action OnInstanceCreated;

    public event Action <Dictionary<ETaskType, TaskProgress>> OnTaskEnded;
    private bool IsRandomTask => SettingsManager.Instance.Settings.RandomTasks;

    private Task _currentTask;

    protected override void Awake()
    {
        base.Awake();
        OnInstanceCreated?.Invoke();
    }

    void Start()
    {
        TimeManager.Instance.OnTimeEnded += EndCurrentTask;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.OnTimeEnded -= EndCurrentTask;
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

    public bool IsLastTask()
    {
        foreach (var task in tasks)
        {
            if (!task.IsCompleted) return false;
        }

        return true;
    }

    public void CurrentTaskResetObjects()
    {
        if (!_currentTask) return;
        _currentTask.ResetObjects();
    }

    public void CurrentTaskRestart()
    {
        if (!_currentTask) return;
        _currentTask.Restart();
        TimeManager.UnfreezeTime();
    }

    public void StartSession()
    {
        //OnSessionStarted?.Invoke();
        LoadNextTask();
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
        //OnSessionEnded?.Invoke();
    }

    private void LoadTask()
    {
        _currentTask.Load();
        //SpatialLogger.Instance.LogInfo($"{_currentTask.Name}");
        OnNewTask?.Invoke(_currentTask.Name, _currentTask.GetTaskTimeDuration(), _currentTask.TaskDescription);
        SpatialLogger.Instance.LogInfo($"Start task");
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

    public void UnloadTask()
    {
        _currentTask.Unload();
    }

    public void LoadNextTask()
    {
        if (IsRandomTask) //handle random task
        {
            var rnd = new System.Random(DateTime.Now.Millisecond);
            int rndTask;
            do
            {
                rndTask = rnd.Next(0, tasks.Count);
                if (IsLastTask())
                {
                    EndSession();
                    return;
                }
            } while (tasks[rndTask].IsCompleted);

            if (_currentTask)
            {
                UnloadTask();
            }

            _currentTask = tasks[rndTask];
        }
        else if (!_currentTask) // load first task if no task active
        {
            _currentTask = tasks[0];
        }
        else // else load next task in order
        {
            UnloadTask();
            if (IsLastTask())
            {
                EndSession();
                return;
            }

            _currentTask = tasks[tasks.IndexOf(_currentTask) + 1];
        }
        LoadTask();
    }

    public bool IsSessionInProgress()
    {
        return _currentTask;
    }
}
