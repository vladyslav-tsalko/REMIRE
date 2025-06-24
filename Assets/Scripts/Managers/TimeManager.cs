using System;
using System.Collections;
using UnityEngine;

// Responsible for all time related events such as starting and ending task timer and updating timer display in session.
// Timer must be initialized by external class to start counting for current task.
// Triggers event when task time is completed unless stopped earlier which can happen if user ends task prematurely.
public class TimeManager : Singleton<TimeManager>
{
    // converts time to minutes and seconds format like so: "11:59"
    public static string MinutesSecondsToString(int timeSeconds)
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60);
        int seconds = Mathf.FloorToInt(timeSeconds % 60);

        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    [Tooltip("Time measured for current task from initialization.")]
    public int CurrentTaskTime { get; private set; } = 0;

    public static void FreezeTime()
    {
        Time.timeScale = 0;
    }
    
    public static void UnfreezeTime()
    {
        Time.timeScale = 1;
    }

    private int _taskTime;
    private Coroutine _taskTimer;
    public event Action OnTimeEnded;
    public event Action<string> OnTimeUpdated;
    
    public void StartTaskTimer(int taskTime)
    {
        CurrentTaskTime = 0;
        _taskTime = taskTime * 60;
        OnTimeUpdated?.Invoke(MinutesSecondsToString(_taskTime - CurrentTaskTime));
        _taskTimer = StartCoroutine(TaskTimer());
    }

    // returns the task duration
    public int StopTaskTimer()
    {
        if (_taskTimer != null)
        {
            StopCoroutine(_taskTimer);
            _taskTimer = null;
        }
        
        return CurrentTaskTime;
    }

    private IEnumerator TaskTimer()
    {
        while (CurrentTaskTime < _taskTime)
        {
            yield return new WaitForSeconds(1);
            CurrentTaskTime += 1;
            OnTimeUpdated?.Invoke(MinutesSecondsToString(_taskTime - CurrentTaskTime));
        }

        // trigger time ended event to stop the task
        OnTimeEnded?.Invoke();
        
        _taskTimer = null;
    }
}