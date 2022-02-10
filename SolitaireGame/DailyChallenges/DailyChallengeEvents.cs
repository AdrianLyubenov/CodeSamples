namespace Events
{
    public struct EvStartChallenge
    {
        public int dayIdx;

        public EvStartChallenge(int dayIdx)
        {
            this.dayIdx = dayIdx;
        }
    }

    public struct EvDailyChallengesCancelled
    {
    }
}
