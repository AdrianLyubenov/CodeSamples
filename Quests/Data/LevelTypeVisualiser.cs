using System;
using Sirenix.OdinInspector;

[Serializable]
public class LevelTypeVisualiser : QuestParameterVisualiser
{
    public LevelTypeVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public LevelType Value
    {
        get => (LevelType)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }
}