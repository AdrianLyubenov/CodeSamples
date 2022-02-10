using System;

namespace Events
{
    public struct EvChangeCardBack
    {
        public CardBackEnum cardBackEnum;

        public EvChangeCardBack(CardBackEnum value)
        {
            cardBackEnum = value;
        }
    }
    
    public struct EvChangeCardFront
    {
        public CardFrontsEnum CardFrontEnum;

        public EvChangeCardFront(CardFrontsEnum value)
        {
            CardFrontEnum = value;
        }
    }

    public struct EvEndGame { };

    public class EvScoreLock { };

    public class EvDeckPass {
        public int score;
    };
    public struct EvDeckPassUndo {
        public int score;
        public EvDeckPassUndo(int score)
        {
            this.score = score;
        }
    };
    public struct EvUndo { };

    public class EvAddScore
    {
        public int score;
        public EvAddScore(int score)
        {
            this.score = score;
        }
    }

    public class EvSyncGameplayStatus
    {
        public bool isStarted = false;
    }

    public class EvAddScoreDelayed
    {
        public int score;
        public Action showScore;
        public EvAddScoreDelayed(int score)
        {
            this.score = score;
        }
    }

    public struct EvAddMove { };

    public struct EvNoMoreMovesReset { };
    public struct EvNewState {
        public bool isUndoMove;

        public EvNewState(bool isUndoMove = false)
        {
            this.isUndoMove = isUndoMove;
        }
    };

    public class EvSendTracking
    {
        public SolitaireTrackingEvents trackingEvent;
        public string userId;
        public string userName;
    }


    public struct EvResultSectionClosed {
        public bool isPendingSection;
        public bool shown;

        public EvResultSectionClosed(bool isPendingSection, bool value)
        {
            this.isPendingSection = isPendingSection;
            this.shown = value;
        }
    }

    public struct EvShowErrorMessage
    {
        public int errorCode;
        public string message;

        public EvShowErrorMessage(int errorCode, string message = "")
        {
            this.errorCode = errorCode;
            this.message = message;
        }
    }

    public struct EvShowPopup
    {
    }

    public struct EvSaveTotalTime {}
    public struct EvShowLoader { }
    public struct EvHideLoader { }

    public struct EvShowShroud
    {
        public Action shroudTapClbk;

        public EvShowShroud(Action shroudTapClbk = null)
        {
            this.shroudTapClbk = shroudTapClbk;
        }
    }

    public struct EvHideShroud { }

    public struct EvChangeBackground
    {
        public BackgroundsEnum backgroundsEnum;
        public EvChangeBackground(BackgroundsEnum value)
        {
            backgroundsEnum = value;
        }
    }
    public struct EvGamePlayed {}

    public struct EvGameWon {}
}
