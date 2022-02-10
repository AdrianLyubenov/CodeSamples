using System;
using System.Collections.Generic;
using System.Globalization;

public class DailyChallengesLogger
{
    public static void LogGameStart(DataBank dataBank)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        FillCommonDCData(dataBank, param);
        EventManager.Broadcast(new EvSendTracking(SolitaireTrackingEvents.dailyGameStarted, param));
    }

    public static void LogGameRestart(DataBank dataBank)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        FillCommonDCData(dataBank, param);
        EventManager.Broadcast(new EvSendTracking(SolitaireTrackingEvents.dailyGameReplayed, param));
    }

    public static void LogGameWon(DataBank dataBank, ScoreContainer scoreContainer)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        FillCommonDCData(dataBank, param);
        param.Add("dateEnded", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        param.Add("score", scoreContainer.score.ToString());
        param.Add("moves", scoreContainer.movesCount.ToString());
        param.Add("time", GameHelper.GetSecondsToTimeFormat(scoreContainer.gameplayTime));
        //Typo in param name is intentional to keep consisetency with previous analytics
        param.Add("historicChallangesWon", dataBank.dailyChallengesModel.GetNumberOfAllChallengesWon().ToString());
        EventManager.Broadcast(new EvSendTracking(SolitaireTrackingEvents.dailyGameWon, param));
    }

    public static void LogCupWon(int month, int year, int prevChallengesWon, int afterChallengesWon, int daysInMonth)
    {
        if (prevChallengesWon < 10 && afterChallengesWon >= 10)
        {
            LogCupEvent(month, year, SolitaireTrackingEvents.dailyBronzeCup);
        }
        if (prevChallengesWon < 20 && afterChallengesWon >= 20)
        {
            LogCupEvent(month, year, SolitaireTrackingEvents.dailySilverCup);
        }
        if (prevChallengesWon < daysInMonth && afterChallengesWon >= daysInMonth)
        {
            LogCupEvent(month, year, SolitaireTrackingEvents.dailyGoldCup);
        }
    }

    private static void LogCupEvent(int month, int year, string eventName)
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("month", new DateTime(year, month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture));
        DateTime now = DateTime.Now;
        param.Add("withinCurrentMonth", (now.Year == year && now.Month == month) ? "true" : "false");
        EventManager.Broadcast(new EvSendTracking(eventName, param));
    }

    private static void FillCommonDCData(DataBank dataBank, Dictionary<string,string> param)
    {
        DailyChallengesModel dcModel = dataBank.dailyChallengesModel;
        param.Add("challengeDate", CalendarModel.GetDateFromDayIdx(dataBank.dailyChallengeDay).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        param.Add("oneCardMode", dataBank.DrawMode == 1 ? "true" : "false");
        param.Add("attempt", dcModel.GetChallengeState(dataBank.dailyChallengeDay).attempts.ToString());
        param.Add("dateStarted", dataBank.dailyChallengeStartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
