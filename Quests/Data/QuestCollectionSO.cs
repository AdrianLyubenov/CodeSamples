using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Quests/QuestCollection", fileName = "QuestCollection", order = 0)]
[Serializable]
public class QuestCollectionSO : ScriptableObject
{
    public List<QuestSO> quests;
    public List<Quest> GetQuests()
    {
        return quests.Select(x => x.data).ToList();
    }
}