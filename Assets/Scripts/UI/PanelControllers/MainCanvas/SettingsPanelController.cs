using Managers;
using TMPro;
using UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelControllers
{
    /// <summary>
    /// Controls the UI panel for settings of the main canvas.
    /// Manages user settings in the settings panel of the main canvas.
    /// </summary>
    [RequireComponent(typeof(UIPanel))]
    public class SettingsPanelController: MonoBehaviour
    {
        [SerializeField] private UserSettingsToggleButton randomnessButton;
        [SerializeField] private UserSettingsToggleButton leftHandedButton;

        private void ConfigureButtons()
        {
            randomnessButton.SetToggleAction(SettingsManager.Instance.ToggleRandomTasks, SettingsManager.Instance.IsRandomTasks);
            leftHandedButton.SetToggleAction(SettingsManager.Instance.ToggleLeftHanded, SettingsManager.Instance.IsLeftHanded);
        }

        
        private void Start()
        {
            if (SettingsManager.Instance == null)
            {
                SettingsManager.OnInstanceCreated += ConfigureButtons;
            }
            else
            {
                ConfigureButtons();
            }
        }

        private void OnDestroy()
        {
            SettingsManager.OnInstanceCreated -= ConfigureButtons;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            var uiPanel = GetComponent<UIPanel>();
            if (uiPanel.PanelType != EPanelType.Settings)
            {
                Debug.LogError($"This script can be attached only to a UIPanel with PanelType: EPanelType.Settings, but this is {uiPanel.PanelType}", this);
            }
        }
#endif
    }
}