using System;
using System.Collections.Generic;
using LearnXR.Core.Utilities;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.PanelControllers
{
    /// <summary>
    /// Controls the UI panel of main menu of the main canvas
    /// Responsible for managing scripts.
    /// </summary>
    [RequireComponent(typeof(UIPanel))]
    public class MainMenuPanelController: MonoBehaviour
    {
        [SerializeField] private Button startButton;
        
        private void OnStartButtonClick()
        {
            if (!TableManager.Instance.IsInit || !ReachAreaManager.Instance.IsInit)
            {
                UIManager.Instance.GetMainCanvasController.OpenTableSettings();
                return;
            }

            UIManager.Instance.ShowTaskCanvas();
            GameManager.Instance.StartSession();
        }

        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var uiPanel = GetComponent<UIPanel>();
            if (uiPanel.PanelType != EPanelType.MainMenu)
            {
                Debug.LogError($"This script can be attached only to a UIPanel with PanelType: EPanelType.MainMenu, but this is {uiPanel.PanelType}", this);
            }
        }
#endif
    }
}