using System;
using System.Collections;
using UnityEngine;

namespace Managers
{ 
    /// <summary>
    /// Manages task-related time tracking and provides utility methods for time formatting and freezing/unfreezing time.
    /// </summary>
    public class TimeManager : Singleton<TimeManager>
    {
        public static void FreezeTime()
        {
            Time.timeScale = 0;
        }
        
        public static void UnfreezeTime()
        {
            Time.timeScale = 1;
        }
        
        /// <summary>
        /// Converts a given time in seconds to a string formatted as minutes and seconds (mm:ss).
        /// </summary>
        /// <param name="timeSeconds">Time in seconds.</param>
        /// <returns>Formatted time string in "minutes:seconds" format.</returns>
        /// <example>11:59</example>
        public static string MinutesSecondsToString(int timeSeconds)
        {
            int minutes = Mathf.FloorToInt(timeSeconds / 60);
            int seconds = Mathf.FloorToInt(timeSeconds % 60);

            return string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        
        public event Action OnTimeEnded;
        public event Action<string> OnTimeUpdated;
        
        /// <summary>
        /// Time measured for current task from initialization.
        /// </summary>
        private int _currentTaskTime = 0;
        private int _taskTime;
        private Coroutine _taskTimer;

        
        public void StartTaskTimer(int taskTime)
        {
            _currentTaskTime = 0;
            _taskTime = taskTime * 60;
            OnTimeUpdated?.Invoke(MinutesSecondsToString(_taskTime - _currentTaskTime));
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
            
            return _currentTaskTime;
        }

        private IEnumerator TaskTimer()
        {
            while (_currentTaskTime < _taskTime)
            {
                yield return new WaitForSeconds(1);
                _currentTaskTime += 1;
                OnTimeUpdated?.Invoke(MinutesSecondsToString(_taskTime - _currentTaskTime));
            }
            OnTimeEnded?.Invoke();
            _taskTimer = null;
        }
    }
}
