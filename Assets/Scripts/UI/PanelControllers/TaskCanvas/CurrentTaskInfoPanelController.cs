using System;
using LearnXR.Core.Utilities;
using Managers;
using Tasks;
using TMPro;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIPanel))]
public class CurrentTaskInfoPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI taskNameTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;
    [SerializeField] private TextMeshProUGUI timeRestTMP;
    [SerializeField] private TextMeshProUGUI hintTMP;

    void UpdateTaskInfo(string newTaskName, int timeRest, string newHint)
    {
        taskNameTMP.text = newTaskName;
        UpdateScore(0);
        UpdateOnTimeRest(timeRest.ToString() + ":00");
        UpdateTaskHint(newHint);
    }

    void UpdateTaskHint(string newHint)
    {
        hintTMP.text = newHint;
    }

    void UpdateOnTimeRest(string newTimeRest)
    {
        timeRestTMP.text = newTimeRest;
    }

    void UpdateScore(int newScore)
    {
        scoreTMP.text = newScore.ToString();
    }

    void BindOnNewTask()
    {
        GameManager.Instance.OnNewTask += UpdateTaskInfo;
    }

    void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.OnInstanceCreated += BindOnNewTask;
        }
        else
        {
            BindOnNewTask();
        }
    }

    void Start()
    {
        TimeManager.Instance.OnTimeUpdated += UpdateOnTimeRest;
        Task.OnScoreUpdate += UpdateScore;
        Task.OnHintUpdated += UpdateTaskHint;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.OnTimeUpdated -= UpdateOnTimeRest;
        GameManager.Instance.OnNewTask -= UpdateTaskInfo;
        Task.OnScoreUpdate -= UpdateScore;
        Task.OnHintUpdated -= UpdateTaskHint;
    }
}
