using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Quests/QuestProgress", fileName = "QuestProgress", order = 0)]
[Serializable]
public class QuestProgressSO : ScriptableObject
{
    public List<QuestOverallUserProgress> progress;
    public List<QuestOverallUserProgress> GetProgress()
    {
        foreach (var userProgress in progress)
        {
            userProgress.UpdateDictionary();
        }
        return progress;
    }
}