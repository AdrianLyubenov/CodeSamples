using AesEverywhere;
using UnityEngine;

public class ActionLogController
{
    private TimeManager timeManager;

    private string actionLog = "";
    private CommandInvoker commandInvoker;
    private ScoreManager scoreManager;
    private string logKey;
    private string UDID;
    private string gameId;
    private AES256 aes = new AES256();

    public ActionLogController(TimeManager timeManager, CommandInvoker commandInvoker, ScoreManager scoreManager, string logKey, string UDID, string gameId)
    {
        this.logKey = logKey;
        this.UDID = UDID;
        this.gameId = gameId;
        this.timeManager = timeManager;
        this.commandInvoker = commandInvoker;
        this.scoreManager = scoreManager;
        commandInvoker.OnActionLog += OnActionLogHandler;
    }

    private void OnActionLogHandler(string action)
    {
        if (action.Length > 0) // action can be 0 - e.g. on reveal last card command
        {
            if (actionLog.Length > 0)
                actionLog += ",";

            actionLog += action + Mathf.FloorToInt(timeManager.GetTimeElapsed());
            DebugUtils.Log("action: " + action + " timeLeft " + Mathf.FloorToInt(timeManager.GetTimeElapsed()) +
                           " score " + scoreManager.GetScore());
        }
    }

    public string GetEncryptedActionLog()
    {
        return aes.Encrypt(actionLog, logKey + UDID + gameId);
    }

    private string EncryptScore(int score)
    {
        return aes.Encrypt(score.ToString(), logKey);
    }

    internal void SaveActionLog()
    {
        PlayerPrefs.SetString(Const.PREFS_LAST_SCORE, EncryptScore(scoreManager.GetScore()));
        PlayerPrefs.SetString(Const.PREFS_ACTION_LOG, GetEncryptedActionLog());// actionLog);
        PlayerPrefs.Save();
    }

    public void Dispose()
    {
        scoreManager = null;
        commandInvoker = null;
        aes = null;
    }

    public void RemoveListeners()
    {
        if (commandInvoker != null)
            commandInvoker.OnActionLog -= OnActionLogHandler;
    }
}
