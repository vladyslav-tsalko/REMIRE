using UI.PanelControllers;
using UnityEngine;

namespace UI.CanvasControllers
{
    public class MainCanvasController : CanvasController
    {
        protected override float DesiredDistance => 2f;
        protected override void Start()
        {
            base.Start();
            OpenMainMenu();
        }
    
        public void OpenMainMenu()
        {
            ShowPanel(EPanelType.MainMenu);
        }

        public void OpenSettings()
        {
            ShowPanel(EPanelType.Settings);
        }
    
        public void OpenTaskSettings()
        {
            ShowPanel(EPanelType.TaskSettings);
        }
    
        public void OpenTableSettings()
        {
            ShowPanel(EPanelType.TableSettings);
        }
    
        public void OpenReachAreaSettings()
        {
            ShowPanel(EPanelType.ReachAreaSettings);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }

}

