using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tasks
{
    public class Stairs: MonoBehaviour
    {
        [SerializeField] private Podest level1;
        [SerializeField] private Podest level2;
        [SerializeField] private Podest level3;
        
        private Action<EPodestLevel> _podestTriggerFunc;
        
        public void RegisterPodestTrigger(Action<EPodestLevel> podestTriggerFunc)
        {
            if (_podestTriggerFunc != null) return;
            _podestTriggerFunc = podestTriggerFunc;
            
            level1.onCorrectTriggered += _podestTriggerFunc;
            level2.onCorrectTriggered += _podestTriggerFunc;
            level3.onCorrectTriggered += _podestTriggerFunc;
        }

        private void OnDestroy()
        {
            if (_podestTriggerFunc != null)
            {
                level1.onCorrectTriggered -= _podestTriggerFunc;
                level2.onCorrectTriggered -= _podestTriggerFunc;
                level3.onCorrectTriggered -= _podestTriggerFunc;
            }
        }
    }
}