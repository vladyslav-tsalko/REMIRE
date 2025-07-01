using System;
using System.Collections.Generic;
using Managers;
using Tasks;
using TMPro;
using UI.Buttons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Tasks.TaskProperties;

namespace UI.PanelControllers
{    
    /// <summary>
    /// Controls the UI panel for task settings of main canvas.
    /// Manages tasks settings for each task independently.
    /// Allows save or cancel settings.
    /// </summary>
    [RequireComponent(typeof(UIPanel))]
    public class TaskSettingsPanelController: MonoBehaviour
    {
        [SerializeField] private List<TaskSettingsToggleButton> taskSettingsToggleButtons;
        [SerializeField] private Button reduceTimeButton;
        [SerializeField] private Button increaseTimeButton;
        [SerializeField] private Button difficultyButton;
        [SerializeField] private TextMeshProUGUI taskTimeDurationTMP;

        private const int MAX_TIME_DURATION = 10;
        private const int MIN_TIME_DURATION = 1;
        
        private TextMeshProUGUI _taskDifficultyTMP;
        private Dictionary<ETaskType, TaskSettings> _taskSettingsMap = new();
        private TaskSettingsToggleButton _activeButton;
        private TaskSettings _currentTaskSettings;

        private void FillTaskSettingsMapFromSettingsManager()
        {
            _taskSettingsMap.Clear();
            foreach (var pair in SettingsManager.Instance.TaskSettingsMap)
            {
                _taskSettingsMap[pair.Key] = new TaskSettings(pair.Value);
            }
        }

        private void Initialize()
        {
            _taskDifficultyTMP = difficultyButton.GetComponentInChildren<TextMeshProUGUI>(true);

            FillTaskSettingsMapFromSettingsManager();

            taskSettingsToggleButtons.ForEach(btn =>
            {
                btn.SetToggleAction(ToggleAction);
            });
            ToggleAction(taskSettingsToggleButtons[0]);
            
            difficultyButton.onClick.RemoveAllListeners();
            difficultyButton.onClick.AddListener(OnDifficultyButtonPress);
            
            reduceTimeButton.onClick.RemoveAllListeners();
            reduceTimeButton.onClick.AddListener(OnReduceTimeButton);
            
            increaseTimeButton.onClick.RemoveAllListeners();
            increaseTimeButton.onClick.AddListener(OnIncreaseTimeButton);
        }
        
        private void RefreshCurrentTaskSettings()
        {
            _currentTaskSettings = _taskSettingsMap[_activeButton.GetTaskType];
            ShowTaskInfoBasedOnActiveButton();
        }

        #region ON_PRESS_BUTTONS_MANIPULATE_CHANGED_DATA

        public void SaveSettings()
        {
            SettingsManager.Instance.UpdateSettings(_taskSettingsMap);
        }

        public void ResetSettings()
        {
            FillTaskSettingsMapFromSettingsManager();
            RefreshCurrentTaskSettings();
        }

        #endregion
        
        

        #region ON_PRESS_BUTTONS_CHANGE_VALUES

        private void OnDifficultyButtonPress()
        {
            int currentDifficultyInt = (int)_currentTaskSettings.difficulty;
            currentDifficultyInt++;
            if (Enum.IsDefined(typeof(Difficulty), currentDifficultyInt))
            {
                _currentTaskSettings.difficulty = (Difficulty)currentDifficultyInt;
            }
            else
            {
                _currentTaskSettings.difficulty = Difficulty.Easy;
            }

            UpdateDifficultyLabel();
        }

        private void OnIncreaseTimeButton()
        {
            if (_currentTaskSettings.taskTimeDuration == MAX_TIME_DURATION) return;
            _currentTaskSettings.taskTimeDuration++;
            UpdateTaskTimeDurationLabel();
        }
        
        private void OnReduceTimeButton()
        {
            if (_currentTaskSettings.taskTimeDuration == MIN_TIME_DURATION) return;
            _currentTaskSettings.taskTimeDuration--;
            UpdateTaskTimeDurationLabel();
        }

        #endregion
        

        private void ToggleAction(TaskSettingsToggleButton btn)
        {
            if(_activeButton) _activeButton.Button.interactable = true;
            _activeButton = btn;
            _activeButton.Button.interactable = false;
            RefreshCurrentTaskSettings();
        }

        #region UPDATE_LABELS
        
        private void UpdateDifficultyLabel()
        {
            if (!_taskDifficultyTMP) return;
            _taskDifficultyTMP.text = _currentTaskSettings.difficulty.ToString();
        }
        
        private void UpdateTaskTimeDurationLabel()
        {
            if (!taskTimeDurationTMP) return;
            taskTimeDurationTMP.text = _currentTaskSettings.taskTimeDuration.ToString();
        }

        private void ShowTaskInfoBasedOnActiveButton()
        {
            UpdateDifficultyLabel();
            UpdateTaskTimeDurationLabel();
        }
        
        #endregion
        

        private void Start()
        {
            if (SettingsManager.Instance == null)
            {
                SettingsManager.OnInstanceCreated += Initialize;
            }
            else
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            SettingsManager.OnInstanceCreated -= Initialize;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            var uiPanel = GetComponent<UIPanel>();
            if (uiPanel.PanelType != EPanelType.TaskSettings)
            {
                Debug.LogError($"This script can be attached only to a UIPanel with PanelType: EPanelType.TaskSettings, but this is {uiPanel.PanelType}", this);
            }
        }
#endif
    }
}