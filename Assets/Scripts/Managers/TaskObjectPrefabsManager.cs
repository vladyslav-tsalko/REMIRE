using UnityEngine;

namespace Managers
{
    public class TaskObjectPrefabsManager: Singleton<TaskObjectPrefabsManager>
    {
        [SerializeField] public GameObject BottlePrefab;
        [SerializeField] public GameObject GlassPrefab;
        [SerializeField] public GameObject CubePrefab;
        [SerializeField] public GameObject CircularPodest;
        [SerializeField] public GameObject StairsPrefab;
        [SerializeField] public GameObject LiquidStreamPrefab;
        [SerializeField] public GameObject GrabMidpointPrefab;
        

        /// <returns>
        /// Stairs sizes in cm, x - length, y - width
        /// </returns>
        public Vector2 GetStairsSizes()
        {
            GameObject stairs = StairsPrefab;
            if (!stairs) return Vector2.zero;
            
            //Initial size of stairs is 3m x 1m. 
            Vector3 scale = stairs.transform.lossyScale;
            return new Vector2(3 * scale.x, scale.z);
        }
    }
}