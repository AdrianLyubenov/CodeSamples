using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

[Serializable]
public class QuestParameterData
{
    public int Parameter;
    public int Operation;
    public object Value;
    
    [JsonIgnore]
    public string backingProperty;

    public void SetValue(object value)
    {
        this.Value = value;
        backingProperty = GetBackingProperty();
    }

    public void UpdateBackingProperty()
    {
        backingProperty = GetBackingProperty();
    }

    public object GetValue()
    {
        if (Value == null && !string.IsNullOrEmpty(backingProperty))
        {
            Value = GetValueFromBackingProperty();
        }

        return Value;
    }

    public void UpdateFromBackingProperty()
    {
        Value = GetValueFromBackingProperty();
    }

    public void UpdateFromBackingPropertyIfNull()
    {
        if (Value == null)
        {
            UpdateFromBackingProperty();
        }
    }
    private object GetValueFromBackingProperty()
    {
        return !string.IsNullOrEmpty(backingProperty) ? JsonConvert.DeserializeObject(backingProperty, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        }) : null;
    }

    private string GetBackingProperty()
    {
        return JsonConvert.SerializeObject(Value, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
    }
}