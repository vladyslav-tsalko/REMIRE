using System;
using LearnXR.Core.Utilities;
using UnityEngine;

namespace Tasks.TaskObjectScripts
{
    /// <summary>
    /// Attached to the DrinkWaterArea GameObject to detect when a glass enters or exits the area.
    /// </summary>
    public class DrinkingArea: MonoBehaviour
    {
        public event Action<Collider> TriggerEntered;
        public event Action<Collider> TriggerExited;
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Glass")) TriggerEntered?.Invoke(other);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Glass")) TriggerExited?.Invoke(other);
        }
    }
}