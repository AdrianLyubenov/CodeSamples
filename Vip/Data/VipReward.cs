namespace KingOfDestiny.Vip.Data
{
    public interface ICurrencyReward
    {
        string Code { get; }
        int Value { get; }
    }

    public sealed class CurrencyRewardData : ICurrencyReward
    {
        public string Code { get; }
        public int Value { get; }

        public CurrencyRewardData(string code, int value)
        {
            Code = code;
            Value = value;
        }
    }
}