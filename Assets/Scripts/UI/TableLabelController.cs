using UnityEngine;

namespace UI
{
    public class TableLabelController: MonoBehaviour
    {
        void LateUpdate()
        {
            if (Camera.main)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180f, 0); // Flip to face user
            }
        }
    }
}