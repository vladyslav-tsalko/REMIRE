using System;
using System.Collections.Generic;
using System.Linq;
using LearnXR.Core.Utilities;
using Managers;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using Utilities;

namespace Tasks
{
    public class MoveCubeTask: Task
    {
        private static readonly IReadOnlyList<byte> CorrectSequenceEasy = new byte[] { 1, 2, 3, 2, 1 };
        private static readonly IReadOnlyList<byte> CorrectSequenceNormal = new byte[] { 1, 2, 1, 2, 3, 2, 1, 2, 1 };
        private static readonly IReadOnlyList<byte> CorrectSequenceHard = new byte[] { 1, 2, 1, 2, 3, 2, 3, 2, 1, 2, 1 };

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
        
        private static readonly float BaseCubeScale = 0.10f;
        private static readonly float DeltaCubeScale = 0.05f;
        
        public override string Name => "Move Cube";
        public override ETaskType TaskType => ETaskType.MoveCube;
        
        public override string TaskDescription => $"Place the cube on the podests, order: {OutputSequence(GetCorrectSequence())}";

        /// <summary>
        /// Treat as read-only.
        /// </summary>
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
            if (_currentIndex > 0 && _correctSequence[_currentIndex - 1] == podestLvl) return;
            
            _currentSequence[_currentIndex] = podestLvl;
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
                UpdateHint($"{CurrentSequenceWithoutZeroesStr} vs {CorrectSequenceStr}");
                ResetSequence();
            }
        }

        protected override void SpawnObjects()
        {
            _correctSequence = GetCorrectSequence();
            _currentSequence = new byte[_correctSequence.Count];
            
            Table table = TableManager.Instance.SelectedTable;

            TaskObjectPrefabsManager taskObjMan = TaskObjectPrefabsManager.Instance;
            
            GameObject spawnedStairs = table.SpawnPrefab(taskObjMan.StairsPrefab, ESpawnLocation.Middle, TaskSettings.difficulty, false);
            _stairs = spawnedStairs.GetComponent<Stairs>();
            _stairs.RegisterPodestTrigger(OnCorrectPodestTrigger);
            SpawnedObjects.Add(spawnedStairs);

            GameObject spawnedCube = table.SpawnPrefab(taskObjMan.CubePrefab, ESpawnLocation.Middle, TaskSettings.difficulty, Vector3.up * 0.5f);
            spawnedCube.GetComponent<Grabbable>().SetPressBlockAreaSize(TaskSettings.difficulty);
            spawnedCube.transform.localScale = Vector3.one * (BaseCubeScale + DeltaCubeScale * (float)TaskSettings.difficulty);
            SpawnedObjects.Add(spawnedCube);
            UpdateHint($"Correct sequence: ");
        }
    }
}