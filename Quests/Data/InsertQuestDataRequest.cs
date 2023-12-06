using System;
using System.Collections.Generic;

[Serializable]
public class InsertQuestDataRequest
{
    public List<Quest> UpsertQuests;
    public List<string> DeleteQuestsIds;

    public InsertQuestDataRequest()
    {
        UpsertQuests = new List<Quest>();
        DeleteQuestsIds = new List<string>();
    }
}