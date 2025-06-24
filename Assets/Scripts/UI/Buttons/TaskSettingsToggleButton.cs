using System;
using Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace UI.Buttons
{
    [RequireComponent(typeof(Button))]
    public class TaskSettingsToggleButton: MonoBehaviour
    {
        [SerializeField] private ETaskType taskType;
        public ETaskType GetTaskType => taskType;
        public Button button { get; private set; }
        private Action<TaskSettingsToggleButton> _toggleAction;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError($"Button not found on {gameObject.name}");
            }
        }

        public void SetToggleAction(Action<TaskSettingsToggleButton> action)
        {
            if (_toggleAction != null) return;
            _toggleAction = action;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                _toggleAction?.Invoke(this);
                button.interactable = false;
            });
        }


#if UNITY_EDITOR
    private void OnValidate()
    {
        var allTaskSettingsButtons = FindObjectsByType<TaskSettingsToggleButton>(FindObjectsInactive.Include ,FindObjectsSortMode.None);
        foreach (var button in allTaskSettingsButtons)
        {
            if (button != this && button.taskType == this.taskType)
            {
                Debug.LogError($"Duplicate task type button: {taskType} already used on {button.gameObject.name}", this);
                break;
            }
        }
    }
#endif
    }
}