using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;

public class MoveCardCommand : Command, ICommand
{
    ILocation source;
    ILocation dest;
    List<Card> cards;
    readonly bool runImmediately = false;
    public float customSpeed = 0.25f;
    public bool isCompleted;

    int scoreGiven = 0;
    bool isMeaningful = false; //is the moved counted by move counter

    Action showScoreClbk = null;

    public MoveCardCommand(List<Card> cardsToMove, ILocation source, ILocation destination, bool runImmediately, bool omitHistory = false)
    {
        this.cards = cardsToMove;
        this.source = source;
        this.dest = destination;
        this.runImmediately = runImmediately;
        this.omitHistory = omitHistory;
    }

    public override void Execute()
    {
        // check if move valid
        source.RemoveCards(cards);
        if (!runImmediately)
            dest.OnAddComplete += OnCompleteExecution;
        else
            dest.OnAddComplete += () => { isCompleted = true; };
        dest.AddCards(cards, customSpeed);

        if (dest is Pile && !(source is Deck))
        {
            SoundManager.Instance.Play(SoundsEnum.CARD_MOVE);
        }
        else if (dest is Foundation)
        {
            SoundManager.Instance.Play(SoundsEnum.PLACE_CARD_ON_FOUNDATION);
        }

        bool betweenFoundations = dest is Foundation && source is Foundation;
        bool kingBetweenEmptyPiles = (cards[0].Value == 13) && dest is Pile && source is Pile && ((Pile)source).FaceDownCardsCount() == 0;
        if (!betweenFoundations && !kingBetweenEmptyPiles && !omitHistory)
        {
            isMeaningful = true;
            EventManager.Broadcast(new EvAddMove());
        }

        int score = ScoreManager.CountScore(source, dest);
        EvAddScoreDelayed ev = EventManager.SyncBroadcast(new EvAddScoreDelayed(score));
        showScoreClbk = ev.showScore;
        scoreGiven = ev.score;

        if (runImmediately)
        {
            AddScore();
            OnComplete?.Invoke();
        }

        if (!omitHistory && !autoExecute)
        {
            EventManager.Broadcast(new EvNewState(false));
        }
    }

    private void AddScore()
    {
        if (showScoreClbk != null)
        {
            showScoreClbk();
            showScoreClbk = null;
        }
    }

    private void OnCompleteExecution()
    {
        AddScore();
        dest.OnAddComplete -= OnCompleteExecution;
        //source.OnAddComplete -= OnCompleteExecution;
        OnComplete?.Invoke();
        isCompleted = true;
    }

    public override void Undo()
    {
        if (isMeaningful)
        {
            EventManager.Broadcast(new EvAddMove());
        }
        showScoreClbk = EventManager.SyncBroadcast(new EvAddScoreDelayed(-scoreGiven)).showScore;
        base.Undo(); //In case the event below adds score, the substraction of undo score must happen afterwards

        SoundManager.Instance.Play(SoundsEnum.UNDO_CARD_MOVE);
        
        dest.RemoveCards(cards);
        source.OnAddComplete += OnCompleteUndoExecution;
        source.AddCards(cards);
    }

    private void OnCompleteUndoExecution()
    {
        if (showScoreClbk != null)
        {
            showScoreClbk();
            showScoreClbk = null;
        }

        source.OnAddComplete -= OnCompleteUndoExecution;
        isCompleted = true;
        OnComplete?.Invoke();
    }

    public override string GetActionLog()
    {
        return Const.ACTION_LOG_MOVE_O + source.ActionTag + dest.ActionTag;
    }

    public override void Dispose()
    {
        showScoreClbk = null;
        source = null;
        dest = null;
        cards = null;
    }
}
