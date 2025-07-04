using System;
using System.Collections.Generic;
using System.Linq;
using Hands.Grabbables;
using LearnXR.Core.Utilities;
using Managers;
using UnityEngine;
using Tasks.TaskObjectScripts;
using Tasks.TaskProperties;

namespace Tasks
{
    /// <summary>
    /// A task where the user must place a cube on multiple podests.
    /// The task is completed when the cube is placed on the podests in the correct sequence, determined by the selected difficulty.
    /// </summary>
    public class MoveCubeTask: Task
    {
        #region Correct sequences
        
        private static readonly IReadOnlyList<byte> CorrectSequenceEasy = new byte[] { 1, 2, 3, 2, 1 };
        private static readonly IReadOnlyList<byte> CorrectSequenceNormal = new byte[] { 1, 2, 1, 2, 3, 2, 1, 2, 1 };
        private static readonly IReadOnlyList<byte> CorrectSequenceHard = new byte[] { 1, 2, 1, 2, 3, 2, 3, 2, 1, 2, 1 };
        
        #endregion

        public override string Name => "Move Cube";
        public override ETaskType TaskType => ETaskType.MoveCube;
        public override string TaskDescription => $"Place the cube on the podests, order: {OutputSequence(GetCorrectSequence())}";
        
                
        private static readonly float BaseCubeScale = 0.10f;
        private static readonly float DeltaCubeScale = 0.05f;
        
        private IReadOnlyList<byte> GetCorrectSequence()
        {
            switch (TaskSettings.difficulty)
            {
                case Difficulty.Easy:
                    return CorrectSequenceEasy;
                case Difficulty.Normal:
                    return CorrectSequenceNormal;
                default:
                    return CorrectSequenceHard;
            }
        }

        private static string OutputSequence(IReadOnlyList<byte> seq) =>  "(" + string.Join(",", seq) + ")";
        private static string OutputSequenceWithoutZeroes(IReadOnlyList<byte> seq) =>
            "(" + string.Join(",", seq.Where(b => b != 0)) + ")";
        private string CurrentSequenceStr => OutputSequence(_currentSequence);
        private string CurrentSequenceWithoutZeroesStr => OutputSequenceWithoutZeroes(_currentSequence);
        private string CorrectSequenceStr => OutputSequence(_correctSequence);

        


        /// <remarks>
        /// Treat as read-only.
        /// </remarks>
        private byte[] _currentSequence;
        private IReadOnlyList<byte> _correctSequence;
        private short _currentIndex;
        private bool _isSequenceCorrect;

        private Stairs _stairs;

        protected override void IncreaseScore()
        {
            base.IncreaseScore();
            ResetSequence();
            _isSequenceCorrect = false;
        }

        protected override bool AreAllObjectsSatisfyConditions()
        {
            return _isSequenceCorrect;
        }

        protected override void InitializeDefaults()
        {
            _correctSequence = GetCorrectSequence();
            _currentSequence = new byte[_correctSequence.Count];
            _currentIndex = 0;
        }

        protected override void SpawnObjects()
        {
            var table = TableManager.Instance.SelectedTable;

            TaskObjectPrefabsManager taskObjMan = TaskObjectPrefabsManager.Instance;
            
            GameObject spawnedStairs = table.SpawnPrefab(taskObjMan.stairsPrefab, TableManager.Table.ESpawnLocation.Middle, TaskSettings.difficulty, false);
            _stairs = spawnedStairs.GetComponent<Stairs>();
            _stairs.RegisterPodestTrigger(OnCorrectPodestTrigger);
            SpawnedObjects.Add(spawnedStairs);

            GameObject spawnedCube = table.SpawnPrefab(taskObjMan.cubePrefab, TableManager.Table.ESpawnLocation.Middle, TaskSettings.difficulty, Vector3.up * 0.5f);
            spawnedCube.GetComponent<KinematicGrabbable>().SetPressBlockAreaSize(TaskSettings.difficulty);
            spawnedCube.transform.localScale = Vector3.one * (BaseCubeScale + DeltaCubeScale * (float)TaskSettings.difficulty);
            SpawnedObjects.Add(spawnedCube);
            UpdateHint($"Correct sequence: ");
        }
        
        private void ResetSequence()
        {
            for (var i = 0; i < _currentIndex; i++)
            {
                _currentSequence[i] = 0;
            }

            _currentIndex = 0;
        }

        private void OnCorrectPodestTrigger(EPodestLevel podestLevel)
        {
            byte podestLvl = (byte)podestLevel;
            //if the same level triggered
            if (_currentIndex > 0 && _correctSequence[_currentIndex - 1] == podestLvl) return;
            
            //assign triggered podest level to current sequence
            _currentSequence[_currentIndex] = podestLvl;
            
            //if it's right
            if (_correctSequence[_currentIndex] == podestLvl)
            {
                _currentIndex++;
                if (_currentIndex == _currentSequence.Length)
                {
                    _isSequenceCorrect = true;
                    UpdateHint($"Current sequence: {CurrentSequenceStr} well done :)");
                }
                else
                {
                    UpdateHint($"Current sequence: {CurrentSequenceStr} in process...");
                }
            }
            else
            {
                if (_currentIndex == 0) return;
                _currentIndex++;
                UpdateHint($"{CurrentSequenceWithoutZeroesStr} vs {CorrectSequenceStr}");
                ResetSequence();
            }
        }
    }
}