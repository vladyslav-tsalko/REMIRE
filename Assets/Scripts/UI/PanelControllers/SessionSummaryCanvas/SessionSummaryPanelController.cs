using System;
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using Managers;
using Tasks;
using TMPro;
using UI.PanelControllers;
using UnityEngine;
using UnityEngine.UI;
using Tasks.TaskProperties;

namespace UI.PanelControllers
{
    /// <summary>
    /// Controls the UI panel for session summary of session summary canvas.
    /// Manages task statistics.
    /// Allows restart task, start next task or return to main menu.
    /// </summary>
    [RequireComponent(typeof(UIPanel))]
    public class SessionSummaryPanelController: MonoBehaviour
    {
        private readonly List<SessionSummaryTaskContent> _taskContentPanels = new();

        public void OnNextTaskButtonClick()
        {
            GameManager.Instance.LoadNextTask();
            if (GameManager.Instance.IsSessionInProgress)
            {
                UIManager.Instance.ShowTaskCanvas();
            }
            else
            {
                UIManager.Instance.ShowMainCanvas();
            }
        }

        public void OnBackToMenuButtonClick()
        {
            GameManager.Instance.UnloadCurrentTask();
            if (GameManager.Instance.IsSessionCompleted)
            {
                GameManager.Instance.EndSession();
            }
            UIManager.Instance.ShowMainCanvas();
        }

        public void OnRestartTaskButtonClick()
        {
            GameManager.Instance.RestartCurrentTask();
            UIManager.Instance.ShowTaskCanvas();
        }

        void UpdateSessionSummary(Dictionary<ETaskType, TaskProgress> taskProgresses)
        {
            _taskContentPanels.ForEach(contentPanel =>
            {
                contentPanel.SetScore(taskProgresses[contentPanel.ContentTaskType].Score);
                contentPanel.SetTimeDuration(taskProgresses[contentPanel.ContentTaskType].TimeDuration);
            });
        }

        void BindOnTaskEnded()
        {
            GameManager.Instance.OnTaskEnded += UpdateSessionSummary;
        }
    
        void Awake()
        {
            //SpatialLogger.Instance.LogInfo("Awaken");
            if (GameManager.Instance == null)
            {
                GameManager.OnInstanceCreated += BindOnTaskEnded;
            }
            else
            {
                BindOnTaskEnded();
            }
            
            _taskContentPanels.AddRange(
                GetComponentsInChildren<SessionSummaryTaskContent>(true)
            );
        }
    
        private void OnDestroy()
        {
            GameManager.Instance.OnTaskEnded -= UpdateSessionSummary;
        }
    }
}
