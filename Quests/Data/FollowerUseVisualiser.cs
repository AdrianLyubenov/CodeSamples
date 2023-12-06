using System;
using Sirenix.OdinInspector;

[Serializable]
public class FollowerUseVisualiser : QuestParameterVisualiser
{
    [ShowInInspector]
    public FollowerUse Value
    {
        get => (FollowerUse)Convert.ToInt32(data?.GetValue() ?? 0);
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }

    public FollowerUseVisualiser(QuestParameterData data) : base(data)
    {
        Value = Value;
    }
}