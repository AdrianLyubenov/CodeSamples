namespace KingOfDestiny.Common.Data
{
    public interface IReadonlyDataProvider<out T>
    {
        T Data { get; }
    }
}