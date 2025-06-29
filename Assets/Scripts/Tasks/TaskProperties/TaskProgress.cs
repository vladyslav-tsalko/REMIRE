namespace Tasks.TaskProperties
{
    public class TaskProgress
    {
        /// <summary>
        /// Current number of successfully completed repetitions
        /// </summary>
        public int Score { get; private set; } = 0;

        /// <summary>
        /// Current time duration in seconds
        /// </summary>
        public int TimeDuration = 0;
        
        public TaskProgress()
        {
            Score = 0;
            TimeDuration = 0;
        }

        public void IncreaseScore()
        {
            Score++;
        }
    }
}