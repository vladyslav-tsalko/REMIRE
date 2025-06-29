using UnityEngine;

namespace UI.PanelControllers
{
    [RequireComponent(typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    public class UIPanel: MonoBehaviour
    {
        [SerializeField] private EPanelType panelType = EPanelType.Default;
        public EPanelType PanelType => panelType;
        
#if UNITY_EDITOR
    private void OnValidate()
    {
        var allPanels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var panel in allPanels)
        {
            if (panel != this && panel.panelType == this.panelType)
            {
                Debug.LogError($"Duplicate panel type: {panelType} already used on {panel.gameObject.name}", this);
                break;
            }
        }
    }
#endif
    }
}