using System;
using Sirenix.OdinInspector;

[Serializable]
public class CivilisationTypeVisualiser : QuestParameterVisualiser
{
    [ShowInInspector]
    public CivilisationType Value
    {
        get => (CivilisationType)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }

    public CivilisationTypeVisualiser(QuestParameterData data) : base(data)
    {
        Value = Value;
    }
}