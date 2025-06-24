using System;
using System.Collections.Generic;
using Managers;
using Tasks;
using TMPro;
using UI.Buttons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    [RequireComponent(typeof(UIPanel))]
    public class TableSettingsPanelController: MonoBehaviour
    {
        [SerializeField] private Button selectTableButton;
        [SerializeField] private Button setReachAreaButton;
        [SerializeField] private TextMeshProUGUI infoTextMeshProUGUI;

        void OnSelectTablePress()
        {
            TableManager.Instance.StartTableSelecting();
            SetText("Set your max reach area!");
            setReachAreaButton.interactable = true;
        }

        void OnSetReachAreaPress()
        {
            ReachAreaManager.Instance.enabled = true;
        }

        void SetText(string newMessage)
        {
            infoTextMeshProUGUI.text = newMessage;
        }

        void Start()
        {
            SetText("Select a table!");
            setReachAreaButton.interactable = false;
            
            setReachAreaButton.onClick.AddListener(OnSetReachAreaPress);
            selectTableButton.onClick.AddListener(OnSelectTablePress);
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            var uiPanel = GetComponent<UIPanel>();
            if (uiPanel.PanelType != EPanelType.TableSettings)
            {
                Debug.LogError($"This script can be attached only to a UIPanel with PanelType: EPanelType.TableSettings, but this is {uiPanel.PanelType}", this);
            }
        }
#endif
        
    }
}