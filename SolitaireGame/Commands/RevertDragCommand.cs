public class RevertDragCommand : Command
{
    private Card card;

    private Pile sourcePile;

    public RevertDragCommand(Card card)
    {
        omitHistory = true;
        this.card = card;

        if (card.location is Pile)
        {
            sourcePile = card.location as Pile;
            sourcePile.LockPile();
        }
    }

    private void OnRevertCardCompleted()
    {
        if (sourcePile != null)
        {
            sourcePile.UnlockPile();
        }
        OnComplete?.Invoke();
    }

    public override void Execute()
    {
        card.OnRevertCardCompleted += OnRevertCardCompleted;
        card.RevertDrag();
    }

    public override void Undo()
    {
        
    }

    public override void Dispose()
    {
        sourcePile = null;
        card = null;
    }
}
