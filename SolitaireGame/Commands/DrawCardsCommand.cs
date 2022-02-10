using System.Collections.Generic;
using UnityEngine;
using Events;

public class DrawCardsCommand : Command
{
    private Deck deck;
    private Waste waste;
    private DataBank dataBank;

    private List<Card> cards = new List<Card>();
    int scoreGiven = 0;
    public string action;

    public DrawCardsCommand(Deck deck, Waste waste, DataBank dataBank)
    {
        this.deck = deck;
        this.waste = waste;
        this.dataBank = dataBank;
    }

    public override void Execute()
    {
        if (deck.cards.Count > 0)
        {
            SoundManager.Instance.Play(SoundsEnum.DRAW_CARDS);

            action = Const.ACTION_LOG_DRAW;
            bool dealOne = dataBank.DrawMode == 1 ? true : false;
            int cardsToDeal = dealOne ? 0 : Mathf.Min(2, deck.cards.Count-1);
            for (int i = cardsToDeal; i >= 0; i--) // draw 3 cards or less
            {
                Card card = deck.RemoveLast();
                card.CompleteAllAnims();
                card.SetFaceUp(true);
                cards.Add(card);
            }
            waste.OnAddComplete += OnAddComplete;
            waste.AddCards(cards);
            EventManager.Broadcast(new EvAddMove());
            EventManager.Broadcast(new EvNewState(false));
        }
        else if (waste.cards.Count > 0)
        {
            SoundManager.Instance.Play(SoundsEnum.DECK_RESHUFFLE);

            action = Const.ACTION_LOG_DRAW_BACK_TO_DECK;
            // back to deck
            waste.cards.Reverse();
            deck.OnAddComplete += OnAddComplete;
            deck.AddCards(waste.cards);
            waste.RemoveAllCards();

            scoreGiven = EventManager.SyncBroadcast(new EvDeckPass()).score;
        }

        //OnComplete?.Invoke();
    }

    private void OnAddComplete()
    {
        deck.OnAddComplete -= OnAddComplete;
        waste.OnAddComplete -= OnAddComplete;
        OnComplete?.Invoke();
    }

    public override void Undo()
    {
        if (cards.Count > 0)
        {
            SoundManager.Instance.Play(SoundsEnum.UNDO_CARD_MOVE);

            cards.Reverse();
            waste.RemoveCards(cards);
            deck.AddCards(cards);
            base.Undo();
            EventManager.Broadcast(new EvAddMove());
            EventManager.Broadcast(new EvNewState(true));
        }
        else
        {
            while(deck.cards.Count > 0)
            {
                List<Card> cardList = new List<Card>();
                bool dealOne = dataBank.DrawMode == 1 ? true : false;
                int cardsToDeal = dealOne ? 0 : Mathf.Min(2, deck.cards.Count-1);
                for (int i = cardsToDeal; i >= 0; i--) // draw 3 cards or less
                {
                    Card card = deck.RemoveLast();
                    card.CompleteAllAnims();
                    card.SetFaceUp(true);
                    cardList.Add(card);
                }
                //cardList.Reverse();
                waste.AddCards(cardList);

            }
            EventManager.Broadcast(new EvDeckPassUndo(-scoreGiven));
            EventManager.Broadcast(new EvNewState(true));
        }
        OnComplete?.Invoke();
    }

    public override string GetActionLog()
    {
        return action;
    }

    public override void Dispose()
    {
        cards = null;
        deck = null;
        waste = null;
    }
}
