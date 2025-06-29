using System;
using LearnXR.Core.Utilities;
using UnityEngine;

namespace Tasks.TaskObjectScripts
{
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