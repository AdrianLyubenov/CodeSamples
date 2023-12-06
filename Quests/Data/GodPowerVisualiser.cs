using System;
using Sirenix.OdinInspector;

[Serializable]
public class GodPowerVisualiser : QuestParameterVisualiser
{
    public GodPowerVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public GodPower Value
    {
        get => (GodPower)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }
}