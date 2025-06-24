using Meta.XR.MRUtilityKit;

namespace Managers
{
    public class FloorManager: Singleton<FloorManager>
    {
        private float _floorY = 0;
        public float FloorY => _floorY;

        public void AssignFloorLevel()
        {
            _floorY = MRUK.Instance.GetCurrentRoom().FloorAnchor.transform.position.y;
        }
    }
}