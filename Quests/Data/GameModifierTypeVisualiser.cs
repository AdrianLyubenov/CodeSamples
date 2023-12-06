using System;
using Sirenix.OdinInspector;

[Serializable]
public class GameModifierTypeVisualiser : QuestParameterVisualiser
{
    [ShowInInspector]
    public GameModifierType Value
    {
        get => (GameModifierType)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }

    public GameModifierTypeVisualiser(QuestParameterData data) : base(data)
    {
        Value = Value;
    }
}