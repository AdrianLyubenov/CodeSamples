using System;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Quests/QuestData", fileName = "QuestData", order = 0)]
[Serializable]
public class QuestSO : ScriptableObject
{
    public Quest data;
    
#if UNITY_EDITOR
    [Button(ButtonSizes.Large)]
    public void CopyToClipboard()
    {
        foreach (var param in data.Tasks.SelectMany(x => x.TaskData.Parameters))
        {
            param.GetValue();
        }
        JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings(){TypeNameHandling = TypeNameHandling.Auto}).CopyToClipboard();
    }
    [Button(ButtonSizes.Large)]
    public void LoadFromClipboard()
    {
        data = Newtonsoft.Json.JsonConvert.DeserializeObject<Quest>(GUIUtility.systemCopyBuffer);
    }
    
    [Button(ButtonSizes.Large)]
    public void OpenEditor()
    {
        var window = QuestEditor.GetWindow<QuestEditor>();
        window.SetData(data);
        window.Show();
    }
    
#endif
}
#if UNITY_EDITOR
public class QuestEditor : Sirenix.OdinInspector.Editor.OdinMenuEditorWindow
{
    [SerializeField, HideLabel]
    private Quest _data = new Quest();
    protected override Sirenix.OdinInspector.Editor.OdinMenuTree BuildMenuTree()
    {
        Sirenix.OdinInspector.Editor.OdinMenuTree tree = new Sirenix.OdinInspector.Editor.OdinMenuTree()
        {
            { "Data", _data},
            { "Tasks", _data.Tasks},
        };
        foreach (var Task in _data.Tasks)
        {
            var taskPath = "Tasks/" + Task.Id;
            tree.Add(taskPath, Task);
        }
        
        return tree;
    }

    public void SetData(Quest data)
    {
        _data = data;
        ForceMenuTreeRebuild();
    }
}

#endif