using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Managers;
using UnityEngine;
using UI.CanvasControllers;

namespace Managers
{
    /// <summary>
    /// Manages all canvases and their panels.
    /// </summary>
    /// <remarks>
    /// This class MUST be attached to the main UI game object. If you want to do it somewhere else, assign firstly it's
    /// grandchildren.
    /// </remarks>
    public class UIManager : Singleton<UIManager>
    {
        private readonly List<CanvasController> _canvasControllers = new();
        
        public void ShowMainCanvas() => ShowCanvas<MainCanvasController>();
        public void ShowTaskCanvas() => ShowCanvas<TaskInfoCanvasController>();
        public void ShowSummaryCanvas() => ShowCanvas<SessionSummaryCanvasController>();

        public MainCanvasController GetMainCanvasController => GetCanvas<MainCanvasController>();
        
        protected override void Awake()
        {
            base.Awake();
            
            foreach (Transform canvasInteractable in transform)
            {
                if (canvasInteractable.TryGetComponent<MainCanvasController>(out var mainCanvasController))
                {
                    mainCanvasController.gameObject.SetActive(true);
                    _canvasControllers.Add(mainCanvasController);
                }else if (canvasInteractable.TryGetComponent<TaskInfoCanvasController>(out var taskInfoCanvasController))
                {
                    taskInfoCanvasController.gameObject.SetActive(true);
                    _canvasControllers.Add(taskInfoCanvasController);
                }else if (canvasInteractable.TryGetComponent<SessionSummaryCanvasController>(out var sessionSummaryCanvasController))
                {
                    sessionSummaryCanvasController.gameObject.SetActive(true);
                    _canvasControllers.Add(sessionSummaryCanvasController);
                }
            }
            
            if (_canvasControllers.Count != 3)
            {
                Debug.LogError("canvasObject is not found");
            }
        }
        
        /// <remarks>
        /// If this GO is attached to the UI GO and it is disabled, then all UIs are also disabled.
        /// If you attach this script to another GO, make sure to handle it in another way.
        /// </remarks>
        void Start()
        {
            ShowMainCanvas();
            TableManager.Instance.OnStartTableSelection += HideAllCanvases;
            TableManager.Instance.OnTableSelected += ShowMainCanvas;
        }

        /// <summary>
        /// Gets a canvas controller of type <typeparamref name="T"/> if it exists.
        /// </summary>
        /// <typeparam name="T">Type of the canvas controller to retrieve.</typeparam>
        /// <returns>
        /// The canvas controller of type <typeparamref name="T"/> if found; otherwise, null.
        /// </returns>
        [CanBeNull] private T GetCanvas<T>() where T : CanvasController =>
            typeof(T) == typeof(CanvasController) ? null : _canvasControllers.OfType<T>().FirstOrDefault();

        /// <summary>
        /// Shows the canvas of type <typeparamref name="T"/> and hides all other canvases.
        /// </summary>
        /// <typeparam name="T">Type of the canvas controller to show.</typeparam>
        private void ShowCanvas<T>() where T : CanvasController
        {
            _canvasControllers.ForEach(canvasController =>
            {
                if (canvasController is T)
                {
                    canvasController.ShowCanvas();
                }
                else
                {
                    canvasController.HideCanvas();
                }
            });
        }

        private void HideAllCanvases()
        {
            _canvasControllers.ForEach(canvas => canvas.HideCanvas());
        }
    }
}

