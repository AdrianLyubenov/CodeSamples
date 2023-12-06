using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

[Serializable]
public class QuestReward
{
    #region UNITY_EDITOR

    [ShowInInspector][JsonIgnore]
    public CurrencyType currencyVisual
    {
        get => (CurrencyType)Currency;
        set => Currency = (int)value;
    }

    [DisableInNonPrefabs]
    #endregion
    public int Currency;
    public int Count;
#if UNITY_EDITOR
    [ValueDropdown("GetListOfQuestTextures", AppendNextDrawer = true)]
#endif
    public string TextureId;
    
#if UNITY_EDITOR
    private IEnumerable<string> GetListOfQuestTextures() => EditorTextureIDCache.GetTexturesForKey("quest_texture");
#endif
}