using Tasks;
using TMPro;
using UnityEngine;
using Managers;
using Tasks.TaskProperties;

namespace UI.PanelControllers
{
    /// <summary>
    /// Represents info about separate task on the session summary panel of session summary canvas
    /// </summary>
    public class SessionSummaryTaskContent: MonoBehaviour
    {
        [SerializeField] public ETaskType ContentTaskType;
        [SerializeField] private TextMeshProUGUI Score;
        [SerializeField] private TextMeshProUGUI Time;

        public void SetScore(int newScore)
        {
            Score.text = newScore.ToString();
        }
        
        public void SetTimeDuration(int newTime)
        {
            Time.text = TimeManager.MinutesSecondsToString(newTime);
        }
    }
}