using System;
using Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Tasks.TaskProperties;

namespace UI.Buttons
{
    /// <summary>
    /// Attached to each "task settings" button in the 
    /// <see cref="UI.PanelControllers.TaskSettingsPanelController"/>.
    /// 
    /// When clicked, this button disables itself and triggers a toggle action,
    /// passing itself as the argument.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TaskSettingsToggleButton: MonoBehaviour
    {
        [SerializeField] private ETaskType taskType;
        public ETaskType GetTaskType => taskType;
        public Button Button { get; private set; }
        private Action<TaskSettingsToggleButton> _toggleAction;
        
        private void Awake()
        {
            Button = GetComponent<Button>();
            
            if (Button == null)
            {
                Debug.LogError($"Button not found on {gameObject.name}");
            }
        }

        public void SetToggleAction(Action<TaskSettingsToggleButton> action)
        {
            if (_toggleAction != null) return;
            _toggleAction = action;
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() =>
            {
                _toggleAction?.Invoke(this);
                Button.interactable = false;
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