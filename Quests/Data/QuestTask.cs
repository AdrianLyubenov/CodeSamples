using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class QuestTask
{
    [Tooltip("Has to be a unique id")]
    public string Id; 
    [Tooltip("Order in the UI")]
    public int Index;
    [Tooltip("Number of progress required")]
    public int Count;
    [Tooltip("Name shown in tasks list view")]
    public string Name;
    [Multiline]
    [Tooltip("Description for info popup")]
    public string Desctiption;
    [Tooltip("Whether to show progress notifications")]
    public bool ShowProgressNotification = true;
    [Tooltip("Rewards for the task, up to 3")]
    public List<QuestReward> Rewards;
    [Tooltip("Data describing the in game action to be performed")]
    public QuestTaskData TaskData;
}