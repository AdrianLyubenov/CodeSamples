using Events;

public class RevealLastCardCommand : Command
{
    private Pile pile;
    private int scoreGiven = 0;

    public RevealLastCardCommand(Pile pile)
    {
        this.pile = pile;
    }

    public override void Execute()
    {
        SoundManager.Instance.Play(SoundsEnum.REVEAL_CARD_ON_PILE);

        pile.ShowLastCard();
        scoreGiven = EventManager.SyncBroadcast(new EvAddScore(ScoreManager.RevealCardScore)).score;
        OnComplete?.Invoke();

        EventManager.Broadcast(new EvNewState(false));
    }


    public override void Undo()
    {
        SoundManager.Instance.Play(SoundsEnum.UNDO_CARD_MOVE);

        pile.HideLastCard();
        base.Undo();

        EventManager.SyncBroadcast(new EvAddScore(-scoreGiven));

        OnComplete?.Invoke();
    }

    public override void Dispose()
    {
        pile = null;
    }
}
