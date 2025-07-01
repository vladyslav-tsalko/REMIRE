using System;
using System.Collections.Generic;
using LearnXR.Core.Utilities;
using Meta.XR.MRUtilityKit;
using Tasks;
using UnityEngine;
using Tasks.TaskProperties;

namespace Managers
{
    /// <summary>
    /// Manages loading, modifying, and saving user and task-related settings.
    /// </summary>
    public class SettingsManager: Singleton<SettingsManager>
    {
        private class UserSettings
        {
            public bool RandomTasks;

            public bool LeftHanded;
        }

        public static event Action OnInstanceCreated;
        private const string UserSettingsJson = "settings";
        
        
        private readonly Dictionary<ETaskType, TaskSettings> _taskSettingsMap = new();
        public IReadOnlyDictionary<ETaskType, TaskSettings> TaskSettingsMap => _taskSettingsMap;

        public bool IsLeftHanded => _settings.LeftHanded;
        public bool IsRandomTasks => _settings.RandomTasks;

        private UserSettings _settings;

        /// <summary>
        /// Gets task settings based on inheritance of TaskType.
        /// This should be treated as read-only, do not modify the contents.
        /// </summary>
        public TaskSettings GetTaskSettings(ETaskType taskType) => TaskSettingsMap[taskType];
        
        /// <summary>
        /// Updates existing task-related settings
        /// </summary>
        /// <param name="newTaskSettingsMap">New task-related settings</param>
        public void UpdateSettings(Dictionary<ETaskType, TaskSettings> newTaskSettingsMap)
        {
            var taskSettingsKeys = newTaskSettingsMap.Keys;
            foreach (var key in taskSettingsKeys)
            {
                _taskSettingsMap[key] = new TaskSettings(newTaskSettingsMap[key]);
            }

            SaveTaskSettingsIntoSystem();
        }
        
        public bool ToggleLeftHanded()
        {
            _settings.LeftHanded = !_settings.LeftHanded;
            SaveUserSettingsIntoSystem();
            return _settings.LeftHanded;
        }

        public bool ToggleRandomTasks()
        {
            _settings.RandomTasks = !_settings.RandomTasks;
            SaveUserSettingsIntoSystem();
            return _settings.RandomTasks;
        }
        
        protected override void Awake()
        {
            base.Awake();
            LoadSettingsFromSystem();
            OnInstanceCreated?.Invoke();
        }

        private void OnDestroy()
        {
            SaveSettingsIntoSystem();
        }

        private void LoadSettingsFromSystem()
        {
            foreach (ETaskType taskType in Enum.GetValues(typeof(ETaskType)))
            {
                _taskSettingsMap[taskType] = LoadTaskSettingsFromSystem(taskType);
            }
            
            if (PlayerPrefs.HasKey(UserSettingsJson))
            {
                string json = PlayerPrefs.GetString(UserSettingsJson);
                _settings = JsonUtility.FromJson<UserSettings>(json);
            }
            else
            {
                _settings = new UserSettings();
                string json = JsonUtility.ToJson(_settings);
                PlayerPrefs.SetString(UserSettingsJson, json);
            }
        }
        
        private TaskSettings LoadTaskSettingsFromSystem(ETaskType taskType)
        {
            return PlayerPrefs.HasKey(taskType.ToString()) ? 
                JsonUtility.FromJson<TaskSettings>(PlayerPrefs.GetString(taskType.ToString())):
                new TaskSettings();
        }
        
        private void SaveSettingsIntoSystem()
        {
            SaveTaskSettingsIntoSystem();
            SaveUserSettingsIntoSystem();
        }
        
        private void SaveTaskSettingsIntoSystem()
        {
            foreach (ETaskType taskType in _taskSettingsMap.Keys)
            {
                if (_taskSettingsMap[taskType] != null)
                {
                    var taskJson = JsonUtility.ToJson(_taskSettingsMap[taskType]);
                    PlayerPrefs.SetString(taskType.ToString(), taskJson);
                }
            }
        }

        private void SaveUserSettingsIntoSystem()
        {
            var userJson = JsonUtility.ToJson(_settings);
            PlayerPrefs.SetString(UserSettingsJson, userJson);
        }
    }
}