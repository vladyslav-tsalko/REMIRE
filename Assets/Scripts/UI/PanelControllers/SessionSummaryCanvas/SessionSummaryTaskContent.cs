using Tasks;
using TMPro;
using UnityEngine;

namespace UI.Panels.SessionSummaryPanelControllers
{
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