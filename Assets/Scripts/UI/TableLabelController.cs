using UnityEngine;

namespace UI
{
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