using System;
using UnityEngine;
using Events;

public class Command : ICommand
{
    public bool autoExecute = false;
    public Action OnComplete;
    public bool undo = false;
    public bool omitHistory = false;

    public virtual void Execute() { }
    public virtual void Undo() {

        Debug.Log("UNDO " + autoExecute);
        if (!autoExecute)
        {
            EventManager.Broadcast(new EvUndo());
        }
    }

    public virtual string GetActionLog()
    {
        return "";
    }

    public virtual void Dispose()
    {

    }
}
