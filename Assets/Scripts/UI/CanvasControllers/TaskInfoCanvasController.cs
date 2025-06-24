using System;
using UI.Panels;
using UnityEngine;

public class TaskInfoCanvasController : CanvasController
{
    protected override float DesiredDistance => 1.5f;
    protected override void Start()
    {
        base.Start();
        OpenTaskInfo();
    }

    public void OpenTaskInfo()
    {
        ShowPanel(EPanelType.CurrentTaskInfo);
    }

    public void OpenTaskPause()
    {
        ShowPanel(EPanelType.TaskPause);
    }

    public override void ShowCanvas()
    {
        base.ShowCanvas();
        OpenTaskInfo();
    }
}
