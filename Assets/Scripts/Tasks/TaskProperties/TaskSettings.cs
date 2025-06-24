namespace Tasks
{
    [System.Serializable]
    public class TaskSettings
    {
        /// <summary>
        /// Time duration in minutes
        /// </summary>
        public int taskTimeDuration;
        
        /// <summary>
        /// Task's difficulty
        /// </summary>
        public Difficulty difficulty;

        public TaskSettings()
        {
            taskTimeDuration = 5;
            difficulty = Difficulty.Easy;
        }

        public TaskSettings(TaskSettings newTaskSettings)
        {
            taskTimeDuration = newTaskSettings.taskTimeDuration;
            difficulty = newTaskSettings.difficulty;
        }
    }
}