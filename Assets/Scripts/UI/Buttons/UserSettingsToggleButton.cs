using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace UI.Buttons
{
    /// <summary>
    /// Attached to each "user settings" button in the 
    /// <see cref="UI.PanelControllers.SettingsPanelController"/>.
    /// 
    /// When clicked, this button invokes a toggle function that returns a boolean.
    /// The button's label is updated based on the returned value.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UserSettingsToggleButton: MonoBehaviour
    {
        private TextMeshProUGUI _buttonLabel;
        private Button _button;
        private Func<bool> _toggleFunction;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonLabel = GetComponentInChildren<TextMeshProUGUI>(true);
            
            if (_button == null || _buttonLabel == null)
            {
                Debug.LogError($"Button or TextMeshProUGUI not found on {gameObject.name}");
            }
        }

        public void SetToggleAction(Func<bool> function, bool initialValue)
        {
            if (_toggleFunction != null) return;
            _toggleFunction = function;

            UpdateButtonLabel(initialValue);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                var isToggled = _toggleFunction?.Invoke() ?? false;
                UpdateButtonLabel(isToggled);
            });
        }

        private void UpdateButtonLabel(bool isToggled)
        {
            if (!_buttonLabel) return;
            _buttonLabel.text = isToggled ? "TRUE" : "FALSE";
        }
    }
}