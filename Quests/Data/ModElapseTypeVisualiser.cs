using System;
using Sirenix.OdinInspector;

[Serializable]
public class ModElapseTypeVisualiser : QuestParameterVisualiser
{
    public ModElapseTypeVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public ModElapseType Value
    {
        get => (ModElapseType)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }
}