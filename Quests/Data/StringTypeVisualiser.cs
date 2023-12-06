using System;
using Sirenix.OdinInspector;

[Serializable]
public class StringTypeVisualiser : QuestParameterVisualiser
{
    public StringTypeVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public string Value
    {
        get => (string)data?.GetValue();
        set
        {
            data?.SetValue((string)value);
            UpdateJsonData();
        }
    }
}