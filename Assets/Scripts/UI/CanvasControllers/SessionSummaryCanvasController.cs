using UI.Panels;
using UnityEngine;

public class SessionSummaryCanvasController : CanvasController
{
    protected override float DesiredDistance => 1.75f;
    protected override void Start()
    {
        base.Start();
        ShowPanel(EPanelType.SessionSummary);
    }
    
}
