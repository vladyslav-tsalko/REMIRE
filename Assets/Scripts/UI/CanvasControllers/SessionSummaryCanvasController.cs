using UI.PanelControllers;
using UnityEngine;

namespace UI.CanvasControllers
{
    /// <summary>
    /// Responsible for managing and switching between different panels on the session summary canvas.
    /// </summary>
    public class SessionSummaryCanvasController : CanvasController
    {
        protected override float DesiredDistance => 1.75f;
        protected override void Start()
        {
            base.Start();
            ShowPanel(EPanelType.SessionSummary);
        }
    }
}

