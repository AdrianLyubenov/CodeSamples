using UnityEngine;
using Events;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CodeStage.AntiCheat.Storage;

public class DataBank : UnityEngine.Object
{
    public int dailyChallengeDay = 0;
    public bool dailyChallengeActive = false;
    public DateTime dailyChallengeStartTime;
    public bool focusFlowInProgress = false;
    public bool isAppInitialized = false;
    public bool openDailyChallengesAfterLaunch = false;
    private DateTime optionalUpdateLastShowTime;

    public DailyChallengesModel dailyChallengesModel = new DailyChallengesModel();
    [SerializeField]
    public List<GameScoreModel> gameScoreModels = new List<GameScoreModel>();
    public AdsConfigModel adsConfigModel;
    public RateUsConfigModel rateUsConfigModel;

    private int backgroundIndex = 1;
    private int cardBackIndex = 1;
    private int cardFrontIndex = 1;
    private int drawMode = 1;
    private int soundMode = 1;

    private int gamesWon = 0;
    private int gamesPlayed = 0;
    private int bestScore = 0;
    private int bestMoves = 0;
    private int bestTime = 0;
    private long totalTime = 0;
    private long lastElapsedTime = 0;
    private int remoteConfigRateAppShow = 5;
    private bool isAppRated = false;
    private bool isDcFirstEnter = true;

    public int  CardBackIndex
    {
        get => cardBackIndex;
        set
        {
            cardBackIndex = value;
            EventManager.Broadcast(new EvChangeCardBack((CardBackEnum)value));
            PlayerPrefs.SetInt(Const.PREFS_CARD_BACK_INDEX, value);
            
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("backCardId", cardBackIndex.ToString());
            EventManager.Broadcast(new EvSendTracking(SolitaireTrackingEvents.themeBackCardChanged, additionalParams));
        }
    }
    
    public int  BackgroundIndex
    {
        get => backgroundIndex;
        set
        {
            backgroundIndex = value;
            EventManager.Broadcast(new EvChangeBackground((BackgroundsEnum)value));
            PlayerPrefs.SetInt(Const.PREFS_BACKGROUND_INDEX, value);
        }
    }
    
    public int  CardFrontIndex
    {
        get => cardFrontIndex;
        set
        {
            cardFrontIndex = value;
            EventManager.Broadcast(new EvChangeCardFront((CardFrontsEnum)value));
            PlayerPrefs.SetInt(Const.PREFS_CARD_FRONT_INDEX, value);
        }
    }

    public int DrawMode
    {
        get => drawMode;
        set
        {
            drawMode = value;
            PlayerPrefs.SetInt(Const.PREFS_DRAW_MODE, value);
            EventManager.Broadcast(new EvNoMoreMovesReset());
        }
    }

    public int SoundMode { get => soundMode;
        set {
            soundMode = value;
            PlayerPrefs.SetInt(PrefName.PREFS_SOUNDS, value);
            EventManager.Broadcast(new EvSoundChanged());
        }
    }

    public int GamesWon
    {
        get => gamesWon;
        set
        {
            gamesWon = value;
            ObscuredPrefs.Set(Const.PREFS_GAMES_WON, value);
        }
    }

    public int BestScore
    {
        get => bestScore;
        set
        {
            bestScore = value;
            ObscuredPrefs.Set(Const.PREFS_BEST_SCORE, value);
        }
    }

    public int GamesPlayed
    {
        get => gamesPlayed;
        set
        {
            gamesPlayed = value;
            PlayerPrefs.SetInt(Const.PREFS_GAMES_PLAYED, value);
        }
    }

    public int BestMoves
    {
        get => bestMoves;
        set
        {
            bestMoves = value;
            ObscuredPrefs.Set(Const.PREFS_BEST_MOVES, value);
        }
    }

    public int BestTime
    {
        get => bestTime;
        set
        {
            bestTime = value;
            ObscuredPrefs.Set(Const.PREFS_BEST_TIME, value);
        }
    }

    public long LastElapsedTime
    {
        get => lastElapsedTime;
        set => lastElapsedTime = value;
    }

    public long TotalTime
    {
        get => totalTime;
        set
        {
            totalTime = value - LastElapsedTime;
            ObscuredPrefs.Set(Const.PREFS_TOTAL_TIME, totalTime);
        }
    }

    public bool IsAppRated
    {
        get => isAppRated;
        set
        {
            isAppRated = value;
            PlayerPrefs.SetInt(Const.PREFS_IS_APP_RATED, value == true ? 1 : 0);
        }
    }

    public DateTime OptionalUpdateLastShowTime
    {
        get
        {
            return optionalUpdateLastShowTime;
        }
        set
        {
            optionalUpdateLastShowTime = value;
            PlayerPrefs.SetString(Const.PREFS_LAST_OPTIONAL_UPDATE, value.Ticks.ToString());
        }
    }

    public bool IsDcFirstEnter
    {
        get => isDcFirstEnter;
        set
        {
            isDcFirstEnter = value;
            PlayerPrefs.SetInt(Const.PREFS_IS_DC_FIRST_ENTER, value == true ? 1 : 0);
        }
    }

    public void StoreData()
    {
        ObscuredPrefs.Set<string>(Const.PREFS_GAME_SCORE_MODELS, JsonConvert.SerializeObject(gameScoreModels));
        ObscuredPrefs.Set<string>(Const.PREFS_DAILY_CHALLENGES_JSON, dailyChallengesModel.SerializeDataToJson());
        ObscuredPrefs.Save();
    }

    public void TidyScoring()
    {
        gameScoreModels.Sort(SortGameScoreModel);
        if (gameScoreModels.Count > 10)
            gameScoreModels.RemoveRange(10, gameScoreModels.Count-10);
    }

    private int SortGameScoreModel(GameScoreModel g1, GameScoreModel g2)
    {
        if (g1.score > g2.score)
            return -1;
        else if (g1.score < g2.score)
            return 1;
        else if (g1.gameplayTime < g2.gameplayTime)
            return -1;
        else if (g1.gameplayTime > g2.gameplayTime)
            return 1;
        else if (g1.moves < g2.moves)
            return -1;
        else if (g1.moves > g2.moves)
            return 1;
        else
            return g1.date.CompareTo(g2.date);
    }
}