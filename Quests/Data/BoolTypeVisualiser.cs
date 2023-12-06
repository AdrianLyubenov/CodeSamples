using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoolTypeVisualiser : QuestParameterVisualiser
{
    public BoolTypeVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }

    [ShowInInspector]
    public bool Value
    {
        get
        {
            var val = data?.GetValue();
            if (val != null)
            {
                return (bool)val;
            }

            return default;
        }
        set
        { 
            data?.SetValue((bool)value);
            UpdateJsonData();
        }
    }
}