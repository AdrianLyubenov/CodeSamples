using System;
using System.Collections.Generic;
using Events;

public class CommandInvoker
{
    private Queue<Command> commandBuffer;
    private List<Command> commandHistory;
    private int counter;

    private Command cmdInProgress;
    public Action OnCommandsRunFinished;
    public Action<string> OnActionLog;

    public Action OnUndoInvoked;

    public CommandInvoker()
    {
        commandBuffer = new Queue<Command>();
        commandHistory = new List<Command>();
    }

    public void AddCommand(Command command)
    {
        if (!command.omitHistory)
        {
            while (commandHistory.Count > counter)
            {
                commandHistory.RemoveAt(counter);
            }

            commandHistory.Add(command);
            counter++;
        }
        commandBuffer.Enqueue(command);
        //if (cmdInProgress == null)
        //  RunCommand();
    }

public void Undo()
    {
        if (counter > 0)
        {
            counter--;
            Command cmd = commandHistory[counter];
            cmd.undo = true;
            commandBuffer.Enqueue(cmd);

            Command lastCmd = cmd;

            if (counter > 0)
            {
                int i = counter;
                while (i > 0 && commandHistory[--i].autoExecute)
                {
                    counter--;
                    Command cmdAuto = commandHistory[i];
                    cmdAuto.undo = true;
                    lastCmd = cmdAuto;
                    commandBuffer.Enqueue(cmdAuto);
                }
            }
            lastCmd.OnComplete += () => {
                EventManager.Broadcast(new EvNewState(true));
            };
            if (Const.SINGLE_UNDO)
            {
                commandHistory.Clear();
                counter = 0;
            }
            OnUndoInvoked?.Invoke();

        }
        if (cmdInProgress == null)
        {
            RunCommand();
        }
    }

    public bool IsHistoryAvailable()
    {
        return commandHistory.Count > 0;
    }

    public void RunCommand()
    {
        if (commandBuffer.Count > 0 && cmdInProgress == null)
        {
            Command cmd = commandBuffer.Dequeue();
            cmdInProgress = cmd;
            cmd.OnComplete += OnCommandFinished;
            if (cmd.undo)
            {
                cmd.Undo();
                OnActionLog?.Invoke(Const.ACTION_LOG_UNDO);
            }
            else
            {
                cmd.Execute();
                if (!cmd.omitHistory)
                    OnActionLog?.Invoke(cmd.GetActionLog());
            }
        }
        else if (commandBuffer.Count == 0 && cmdInProgress == null)
        {
            OnCommandsRunFinished?.Invoke();
        }
    }

    private void OnCommandFinished()
    {
        cmdInProgress.OnComplete -= OnCommandFinished;
        cmdInProgress = null;
        RunCommand();
    }

    public Command GetCommandInProgress()
    {
        return cmdInProgress;
    }

    public void Dispose()
    {
        foreach(Command cmd in commandBuffer)
        {
            cmd.Dispose();
        }
        commandBuffer = null;

        foreach (Command cmd in commandHistory)
        {
            cmd.Dispose();
        }
        commandHistory = null;

        cmdInProgress = null;
        OnCommandsRunFinished = null;
        OnActionLog = null;
        OnUndoInvoked = null;
    }
}
