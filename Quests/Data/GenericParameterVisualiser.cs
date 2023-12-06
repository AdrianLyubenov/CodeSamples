using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class GenericParameterVisualiser<T> : QuestParameterVisualiser
{
    public Action<GenericParameterVisualiser<T>, T> valueChanged;
    public Func<object, object> customValueFunc;
    public GenericParameterVisualiser(QuestParameterData data, Action<GenericParameterVisualiser<T>, T> onChanged, bool showOperand = false, Func<object, object> customValueFunc = null):base(data)
    {
        this.showOperand = showOperand;
        data.UpdateFromBackingPropertyIfNull();
        valueChanged += onChanged;
        this.customValueFunc = customValueFunc;
    }
        
    [ShowInInspector] [ReadOnly]
    public QuestParameter param
    {
        get
        {
            return (QuestParameter)data.Parameter;
        }
        set
        {
            data.Parameter = (int)value; 
            UpdateJsonData();
        }
    }
    
    [ShowInInspector] [ReadOnly][ShowIf("@showOperand")]
    public Operand operation
    {
        get
        {
            return (Operand)data.Operation;
        }
        set
        {
            data.Operation = (int)value; 
            UpdateJsonData();
        }
    }

    [ShowInInspector]
    public T Value
    {
        get
        {
            var value = data?.GetValue();
            if (value != null)
            {
                if (customValueFunc != null)
                {
                    return (T)customValueFunc.Invoke(value);
                }
                else
                {
                    return (T)value;
                }
            }
            return  default; 
        }
        set
        {
            valueChanged?.Invoke(this, value);
            UpdateJsonData();
        }
    }
        
    [HideInInspector]
    [NonSerialized]
    public QuestParameterData data;

    public bool showJson;
    [HideInInspector]
    public bool showOperand;
    [Multiline(order = 99)][ShowIf("@showJson")][ReadOnly]
    public string jsonData;
    
    public void UpdateJsonData()
    {
        jsonData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
    }
}