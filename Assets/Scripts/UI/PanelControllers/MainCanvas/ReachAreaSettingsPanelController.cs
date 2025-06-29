using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelControllers
{
    [RequireComponent(typeof(UIPanel))]
    public class ReachAreaSettingsPanelController: MonoBehaviour
    {
        public static Action OnTimerCompleted;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI hintTMProUGUI;
        [SerializeField] private TextMeshProUGUI timerTMProUGUI;
        
        private const int TimeDuration = 5;
        private const string PutHands = "Put hands on the table as far as you can";
        private const string KeepHands = "Keep hands on the table";
        private const string SpreadHands = "Spread your hands";

        private bool _isStarted = false;
        private float _currentTime = 0f;
        private const float MinHandsDistance = 0.45f;


        private void SetHintText(string newHint)
        {
            if(hintTMProUGUI.text != newHint) hintTMProUGUI.text = newHint;
        }
        
        private void UpdateTimerText()
        {
            string newTimeRemaining = _currentTime.ToString("00.00");
            if(timerTMProUGUI.text != newTimeRemaining) timerTMProUGUI.text = newTimeRemaining;
        }
        
        void Start()
        {
            startButton.onClick.AddListener(StartPositioning);
            cancelButton.onClick.AddListener(CancelPositioning);
            cancelButton.onClick.AddListener(() => ReachAreaManager.Instance.enabled = false);
            _currentTime = TimeDuration;
            //SpatialLogger.Instance.LogInfo($"Top center: {TableManager.Instance.SelectedTable.TopCenter}");
        }

        void OnEnable()
        {
            SetHintText(ReachAreaManager.Instance.IsInit
                ? "Press start button to set hands"
                : "Press start button to change hands");
        }

        void Update()
        {
            if (!_isStarted) return;
            HandsManager handsManager = HandsManager.Instance;
            
            if (!handsManager.KinematicGrabberLeft.IsActive)
            {
                SetHintText("Left hand is not tracked");
                return;
            }

            if (!handsManager.KinematicGrabberRight.IsActive)
            {
                SetHintText("Right hand is not tracked");
                return;
            }
            var selectedTable = TableManager.Instance.SelectedTable;
            
            Vector3 leftHandWPos = handsManager.KinematicGrabberLeft.GetWorldPos();
            Vector3 rightHandWPos = handsManager.KinematicGrabberRight.GetWorldPos();
            
            
            if (selectedTable != null && selectedTable.IsPositionOnTable(leftHandWPos) && selectedTable.IsPositionOnTable(rightHandWPos))
            {
                if (Vector3.Distance(leftHandWPos, rightHandWPos) < MinHandsDistance)
                {
                    SetHintText(SpreadHands);
                    ResetTimer();
                }
                else
                {
                    _currentTime -= Time.deltaTime;
                    UpdateTimerText();

                    if (_currentTime <= 0f)
                    {
                        TimerCompleted();
                    }
                    SetHintText(KeepHands);
                }
            }
            else
            {
                ResetTimer();
                SetHintText(PutHands);
            }
        }

        private void ResetTimer()
        {
            if (_currentTime == TimeDuration) return;
            
            _currentTime = TimeDuration;
            UpdateTimerText();
        }

        private void TimerCompleted()
        {
            CancelPositioning();
            OnTimerCompleted?.Invoke();
            SetHintText("Press start button to change hands");
        }
        
        private void StartPositioning()
        {
            _isStarted = true;
            startButton.interactable = false;
            SetHintText(PutHands);
        }

        private void CancelPositioning()
        {
            _isStarted = false;
            ResetTimer();
            startButton.interactable = true;
        }





#if UNITY_EDITOR
        private void OnValidate()
        {
            var uiPanel = GetComponent<UIPanel>();
            if (uiPanel.PanelType != EPanelType.ReachAreaSettings)
            {
                Debug.LogError($"This script can be attached only to a UIPanel with PanelType: EPanelType.ReachAreaSettings, but this is {uiPanel.PanelType}", this);
            }
        }
#endif
        
    }
}