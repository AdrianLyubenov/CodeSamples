using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChallengeWinType
{
    NOT_WON = 0,
    WON = 1,
    WON_TODAY = 2
}

public struct ChallengeState
{
    public ChallengeWinType winType;
    public int bestScore;
    public int attempts;
}

[Serializable]
struct ChallengeStateSerializable
{
    public int d;
    public int w;
    public int s;
    public int a;
}

[Serializable]
class ChallengeStateSerializableList
{
    public List<ChallengeStateSerializable> l = new List<ChallengeStateSerializable>();
}

public class DailyChallengesModel
{
    private Dictionary<int, ChallengeState> attemptedChallenges = new Dictionary<int, ChallengeState>();

    public ChallengeState GetChallengeState(int dayIdx)
    {
        ChallengeState challengeState;
        if (!attemptedChallenges.TryGetValue(dayIdx, out challengeState))
        {
            challengeState.winType = ChallengeWinType.NOT_WON;
            challengeState.bestScore = 0;
            challengeState.attempts = 0;
        }

        return challengeState;
    }

    public void SetChallengeWin(int dayIdx, ChallengeWinType winType, int score)
    {
        ChallengeState challengeState;
        if (attemptedChallenges.TryGetValue(dayIdx, out challengeState))
        {
            challengeState.bestScore = Math.Max(score, challengeState.bestScore);
            if (challengeState.winType == ChallengeWinType.NOT_WON)
            {
                challengeState.winType = winType;
            }
            attemptedChallenges[dayIdx] = challengeState;
        }
        else
        {
            challengeState.winType = winType;
            challengeState.bestScore = score;
            challengeState.attempts = 1;
            attemptedChallenges[dayIdx] = challengeState;
        }
    }

    public int GetNumberOfChallengesWon(int minDay, int maxDay)
    {
        int count = 0;
        for (int i = minDay; i <= maxDay; ++i)
        {
            ChallengeState challengeState;
            if (attemptedChallenges.TryGetValue(i, out challengeState))
            {
                if (challengeState.winType != ChallengeWinType.NOT_WON)
                {
                    ++count;
                }
            }
        }
        return count;
    }

    public int GetNumberOfAllChallengesWon()
    {
        int count = 0;
        foreach (ChallengeState cs in attemptedChallenges.Values)
        {
            if (cs.winType != ChallengeWinType.NOT_WON)
            {
                ++count;
            }
        }
        return count;
    }

    public void AddAttempt(int dayIdx)
    {
        ChallengeState challengeState;
        if (attemptedChallenges.TryGetValue(dayIdx, out challengeState))
        {
            challengeState.attempts++;
            attemptedChallenges[dayIdx] = challengeState;
        }
        else
        {
            challengeState.winType = ChallengeWinType.NOT_WON;
            challengeState.bestScore = 0;
            challengeState.attempts = 1;
            attemptedChallenges[dayIdx] = challengeState;
        }
    }

    public string SerializeDataToJson()
    {
        ChallengeStateSerializableList cssl = new ChallengeStateSerializableList();
        foreach (var challenge in attemptedChallenges)
        {
            ChallengeStateSerializable css;
            css.s = challenge.Value.bestScore;
            css.w = (int)challenge.Value.winType;
            css.d = challenge.Key;
            css.a = challenge.Value.attempts;
            cssl.l.Add(css);
        }
        return JsonUtility.ToJson(cssl);
    }

    public void DeserializeDataFromJson(string json)
    {
        string trimmed = json.Replace("\r", "").Replace("\n", "");
        var challenges = JsonUtility.FromJson<ChallengeStateSerializableList>(trimmed);
        attemptedChallenges.Clear();
        foreach (var challenge in challenges.l)
        {
            ChallengeState newCS;
            newCS.winType = (ChallengeWinType)challenge.w;
            newCS.bestScore = challenge.s;
            newCS.attempts = challenge.a;
            attemptedChallenges.Add(challenge.d, newCS);
        }
    }
}
