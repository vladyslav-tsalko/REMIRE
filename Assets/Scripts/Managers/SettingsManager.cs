using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Tasks;
using UnityEngine;
using Tasks.TaskProperties;

namespace Managers
{
    public class SettingsManager: Singleton<SettingsManager>
    {
        public class UserSettings
        {
            private bool randomTasks = false;
            private bool leftHanded = false;

            public bool RandomTasks
            {
                get => randomTasks;
                internal set => randomTasks = value;
            }
            
            public bool LeftHanded
            {
                get => leftHanded;
                internal set => leftHanded = value;
            }
        }

        private UserSettings settings = null;
        public UserSettings Settings { get => settings; private set => settings = value; }
        private readonly Dictionary<ETaskType, TaskSettings> _taskSettingsMap = new();
        public IReadOnlyDictionary<ETaskType, TaskSettings> TaskSettingsMap => _taskSettingsMap;

        /// <summary>
        /// Gets task settings based on inheritance of TaskType.
        /// This should be treated as read-only, do not modify the contents.
        /// </summary>
        public TaskSettings GetTaskSettings(ETaskType taskType)
        {
            return TaskSettingsMap[taskType];
        }

        public static event Action OnInstanceCreated;
        
        #region JSON STRINGS
        private readonly string _userSettingsJson = "settings";
        #endregion
        
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

        void LoadSettingsFromSystem()
        {
            foreach (ETaskType taskType in Enum.GetValues(typeof(ETaskType)))
            {
                _taskSettingsMap[taskType] = LoadTaskSettingsFromSystem(taskType);
            }
            
            // load user settings
            if (PlayerPrefs.HasKey(_userSettingsJson))
            {
                Settings = JsonUtility.FromJson<UserSettings>(PlayerPrefs.GetString(_userSettingsJson));
                if(settings == null)
                {
                    settings = new UserSettings();
                    var json2 = JsonUtility.ToJson(settings);
                    PlayerPrefs.SetString(_userSettingsJson, json2);
                }
            }
            else
            {
                // load default settings
                Settings = new UserSettings();
            }
        }
        
        TaskSettings LoadTaskSettingsFromSystem(ETaskType taskType)
        {
            return PlayerPrefs.HasKey(taskType.ToString()) ? 
                JsonUtility.FromJson<TaskSettings>(PlayerPrefs.GetString(taskType.ToString())):
                new TaskSettings();
            /*if (PlayerPrefs.HasKey(taskType.ToString()))
            {
                return JsonUtility.FromJson<TaskSettings>(PlayerPrefs.GetString(taskType.ToString()));
            }
            else
            {
                // load default settings
                return new TaskSettings();
            }*/
        }

        public void UpdateSettings(Dictionary<ETaskType, TaskSettings> newTaskSettingsMap)
        {
            var taskSettingsKeys = newTaskSettingsMap.Keys;
            foreach (var key in taskSettingsKeys)
            {
                _taskSettingsMap[key] = new TaskSettings(newTaskSettingsMap[key]);
            }

            SaveTaskSettingsIntoSystem();
        }

        void SaveTaskSettingsIntoSystem()
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

        void SaveUserSettingsIntoSystem()
        {
            var userJson = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(_userSettingsJson, userJson);
        }

        void SaveSettingsIntoSystem()
        {
            SaveTaskSettingsIntoSystem();
            SaveUserSettingsIntoSystem();
        }

        public bool ToggleLeftHanded()
        {
            settings.LeftHanded = !settings.LeftHanded;
            SaveUserSettingsIntoSystem();
            return settings.LeftHanded;
        }

        public bool ToggleRandomTasks()
        {
            settings.RandomTasks = !settings.RandomTasks;
            SaveUserSettingsIntoSystem();
            return settings.RandomTasks;
        }
        
    }
}