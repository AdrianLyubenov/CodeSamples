using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class QuestParameterVisualiser
{
    [HideInInspector]
    [NonSerialized]
    protected QuestParameterData data;
    
    [ShowInInspector] [ReadOnly]
    public QuestParameter param => data != null ? (QuestParameter)data.Parameter : QuestParameter.FollowerCivilization;
    
    public bool showJson;
    [Multiline(order = 99)][ShowIf("@showJson")][ReadOnly]
    public string jsonData;

    public QuestParameterVisualiser(QuestParameterData data)
    {
        this.data = data;
        data.UpdateFromBackingPropertyIfNull();
        UpdateJsonData();
    }
    
    public void UpdateJsonData()
    {
        if (data == null) return;
        jsonData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}