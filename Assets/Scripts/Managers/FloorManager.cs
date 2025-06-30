using System;
using LearnXR.Core.Utilities;
using Meta.XR.MRUtilityKit;

namespace Managers
{
    public class FloorManager: Singleton<FloorManager>
    {
        public float FloorY { get; private set; }

        private void Start()
        {
            base.Awake();
            MRUK.Instance.SceneLoadedEvent.AddListener(AssignFloorLevel);
        }

        private void AssignFloorLevel()
        {
            FloorY = MRUK.Instance.GetCurrentRoom().FloorAnchor.transform.position.y;
        }

        private void OnDestroy()
        {
            if(MRUK.Instance) MRUK.Instance.SceneLoadedEvent.RemoveListener(AssignFloorLevel);
        }
    }
}