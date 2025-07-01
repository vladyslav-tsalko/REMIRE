using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Holds references to prefabs used in various tasks.
    /// </summary>
    public class TaskObjectPrefabsManager: Singleton<TaskObjectPrefabsManager>
    {
        [SerializeField] public GameObject bottlePrefab;
        [SerializeField] public GameObject glassPrefab;
        [SerializeField] public GameObject cubePrefab;
        [SerializeField] public GameObject circularPodest;
        [SerializeField] public GameObject stairsPrefab;
        [SerializeField] public GameObject liquidStreamPrefab;
        [SerializeField] public GameObject grabMidpointPrefab;
        

        /// <returns>
        /// Stairs sizes in cm, x - length, y - width
        /// </returns>
        public Vector2 GetStairsSizes()
        {
            GameObject stairs = stairsPrefab;
            if (!stairs) return Vector2.zero;
            
            //Initial size of stairs is 3m x 1m. 
            Vector3 scale = stairs.transform.lossyScale;
            return new Vector2(3 * scale.x, scale.z);
        }
    }
}