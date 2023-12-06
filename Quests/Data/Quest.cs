using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Quest
{
    public string Id;

    public int Index;//Order in the UI independent of DB order
    public string Name;//Daily/Weekly/Monthly
#if UNITY_EDITOR
    [BoxGroup("Dates")] [HorizontalGroup("Dates/Start")] [OnValueChanged("UpdateStartDate")][LabelWidth(200)]
#endif
    public long StartDate;
#if UNITY_EDITOR
    [BoxGroup("Dates")] [HorizontalGroup("Dates/End")] [OnValueChanged("UpdateEndDate")][LabelWidth(200)]
#endif
    public long EndDate;
    public List<QuestReward> Rewards;
    
#if UNITY_EDITOR
    [ValueDropdown("GetListOfQuestTextures", AppendNextDrawer = true)]
#endif
    public string ClaimedRewardsTextureId;
#if UNITY_EDITOR
    [ValueDropdown("GetListOfQuestTextures", AppendNextDrawer = true)]
#endif
    public string UnclaimedRewardsTextureId;
#if UNITY_EDITOR
    [ListDrawerSettings(CustomAddFunction = "AddTask", OnBeginListElementGUI = "OnBeginTaskListElementGUI", NumberOfItemsPerPage = 5, ShowPaging = true)]
#endif
    public List<QuestTask> Tasks;
    
#if UNITY_EDITOR
    
    [JsonIgnore] [NonSerialized] [ShowInInspector] [HorizontalGroup("IDUpdate")] public string IdFormat = "{0}_{1}";
    [Button] [HorizontalGroup("IDUpdate")]
    public void UpdateTaskIds()
    {
        for (var index = 0; index < Tasks.Count; index++)
        {
            var questTask = Tasks[index];
            questTask.Index = index;
            questTask.Id = string.Format(IdFormat, Id, index);
        }
    }
    
    [Button]
    public void AddTask()
    {
        var newTask = new QuestTask();
        newTask.Index = Tasks.Count;
        newTask.Id = Id + "_" + newTask.Index;
        Tasks.Add(newTask);
    }
    
    
    private void OnBeginTaskListElementGUI(int index)
    {
        if(index == 0) return;
        GUILayout.Space(25);
    }
    
    public void UpdateStartDate()
    {
        startDateView = DateTimeOffset.FromUnixTimeSeconds(StartDate).LocalDateTime.ToString("M/dd/yyyy HH:mm:ss");
    }
    public void UpdateStartDate2()
    {
        StartDate = ((DateTimeOffset)DateTime.Parse(startDateView)).ToUnixTimeSeconds();
    }
    
    public void UpdateEndDate()
    {
        endDateView = DateTimeOffset.FromUnixTimeSeconds(EndDate).LocalDateTime.ToString("M/dd/yyyy HH:mm:ss");
    }
    public void UpdateEndDate2()
    {
        EndDate = ((DateTimeOffset)DateTime.Parse(endDateView)).ToUnixTimeSeconds();
    }
    
    [ShowInInspector] [NonSerialized] [JsonIgnore] [HorizontalGroup("Dates/Start")] [HideLabel][DelayedProperty][OnValueChanged("UpdateStartDate2")]
    public string startDateView;
    
    [ShowInInspector] [NonSerialized] [JsonIgnore] [HorizontalGroup("Dates/End")] [HideLabel][DelayedProperty][OnValueChanged("UpdateEndDate2")]
    public string endDateView;


    private IEnumerable<string> GetListOfQuestTextures() => EditorTextureIDCache.GetTexturesForKey("quest_texture");
#endif
}