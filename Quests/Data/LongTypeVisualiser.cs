using System;
using Sirenix.OdinInspector;

[Serializable]
public class LongTypeVisualiser : QuestParameterVisualiser
{
    public LongTypeVisualiser(QuestParameterData data):base(data)
    {
        Value = Value;
    }
    
    
    [ShowInInspector]
    public Operand operation
    {
        get
        {
            return data != null ? (Operand)data.Operation : Operand.Equal;
        }
        set
        {
            if (data == null) return;
            data.Operation = (int)value;
            UpdateJsonData();
        }
    }

    [ShowInInspector]
    public long Value
    {
        get
        {
            var val = data?.GetValue();
            if (val != null)
            {
                return (long)val;
            }

            return default; 
        }
        set
        {
            data?.SetValue((long)value);
            UpdateJsonData();
        }
    }
}