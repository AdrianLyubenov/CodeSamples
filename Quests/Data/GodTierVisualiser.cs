using System;
using Sirenix.OdinInspector;

[Serializable]
public class GodTierVisualiser : QuestParameterVisualiser
{
    public GodTierVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public GodTier Value
    {
        get => (GodTier)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }
}