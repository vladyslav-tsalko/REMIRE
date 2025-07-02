using UnityEngine;

namespace UI
{
    /// <summary>
    /// Attached to each label that is spawned above the table to show it's dimensions. Follows camera view.
    /// </summary>
    public class TableLabelController: MonoBehaviour
    {
        void LateUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera)
            {
                transform.LookAt(mainCamera.transform);
                transform.Rotate(0, 180f, 0); //To face
            }
        }
    }
}