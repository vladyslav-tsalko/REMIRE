namespace UI.PanelControllers
{
    /// <summary>
    /// Attached to each separate panel that can be shown/hidden on a canvas.
    /// </summary>
    /// <remarks>
    /// New panels need to be either added in the end of the enum, or where ever you want,
    /// but don't forget to change them in the scene.
    /// </remarks>
    public enum EPanelType
    {
        Default,
        MainMenu,
        TaskPause,
        Settings,
        TaskSettings,
        TableSettings,
        ReachAreaSettings,
        CurrentTaskInfo,
        SessionSummary,
    }
}