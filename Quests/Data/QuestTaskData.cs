using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
public class QuestTaskData
{
    [PropertyTooltip("Action to be performed by the player")]
    [ShowInInspector][JsonIgnore] [LabelText("Action")][VerticalGroup("VerticalGroup")][HorizontalGroup("VerticalGroup/HorizontalGroup1", Order = 0)]
    public QuestAction actionVisual
    {
        get => (QuestAction)Action;
        set => Action = (int)value;
    }
    
    [HideInInspector]
    public int Action;
    
    [PropertyTooltip("Parameter to be used for progression of the task")]
    [ShowInInspector][JsonIgnore] [LabelText("Progression Parameter")][HorizontalGroup("VerticalGroup/HorizontalGroup1")]
    public QuestProgressionParameter progressionVisual
    {
        get => (QuestProgressionParameter)ProgressionParameter;
        set => ProgressionParameter = (int)value;
    }
    
    [HideInInspector]
    public int ProgressionParameter = 0;
    
#if UNITY_EDITOR
    [InfoBox("Invalid format", "_isFormatInvalid", InfoMessageType = InfoMessageType.Error)]
    [Tooltip("Input a string in this format '(0 OR (1 AND 2)) AND (3 AND 4)' using numbers start at 0 as a placeholders for the rules in the rules list.")]
    [OnValueChanged("CheckFormat")] [DelayedProperty] [HorizontalGroup("VerticalGroup/HorizontalGroup2", Order = 3)]
#endif
    public string ParametersFormat;

    [InfoBox("Do not edit raw parameters by hand if you are not familiar with the format, use the visualizers !!!", "showRawParameters", InfoMessageType = InfoMessageType.Error)]
    [ShowIf("@showRawParameters")] [BoxGroup("VerticalGroup/Parameters", Order = 5)] [ListDrawerSettings(HideAddButton = true, ShowIndexLabels = true)]
    public List<QuestParameterData> Parameters;

#if UNITY_EDITOR
    [HorizontalGroup("VerticalGroup/Show_Hide", Order = 2)]
    [Button]
    public void ToggleVisualisers()
    { 
        showVisualisers = !showVisualisers;
        if (showVisualisers)
        {
            AssemblyReloadEvents.beforeAssemblyReload += ToggleVisualisers;
        }
        else
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ToggleVisualisers;
        }
        UpdateVisualizers();
    }

    [Button] [HorizontalGroup("VerticalGroup/Show_Hide")]
    public void ToggleRawData()
    {
        showRawParameters = !showRawParameters;
    }

    [Button] [HorizontalGroup("VerticalGroup/Add", Order = 1)]
    public void AddParameter()
    {
        Parameters ??= new List<QuestParameterData>();
        Parameters.Add(new QuestParameterData(){Parameter = (int)paramToAdd, Operation = (int)Operand.Equal});
        if (showVisualisers)
        {
            UpdateVisualizers();
        }
    }

    private void UpdateVisualizers()
    {
        ParametersVisualizers = showVisualisers ? Parameters.Select(x => GetParameterVisualiser(x)).ToList() : null;
    }

    [JsonIgnore] [HorizontalGroup("VerticalGroup/Add")] [HideLabel][NonSerialized][ShowInInspector]
    public QuestParameter paramToAdd;
    public QuestParameterVisualiser GetParameterVisualiser(QuestParameterData data)
    {
        var parameterType = (QuestParameter)data.Parameter;
        switch (parameterType)
        {
            case QuestParameter.FollowersUsed:
            case QuestParameter.PieceLevel:
            case QuestParameter.CascadeMergeLevel:
            case QuestParameter.LevelScore:
            case QuestParameter.StarsEarned:
            case QuestParameter.StarsScored:
            case QuestParameter.ScoreAdded:
                return new LongTypeVisualiser(data);
            case QuestParameter.GodName:
            case QuestParameter.LevelID:
            case QuestParameter.PieceID:
                return new StringTypeVisualiser(data);
            case QuestParameter.LevelIsPractice:
            case QuestParameter.HasQuit:
                return new BoolTypeVisualiser(data);
            case QuestParameter.FollowerCivilization:
            case QuestParameter.GodCivilization:
            case QuestParameter.LevelCivilization:
                return new CivilisationTypeVisualiser(data);
            case QuestParameter.FollowerUseTrigger:
                return new FollowerUseVisualiser(data);
            case QuestParameter.GodTier:
                return new GodTierVisualiser(data);
            case QuestParameter.GodPower:
                return new GodPowerVisualiser(data);
            case QuestParameter.LevelType:
                return new LevelTypeVisualiser(data);
            case QuestParameter.ModLimitGameType:
                return new ModElapseTypeVisualiser(data);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    [JsonIgnore]
    private bool showRawParameters;
    [JsonIgnore]
    private bool showVisualisers;
    [JsonIgnore] [ShowIf("@showVisualisers")] [ListDrawerSettings(CustomRemoveIndexFunction = "RemoveParameter", ShowIndexLabels = true, ShowPaging = true)]
    [ShowInInspector] private List<QuestParameterVisualiser> ParametersVisualizers;

    private void RemoveParameter(int index)
    {
        Parameters.RemoveAt(index); 
        UpdateVisualizers(); 
    } 
    private bool _isFormatInvalid;//used in info box condition
    private void CheckFormat()
    {
        _isFormatInvalid = !string.IsNullOrEmpty(ParametersFormat) && !RuleUtilities.ValidateRuleFormat(ParametersFormat);
    }
#endif
}